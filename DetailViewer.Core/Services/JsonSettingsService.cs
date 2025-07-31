using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class JsonSettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private readonly ILogger _logger;

        public JsonSettingsService(ILogger logger)
        {
            _logger = logger;
            _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DetailViewer", "settings.json");
            var settingsDirectory = Path.GetDirectoryName(_settingsFilePath);
            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }
        }

        public AppSettings LoadSettings()
        {
            _logger.Log("Loading settings");
            if (!File.Exists(_settingsFilePath))
            {
                _logger.LogInfo($"Settings file not found at {_settingsFilePath}. Returning default settings.");
                return new AppSettings(); // Return default settings if file doesn't exist
            }

            try
            {
                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                _logger.LogInfo($"Settings loaded from {_settingsFilePath}.");
                return settings ?? new AppSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading settings from {_settingsFilePath}: {ex.Message}", ex);
                return new AppSettings(); // Return default settings on error
            }
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            _logger.Log("Saving settings");
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_settingsFilePath, json);
                _logger.LogInfo($"Settings saved to {_settingsFilePath}.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving settings to {_settingsFilePath}: {ex.Message}", ex);
                throw; // Re-throw to propagate the error
            }
        }
    }
}
