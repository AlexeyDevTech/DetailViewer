
using System.Collections.Generic;
using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using Prism.Services.Dialogs;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class SynchronizationService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _localContextFactory;
        private readonly ISettingsService _settingsService;
        private readonly ILogger _logger;
        private readonly IDialogService _dialogService;
        private Timer _timer;

        public SynchronizationService(IDbContextFactory<ApplicationDbContext> localContextFactory, ISettingsService settingsService, ILogger logger, IDialogService dialogService)
        {
            _localContextFactory = localContextFactory;
            _settingsService = settingsService;
            _logger = logger;
            _dialogService = dialogService;
        }

        public void Start()
        {
            _timer = new Timer(async _ => await SynchronizeAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, 0);
        }

        public async Task<List<ChangeLog>> GetChangesSince(DateTime timestamp)
        {
            _logger.Log($"Getting changes since: {timestamp}");
            try
            {
                using var dbContext = await _localContextFactory.CreateDbContextAsync();
                return await dbContext.ChangeLogs
                    .Where(cl => cl.Timestamp > timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting changes", ex);
                throw;
            }
        }

        private async Task SynchronizeAsync()
        {
            var settings = _settingsService.LoadSettings();
            if (string.IsNullOrEmpty(settings.DatabasePath))
            {
                return;
            }

            try
            {
                var remoteOptionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                remoteOptionsBuilder.UseSqlite($"Data Source={settings.DatabasePath}");

                using var localDbContext = await _localContextFactory.CreateDbContextAsync();
                using var remoteDbContext = new ApplicationDbContext(remoteOptionsBuilder.Options);
                // await remoteDbContext.Database.MigrateAsync();

                var pendingChanges = await localDbContext.ChangeLogs.ToListAsync();

                foreach (var change in pendingChanges)
                {
                    try
                    {
                        var entityType = Type.GetType(change.EntityName);
                        var entityId = Convert.ToInt32(change.EntityId);

                        switch (change.OperationType)
                        {
                            case OperationType.Create:
                                var createEntity = JsonSerializer.Deserialize(change.Payload, entityType);
                                remoteDbContext.Add(createEntity);
                                await remoteDbContext.SaveChangesAsync();
                                break;

                            case OperationType.Update:
                                var localEntity = JsonSerializer.Deserialize(change.Payload, entityType);
                                var remoteEntity = await remoteDbContext.FindAsync(entityType, entityId);

                                if (remoteEntity != null)
                                {
                                    var localVersion = (byte[])localEntity.GetType().GetProperty("Version").GetValue(localEntity, null);
                                    var remoteVersion = (byte[])remoteEntity.GetType().GetProperty("Version").GetValue(remoteEntity, null);

                                    if (!localVersion.SequenceEqual(remoteVersion))
                                    {
                                        var parameters = new DialogParameters
                                        {
                                            { "localEntity", localEntity },
                                            { "remoteEntity", remoteEntity }
                                        };

                                        _dialogService.ShowDialog("ConflictResolutionView", parameters, async r =>
                                        {
                                            if (r.Result == ButtonResult.Yes) // Keep local
                                            {
                                                remoteDbContext.Entry(remoteEntity).CurrentValues.SetValues(localEntity);
                                                await remoteDbContext.SaveChangesAsync();
                                            }
                                            else if (r.Result == ButtonResult.No) // Keep remote
                                            {
                                                // Do nothing, local changes will be discarded
                                            }
                                            else // Postpone
                                            {
                                                return;
                                            }
                                        });
                                    }
                                    else
                                    {
                                        remoteDbContext.Entry(remoteEntity).CurrentValues.SetValues(localEntity);
                                        await remoteDbContext.SaveChangesAsync();
                                    }
                                }
                                break;

                            case OperationType.Delete:
                                var entityToDelete = await remoteDbContext.FindAsync(entityType, entityId);
                                if (entityToDelete != null)
                                {
                                    remoteDbContext.Remove(entityToDelete);
                                    await remoteDbContext.SaveChangesAsync();
                                }
                                break;
                        }

                        localDbContext.ChangeLogs.Remove(change);
                        await localDbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error synchronizing change {change.Id}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error connecting to remote database", ex);
            }
        }
    }
}
