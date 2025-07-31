
using DetailViewer.Core.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class DatabaseSyncService
    {
        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;

        public DatabaseSyncService(ISettingsService settingsService, ILogger logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        public async Task SyncDatabaseAsync()
        {
            _logger.Log("Syncing database");
            var settings = _settingsService.LoadSettings();
            var remoteDbPath = settings.DatabasePath;
            var localDbPath = settings.LocalDatabasePath;

            if (string.IsNullOrEmpty(localDbPath))
            {
                localDbPath = Path.Combine(Path.GetTempPath(), "DetailViewer", "local_database.db");
                settings.LocalDatabasePath = localDbPath;
                await _settingsService.SaveSettingsAsync(settings);
            }

            var localDbDirectory = Path.GetDirectoryName(localDbPath);
            if (!Directory.Exists(localDbDirectory))
            {
                Directory.CreateDirectory(localDbDirectory);
            }

            if (File.Exists(remoteDbPath))
            {
                File.Copy(remoteDbPath, localDbPath, true);
            }
        }
    }
}
