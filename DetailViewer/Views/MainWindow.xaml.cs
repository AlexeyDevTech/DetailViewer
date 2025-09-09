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
                // Преобразуем URL в UNC-путь для доступа к сетевой папке
                string versionFilePath = @"\\192.168.157.29\cod2\soft\version.json";
                _logger.LogInfo($"Path to search version: {versionFilePath}");

                // Проверяем, существует ли файл, перед чтением
                if (System.IO.File.Exists(versionFilePath))
                {
                    // Асинхронно читаем содержимое файла
                    string content = await System.IO.File.ReadAllTextAsync(versionFilePath);
                    var remoteVersionInfo = JsonConvert.DeserializeObject<VersionInfo>(content);

                    var assembly = Assembly.GetExecutingAssembly();
                    var localVersion = new Version(assembly.GetName().Version.ToString());
                    var remoteVersion = new Version(remoteVersionInfo.Version);

                    var localVersionThreeComponents = new Version(localVersion.Major, localVersion.Minor, localVersion.Build);

                    var remoteVersionThreeComponents = new Version(remoteVersion.Major, remoteVersion.Minor, remoteVersion.Build);

                    if (remoteVersionThreeComponents > localVersionThreeComponents)
                    {
                        var result = MessageBox.Show($"Доступна новая версия {remoteVersionInfo.Version}. Хотите установить ее ? ", "Обновление", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (result == MessageBoxResult.Yes)
                        {
                            // Убедимся, что URL для скачивания также является корректным путем
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(remoteVersionInfo.DownloadUrl)
                            { UseShellExecute = true });
                            Application.Current.Shutdown();
                        }
                    }
                    else _logger.LogInfo($"local version: {localVersion.Major}.{localVersion.Minor}.{localVersion.Build} -> remote version: {remoteVersion.Major}.{remoteVersion.Minor}.{remoteVersion.Build}");
                    
                }
                else
                {
                    _logger.LogWarning($"Update file not found at path: {versionFilePath}");
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
