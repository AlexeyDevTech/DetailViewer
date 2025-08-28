using DetailViewer.Core.Interfaces;
using Newtonsoft.Json;
using Prism.Ioc;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace DetailViewer.Views
{
    /// <summary>
    /// Code-behind для главного окна приложения (MainWindow).
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILogger _logger;
        private readonly IContainerProvider _containerProvider;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MainWindow"/>.
        /// </summary>
        /// <param name="logger">Сервис логирования.</param>
        /// <param name="containerProvider">Провайдер контейнера зависимостей.</param>
        public MainWindow(ILogger logger, IContainerProvider containerProvider)
        {
            _logger = logger;
            _containerProvider = containerProvider;
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// Обработчик события загрузки главного окна.
        /// </summary>
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CheckForUpdatesAsync();
        }

        /// <summary>
        /// Асинхронно проверяет наличие обновлений приложения.
        /// </summary>
        private async Task CheckForUpdatesAsync()
        {
            _logger.Log("Checking for updates");
            try
            {
                string versionFilePath = "http://192.168.157.29/cod2/soft/version.json";
                using (var httpClient = new HttpClient())
                {
                    string content = await httpClient.GetStringAsync(versionFilePath);
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
            }
            catch (Exception ex)
            {
                _logger.LogError("Error checking for updates", ex);
            }
        }
    }

    /// <summary>
    /// Представляет информацию о версии приложения.
    /// </summary>
    public class VersionInfo
    {
        /// <summary>
        /// Строка версии.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// URL для загрузки новой версии.
        /// </summary>
        public string DownloadUrl { get; set; }
    }
}
