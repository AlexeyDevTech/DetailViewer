using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class DatabaseSyncService
    {
        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private static bool _isSyncing = false;
        private static readonly object _syncLock = new object();

        public DatabaseSyncService(ISettingsService settingsService, ILogger logger)
        {
            _settingsService = settingsService;
            _logger = logger;
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

                var localDbContextFactory = new DbContextFactory(localDbPath);
                var remoteDbContextFactory = new DbContextFactory(remoteDbPath);

                using var localDbContext = localDbContextFactory.CreateDbContext();
                using var remoteDbContext = remoteDbContextFactory.CreateDbContext();

                // Шаг 1: Убедиться, что схемы баз данных совместимы
                await EnsureSchemaCompatibilityAsync(localDbContext, remoteDbContext);

                // Шаг 2: Синхронизация данных
                var lastSyncTimestamp = settings.LastSyncTimestamp;
                var localChanges = await GetChangesSince(localDbContext, lastSyncTimestamp);
                var remoteChanges = await GetChangesSince(remoteDbContext, lastSyncTimestamp);

                if (!localChanges.Any() && !remoteChanges.Any())
                {
                    _logger.LogInfo("No data changes to synchronize.");
                    return;
                }

                var (toLocal, toRemote) = ResolveChanges(localChanges, remoteChanges);

                await ApplyChangesInTransaction(remoteDbContext, toRemote, "remote");
                await ApplyChangesInTransaction(localDbContext, toLocal, "local");

                settings.LastSyncTimestamp = DateTime.UtcNow;
                await _settingsService.SaveSettingsAsync(settings);

                _logger.Log("Database synchronization finished successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("A critical error occurred during the synchronization process.", ex);
            }
            finally
            {
                ReleaseSyncLock();
            }
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

        private (List<ChangeLog> toLocal, List<ChangeLog> toRemote) ResolveChanges(List<ChangeLog> localChanges, List<ChangeLog> remoteChanges)
        {
            var changesToApplyLocally = new List<ChangeLog>(remoteChanges);
            var changesToApplyRemotely = new List<ChangeLog>(localChanges);

            var conflicts = localChanges.Select(c => new { c.EntityId, c.EntityName })
                                      .Intersect(remoteChanges.Select(c => new { c.EntityId, c.EntityName }))
                                      .ToList();

            foreach (var conflictKey in conflicts)
            {
                var latestLocal = localChanges.First(c => c.EntityId == conflictKey.EntityId && c.EntityName == conflictKey.EntityName);
                var latestRemote = remoteChanges.First(c => c.EntityId == conflictKey.EntityId && c.EntityName == conflictKey.EntityName);

                if (latestLocal.Timestamp > latestRemote.Timestamp)
                {
                    changesToApplyLocally.RemoveAll(c => c.EntityId == conflictKey.EntityId && c.EntityName == conflictKey.EntityName);
                }
                else
                {
                    changesToApplyRemotely.RemoveAll(c => c.EntityId == conflictKey.EntityId && c.EntityName == conflictKey.EntityName);
                }
            }
            return (changesToApplyLocally, changesToApplyRemotely);
        }

        private async Task ApplyChangesInTransaction(ApplicationDbContext dbContext, List<ChangeLog> changes, string dbName)
        {
            if (!changes.Any()) return;

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var change in changes.OrderBy(c => c.Timestamp))
                {
                    var entityType = GetEntityType(change.EntityName);
                    if (entityType == null) continue;

                    var entity = JsonSerializer.Deserialize(change.Payload, entityType);
                    if (entity == null) continue;

                    var existingEntity = await dbContext.FindAsync(entityType, change.EntityId);

                    switch (change.OperationType)
                    {
                        case OperationType.Create:
                            if (existingEntity == null) dbContext.Entry(entity).State = EntityState.Added;
                            break;
                        case OperationType.Update:
                            if (existingEntity != null) dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
                            else dbContext.Entry(entity).State = EntityState.Added;
                            break;
                        case OperationType.Delete:
                            if (existingEntity != null) dbContext.Entry(existingEntity).State = EntityState.Deleted;
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
            return await dbContext.ChangeLogs.AsNoTracking().Where(cl => cl.Timestamp > timestamp).ToListAsync();
        }

        private Type GetEntityType(string entityName)
        {
            var assembly = typeof(ChangeLog).Assembly;
            var type = assembly.GetType(entityName);
            if (type != null) return type;
            return assembly.GetTypes().FirstOrDefault(t => t.Name == entityName);
        }

        #endregion
    }
}
