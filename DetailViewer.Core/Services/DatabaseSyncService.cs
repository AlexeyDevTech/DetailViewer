using DetailViewer.Core.Data;
using DetailViewer.Core.Events;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class DatabaseSyncService
    {
        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IEventAggregator _eventAggregator;
        private static bool _isSyncing = false;
        private static readonly object _syncLock = new object();

        public DatabaseSyncService(ISettingsService settingsService, ILogger logger, IDialogService dialogService, IEventAggregator eventAggregator)
        {
            _settingsService = settingsService;
            _logger = logger;
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
        }

        public async Task SyncDatabaseAsync()
        {
            if (!TryAcquireSyncLock()) return;

            try
            {
                _logger.Log("Starting database synchronization process.");
                var settings = _settingsService.LoadSettings();
                var remoteDbPath = settings.DatabasePath;
                var localDbPath = settings.LocalDatabasePath;

                if (!ArePathsConfigured(localDbPath, remoteDbPath)) return;

                var isFirstSync = !File.Exists(localDbPath);

                var dbContextFactory = new ApplicationDbContextFactory(_settingsService);
                using var localDbContext = dbContextFactory.CreateDbContext();
                localDbContext.Database.EnsureCreated();

                if (isFirstSync)
                {
                    _logger.LogInfo("First sync detected. Performing initial data population from remote database.");
                    using var remoteDbContextForCopy = dbContextFactory.CreateRemoteDbContext();
                    await PerformInitialBulkCopy(localDbContext, remoteDbContextForCopy);

                    settings.LastSyncTimestamp = DateTime.UtcNow;
                    await _settingsService.SaveSettingsAsync(settings);
                    _logger.LogInfo("Initial sync complete.");
                    _eventAggregator.GetEvent<SyncCompletedEvent>().Publish();
                    return; 
                }

                using var remoteDbContext = dbContextFactory.CreateRemoteDbContext();

                await EnsureSchemaCompatibilityAsync(localDbContext, remoteDbContext);

                var lastSyncTimestamp = settings.LastSyncTimestamp;
                
                var localChanges = await GetChangesSince(localDbContext, lastSyncTimestamp);
                var remoteChanges = await GetChangesSince(remoteDbContext, lastSyncTimestamp);

                if (!localChanges.Any() && !remoteChanges.Any())
                {
                    _logger.LogInfo("No data changes to synchronize.");
                    return;
                }

                var (toLocal, toRemote, conflicts) = await ResolveChanges(localChanges, remoteChanges, localDbContext);

                if (conflicts.Any())
                {
                    await HandleConflicts(conflicts, localDbContext, remoteDbContext);
                }

                await ApplyChangesInTransaction(remoteDbContext, toRemote, "remote");
                await ApplyChangesInTransaction(localDbContext, toLocal, "local");

                settings.LastSyncTimestamp = DateTime.UtcNow;
                await _settingsService.SaveSettingsAsync(settings);

                _logger.Log("Database synchronization finished successfully.");
                _eventAggregator.GetEvent<SyncCompletedEvent>().Publish();
            }
            catch (Exception ex)
            {
                _logger.LogError("A critical error occurred during the synchronization process.", ex);
            }
            finally
            {
                _logger.Log("Synchronization process finished. Releasing lock and notifying UI.");
                _eventAggregator.GetEvent<SyncCompletedEvent>().Publish();
                ReleaseSyncLock();
            }
        }

        private async Task PerformInitialBulkCopy(ApplicationDbContext localContext, ApplicationDbContext remoteContext)
        {
            _logger.LogInfo("Performing initial bulk data copy from remote to local.");
            _eventAggregator.GetEvent<StatusUpdateEvent>().Publish("Загрузка классификаторов...");
            await using var transaction = await localContext.Database.BeginTransactionAsync();
            try
            {
                var classifiers = await remoteContext.Classifiers.AsNoTracking().ToListAsync();
                _logger.LogInfo($"Read {classifiers.Count} classifiers from remote DB.");
                await localContext.Classifiers.AddRangeAsync(classifiers);
                await localContext.SaveChangesAsync();

                _eventAggregator.GetEvent<StatusUpdateEvent>().Publish("Загрузка номеров ЕСКД...");
                var eskdNumbers = await remoteContext.ESKDNumbers.AsNoTracking().ToListAsync();
                _logger.LogInfo($"Read {eskdNumbers.Count} ESKD numbers from remote DB.");
                await localContext.ESKDNumbers.AddRangeAsync(eskdNumbers);
                await localContext.SaveChangesAsync();

                _eventAggregator.GetEvent<StatusUpdateEvent>().Publish("Загрузка продуктов...");
                var products = await remoteContext.Products.AsNoTracking().ToListAsync();
                _logger.LogInfo($"Read {products.Count} products from remote DB.");
                await localContext.Products.AddRangeAsync(products);
                await localContext.SaveChangesAsync();

                _eventAggregator.GetEvent<StatusUpdateEvent>().Publish("Загрузка сборок...");
                var assemblies = await remoteContext.Assemblies.AsNoTracking().ToListAsync();
                _logger.LogInfo($"Read {assemblies.Count} assemblies from remote DB.");
                await localContext.Assemblies.AddRangeAsync(assemblies);
                await localContext.SaveChangesAsync();

                _eventAggregator.GetEvent<StatusUpdateEvent>().Publish("Загрузка записей документов...");
                var documentRecords = await remoteContext.DocumentRecords.AsNoTracking().ToListAsync();
                _logger.LogInfo($"Read {documentRecords.Count} document records from remote DB.");
                await localContext.DocumentRecords.AddRangeAsync(documentRecords);
                await localContext.SaveChangesAsync();

                _eventAggregator.GetEvent<StatusUpdateEvent>().Publish("Загрузка профилей...");
                var profiles = await remoteContext.Profiles.AsNoTracking().ToListAsync();
                _logger.LogInfo($"Read {profiles.Count} profiles from remote DB.");
                await localContext.Profiles.AddRangeAsync(profiles);
                await localContext.SaveChangesAsync();

                _eventAggregator.GetEvent<StatusUpdateEvent>().Publish("Загрузка деталей сборок...");
                var assemblyDetails = await remoteContext.AssemblyDetails.AsNoTracking().ToListAsync();
                _logger.LogInfo($"Read {assemblyDetails.Count} assembly details from remote DB.");
                await localContext.AssemblyDetails.AddRangeAsync(assemblyDetails);
                await localContext.SaveChangesAsync();

                _eventAggregator.GetEvent<StatusUpdateEvent>().Publish("Загрузка сборок продуктов...");
                var productAssemblies = await remoteContext.ProductAssemblies.AsNoTracking().ToListAsync();
                _logger.LogInfo($"Read {productAssemblies.Count} product assemblies from remote DB.");
                await localContext.ProductAssemblies.AddRangeAsync(productAssemblies);
                await localContext.SaveChangesAsync();

                _eventAggregator.GetEvent<StatusUpdateEvent>().Publish("Загрузка родительских сборок...");
                var assemblyParents = await remoteContext.AssemblyParents.AsNoTracking().ToListAsync();
                _logger.LogInfo($"Read {assemblyParents.Count} assembly parents from remote DB.");
                await localContext.AssemblyParents.AddRangeAsync(assemblyParents);

                await localContext.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInfo("Initial bulk data copy completed successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("Error during initial bulk data copy.", ex);
                throw;
            }
        }

        private async Task HandleConflicts(List<ConflictLog> conflicts, ApplicationDbContext localDbContext, ApplicationDbContext remoteDbContext)
        {
            foreach (var conflict in conflicts)
            {
                var localEntityType = GetEntityType(conflict.EntityName);
                var localEntity = JsonSerializer.Deserialize(conflict.LocalPayload, localEntityType);

                var remoteEntityType = GetEntityType(conflict.EntityName);
                var remoteEntity = JsonSerializer.Deserialize(conflict.RemotePayload, remoteEntityType);

                var parameters = new DialogParameters
                {
                    { "localEntity", localEntity },
                    { "remoteEntity", remoteEntity }
                };

                var tcs = new TaskCompletionSource<IDialogResult>();
                _dialogService.ShowDialog("ConflictResolutionView", parameters, r => tcs.SetResult(r));
                var result = await tcs.Task;

                if (result.Result == ButtonResult.Yes) // Keep local
                {
                    var changeLog = new ChangeLog
                    {
                        EntityName = conflict.EntityName,
                        EntityId = conflict.EntityId,
                        OperationType = OperationType.Update, // Or determine dynamically
                        Payload = conflict.LocalPayload,
                        Timestamp = DateTime.UtcNow
                    };
                    await ApplyChangesInTransaction(remoteDbContext, new List<ChangeLog> { changeLog }, "remote");
                }
                else if (result.Result == ButtonResult.No) // Keep remote
                {
                    var changeLog = new ChangeLog
                    {
                        EntityName = conflict.EntityName,
                        EntityId = conflict.EntityId,
                        OperationType = OperationType.Update, // Or determine dynamically
                        Payload = conflict.RemotePayload,
                        Timestamp = DateTime.UtcNow
                    };
                    await ApplyChangesInTransaction(localDbContext, new List<ChangeLog> { changeLog }, "local");
                }

                localDbContext.ConflictLogs.Remove(conflict);
                await localDbContext.SaveChangesAsync();
            }
        }

        private async Task<(List<ChangeLog> toLocal, List<ChangeLog> toRemote, List<ConflictLog> conflicts)> ResolveChanges(List<ChangeLog> localChanges, List<ChangeLog> remoteChanges, ApplicationDbContext localDbContext)
        {
            var changesToApplyLocally = new List<ChangeLog>(remoteChanges);
            var changesToApplyRemotely = new List<ChangeLog>(localChanges);
            var conflicts = new List<ConflictLog>();

            var conflictKeys = localChanges.Select(c => new { c.EntityId, c.EntityName })
                                      .Intersect(remoteChanges.Select(c => new { c.EntityId, c.EntityName }))
                                      .ToList();

            foreach (var conflictKey in conflictKeys)
            {
                var localChange = localChanges.First(c => c.EntityId == conflictKey.EntityId && c.EntityName == conflictKey.EntityName);
                var remoteChange = remoteChanges.First(c => c.EntityId == conflictKey.EntityId && c.EntityName == conflictKey.EntityName);

                var conflict = new ConflictLog
                {
                    EntityName = localChange.EntityName,
                    EntityId = localChange.EntityId,
                    LocalPayload = localChange.Payload,
                    RemotePayload = remoteChange.Payload,
                    Timestamp = DateTime.UtcNow
                };

                conflicts.Add(conflict);
                await localDbContext.ConflictLogs.AddAsync(conflict);

                changesToApplyLocally.RemoveAll(c => c.EntityId == conflictKey.EntityId && c.EntityName == conflictKey.EntityName);
                changesToApplyRemotely.RemoveAll(c => c.EntityId == conflictKey.EntityId && c.EntityName == conflictKey.EntityName);
            }

            await localDbContext.SaveChangesAsync();
            return (changesToApplyLocally, changesToApplyRemotely, conflicts);
        }

        private async Task ApplyChangesInTransaction(ApplicationDbContext dbContext, List<ChangeLog> changes, string dbName)
        {
            if (!changes.Any()) return;

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // Group changes by entity to process them in a more controlled manner
                var changesByEntity = changes.GroupBy(c => new { c.EntityName, c.EntityId })
                                             .Select(g => g.OrderByDescending(c => c.Timestamp).First())
                                             .ToList();

                // Define the order of operations based on entity dependencies
                var entityOrder = new List<string>
                {
                    nameof(Classifier),
                    nameof(ESKDNumber),
                    nameof(Profile),
                    nameof(Product),
                    nameof(Models.Assembly),
                    nameof(DocumentDetailRecord),
                    nameof(ProductAssembly),
                    nameof(AssemblyDetail),
                    nameof(AssemblyParent)
                };

                var sortedChanges = changesByEntity
                    .OrderBy(c => c.OperationType == OperationType.Delete ? 1 : 0) // Deletes first
                    .ThenBy(c => entityOrder.IndexOf(c.EntityName))
                    .ThenBy(c => c.Timestamp);

                foreach (var change in sortedChanges)
                {
                    var entityType = GetEntityType(change.EntityName);
                    if (entityType == null)
                    {
                        _logger.LogWarning($"Could not find entity type for '{change.EntityName}'. Skipping change.");
                        continue;
                    }

                    var entity = JsonSerializer.Deserialize(change.Payload, entityType, options);
                    if (entity == null)
                    {
                        _logger.LogWarning($"Could not deserialize payload for entity '{change.EntityName}' with id '{change.EntityId}'. Skipping change.");
                        continue;
                    }

                    if (!int.TryParse(change.EntityId, out var entityId))
                    {
                        _logger.LogWarning($"Could not parse EntityId '{change.EntityId}' to an integer for entity type '{change.EntityName}'. Skipping change.");
                        continue;
                    }

                    var existingEntity = await dbContext.FindAsync(entityType, entityId);

                    switch (change.OperationType)
                    {
                        case OperationType.Create:
                            if (existingEntity == null)
                            {
                                dbContext.Add(entity);
                            }
                            break;
                        case OperationType.Update:
                            if (existingEntity != null)
                            {
                                dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
                            }
                            else
                            {
                                // If the entity doesn't exist, it might have been deleted and recreated.
                                // Treat it as a creation.
                                dbContext.Add(entity);
                            }
                            break;
                        case OperationType.Delete:
                            if (existingEntity != null)
                            {
                                dbContext.Remove(existingEntity);
                            }
                            break;
                    }
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Failed to apply changes to {dbName} database. Transaction rolled back.", ex);
                throw;
            }
        }

        #region Helper Methods

        private bool TryAcquireSyncLock()
        {
            lock (_syncLock)
            {
                if (_isSyncing)
                {
                    _logger.LogWarning("Synchronization is already in progress. Skipping this run.");
                    return false;
                }
                _isSyncing = true;
                return true;
            }
        }

        private void ReleaseSyncLock()
        {
            lock (_syncLock)
            {
                _isSyncing = false;
            }
        }

        private bool ArePathsConfigured(string localDbPath, string remoteDbPath)
        {
            if (string.IsNullOrEmpty(localDbPath) || string.IsNullOrEmpty(remoteDbPath))
            {
                _logger.LogError("Local or remote database path is not configured.");
                return false;
            }
            if (!File.Exists(remoteDbPath))
            {
                _logger.LogWarning($"Remote database not found at {remoteDbPath}, skipping sync.");
                return false;
            }
            return true;
        }

        private async Task<List<ChangeLog>> GetChangesSince(ApplicationDbContext dbContext, DateTime timestamp)
        {
            var changes = await dbContext.ChangeLogs.AsNoTracking().Where(cl => cl.Timestamp > timestamp).ToListAsync();
            _logger.LogInfo($"Found {changes.Count} changes in {dbContext.Database.GetDbConnection().DataSource} since {timestamp}");
            return changes;
        }

        private Type GetEntityType(string entityName)
        {
            var assembly = typeof(ChangeLog).Assembly;
            var type = assembly.GetType(entityName);
            if (type != null) return type;
            return assembly.GetTypes().FirstOrDefault(t => t.Name == entityName);
        }

        private async Task EnsureSchemaCompatibilityAsync(ApplicationDbContext localContext, ApplicationDbContext remoteContext)
        {
            _logger.LogInfo("Checking for schema compatibility...");

            var localMigrations = await localContext.Database.GetPendingMigrationsAsync();
            if (localMigrations.Any())
            {
                _logger.LogWarning($"Local database schema is outdated. Applying {localMigrations.Count()} migrations...");
                await localContext.Database.MigrateAsync();
                _logger.LogInfo("Local database schema updated successfully.");
            }

            var remoteMigrations = await remoteContext.Database.GetPendingMigrationsAsync();
            if (remoteMigrations.Any())
            {
                _logger.LogError($"CRITICAL: Remote (shared) database schema is outdated and requires manual migration. Halting synchronization.");
                throw new Exception("Remote database schema is outdated. Please contact administrator.");
            }

            _logger.LogInfo("Database schemas are compatible.");
        }

        #endregion
    }
}
