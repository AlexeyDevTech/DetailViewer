using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Services;
using Newtonsoft.Json;
using Prism.Ioc;
using System;
using System.ComponentModel;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace DetailViewer.Views
{
    public partial class MainWindow : Window
    {
        private readonly ILogger _logger;
        private readonly IContainerProvider _containerProvider;

        public MainWindow(ILogger logger, IContainerProvider containerProvider)
        {
            _logger = logger;
            _containerProvider = containerProvider;
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var splashScreen = new SplashScreenView();
            splashScreen.Show();

            var syncService = _containerProvider.Resolve<DatabaseSyncService>();
            await syncService.SyncDatabaseAsync();

            splashScreen.Close();

            await CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            _logger.Log("Checking for updates");
            try
            {
                string versionFilePath = "file://192.168.157.29/cod2/soft/version.json";
                string content = new WebClient().DownloadString(versionFilePath);
                var remoteVersionInfo = JsonConvert.DeserializeObject<VersionInfo>(content);

                var assembly = Assembly.GetExecutingAssembly();
                var localVersion = new Version(assembly.GetName().Version.ToString());
                var remoteVersion = new Version(remoteVersionInfo.Version);

                var localVersionThreeComponents = new Version(localVersion.Major, localVersion.Minor, localVersion.Build);
                var remoteVersionThreeComponents = new Version(remoteVersion.Major, remoteVersion.Minor, remoteVersion.Build);

                if (remoteVersionThreeComponents > localVersionThreeComponents)
                {
                    var result = MessageBox.Show($"Доступна новая версия {remoteVersionInfo.Version}. Хотите установить ее?", "Обновление", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(remoteVersionInfo.DownloadUrl) { UseShellExecute = true });
                        Application.Current.Shutdown();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error checking for updates", ex);
            }
        }
    }

    public class VersionInfo
    {
        public string Version { get; set; }
        public string DownloadUrl { get; set; }
    }
}