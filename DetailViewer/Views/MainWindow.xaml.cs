using System.ComponentModel;
using System.Windows;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using System.Net;
using DetailViewer.Core.Interfaces;

namespace DetailViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILogger _logger;

        public MainWindow(ILogger logger)
        {
            _logger = logger;
            InitializeComponent();
            Closing += MainWindow_Closing;
            CheckForUpdatesAsync();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            _logger.Log("Main window closing");
            //// Отменяем закрытие окна
            //e.Cancel = true;
            //// Скрываем окно вместо закрытия
            //Hide();
        }

        private async Task CheckForUpdatesAsync()
        {
            _logger.Log("Checking for updates");
            try
            {
                // ЗАМЕНИТЕ ПУТЬ НА ВАШ РЕАЛЬНЫЙ СЕТЕВОЙ ПУТЬ
                string versionFilePath = "file://192.168.157.29/cod2/soft/version.json";
                string content = new WebClient().DownloadString(versionFilePath);
                var remoteVersionInfo = JsonConvert.DeserializeObject<VersionInfo>(content);

                var assembly = Assembly.GetExecutingAssembly();
                var localVersion = new Version(assembly.GetName().Version.ToString());
                var remoteVersion = new Version(remoteVersionInfo.Version);

                // Сравниваем только первые три компонента версии
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
                // Ошибки (например, нет доступа к сети) будут проигнорированы
            }
        }
    }

    public class VersionInfo
    {
        public string Version { get; set; }
        public string DownloadUrl { get; set; }
    }
}