using Microsoft.EntityFrameworkCore;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Services;

namespace DetailViewer.Core.Data
{
    public class ApplicationDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly ISettingsService _settingsService;
        private readonly DatabaseSyncService _syncService;

        public ApplicationDbContextFactory(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _syncService = new DatabaseSyncService(settingsService);
        }

        public ApplicationDbContext CreateDbContext()
        {
            _syncService.SyncDatabaseAsync().GetAwaiter().GetResult();

            var settings = _settingsService.LoadSettings();
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite($"Data Source={settings.LocalDatabasePath}");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
