using System.ComponentModel;using System.Windows;using System.IO;using System.Reflection;using Newtonsoft.Json;using System.Threading.Tasks;using System;

namespace DetailViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
            Loaded += async (s, e) => await CheckForUpdatesAsync();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // Отменяем закрытие окна
            e.Cancel = true;
            // Скрываем окно вместо закрытия
            Hide();
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                // ЗАМЕНИТЕ ПУТЬ НА ВАШ РЕАЛЬНЫЙ СЕТЕВОЙ ПУТЬ
                string versionFilePath = "\\192.168.157.29\\cod2\\soft\\version.json";
                string content = File.ReadAllText(versionFilePath);
                var remoteVersion = JsonConvert.DeserializeObject<VersionInfo>(content);

                var assembly = Assembly.GetExecutingAssembly();
                var localVersion = new Version(assembly.GetName().Version.ToString());

                if (new Version(remoteVersion.Version) > localVersion)
                {
                    var result = MessageBox.Show($"Доступна новая версия {remoteVersion.Version}. Хотите установить ее?", "Обновление", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(remoteVersion.DownloadUrl) { UseShellExecute = true });
                        Application.Current.Shutdown();
                    }
                }
            }
            catch (Exception ex)
            {
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