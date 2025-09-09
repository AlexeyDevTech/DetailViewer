using DetailViewer.Core.Interfaces;
using DetailViewer.Infrastructure.Services;

using DetailViewer.ViewModels;
using DetailViewer.Views;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Services.Dialogs;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace DetailViewer
{
    /// <summary>
    /// Главный класс приложения, отвечающий за инициализацию Prism, регистрацию сервисов и управление жизненным циклом приложения.
    /// </summary>
    public partial class App
    {
        private ILogger _logger;
        private NotifyIcon _notifyIcon;
        private ISettingsService _settingsService;

        /// <summary>
        /// Создает и возвращает главную оболочку приложения (Shell).
        /// </summary>
        /// <returns>Главное окно приложения.</returns>
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        /// <summary>
        /// Вызывается после инициализации оболочки и регистрации модулей.
        /// Выполняет дополнительную инициализацию приложения, включая отображение SplashScreen и авторизацию.
        /// </summary>
        protected override void OnInitialized()
        {
            var splashScreen = new SplashScreenView();
            splashScreen.Show();

            base.OnInitialized();

            _logger = Container.Resolve<ILogger>();
            var eventAggregator = Container.Resolve<IEventAggregator>();
            splashScreen.DataContext = new SplashScreenViewModel(eventAggregator);

            _logger.Log("Application initialization started.");

            _settingsService = Container.Resolve<ISettingsService>();

            //InitializeNotifyIcon();

            splashScreen.Close();

            var dialogService = Container.Resolve<IDialogService>();
            dialogService.ShowDialog("AuthorizationView", null, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var activeUserService = Container.Resolve<IActiveUserService>();
                    activeUserService.CurrentUser = r.Parameters.GetValue<Core.Models.Profile>("user");
                }
                else
                {
                    Current.Shutdown();
                }
            });
        }

        /// <summary>
        /// Регистрирует типы в контейнере зависимостей Prism.
        /// </summary>
        /// <param name="containerRegistry">Реестр контейнера зависимостей.</param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            string logFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Logs", "app.log");
            containerRegistry.RegisterSingleton<ILogger>(() => new FileLogger(logFilePath));

            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();

            // Register all application services
            containerRegistry.RegisterSingleton<ISettingsService, JsonSettingsService>();
            containerRegistry.RegisterSingleton<IApiClient, ApiClient>();
            containerRegistry.RegisterSingleton<IClassifierService, ClassifierService>();
            containerRegistry.RegisterSingleton<IDocumentRecordService, DocumentRecordService>();
            containerRegistry.RegisterSingleton<IAssemblyService, AssemblyService>();
            containerRegistry.RegisterSingleton<IProductService, ProductService>();
            containerRegistry.RegisterSingleton<IExcelImportService, ExcelImportService>();
            containerRegistry.RegisterSingleton<IExcelExportService, ExcelExportService>();
            containerRegistry.RegisterSingleton<IProfileService, ProfileService>();
            containerRegistry.RegisterSingleton<IPasswordService, PasswordService>();
            containerRegistry.RegisterSingleton<IActiveUserService, ActiveUserService>();
            containerRegistry.RegisterSingleton<IDocumentFilterService, DocumentFilterService>();
            containerRegistry.RegisterSingleton<ICsvExportService, CsvExportService>();
            containerRegistry.RegisterSingleton<IEskdNumberService, EskdNumberService>();

            // Load settings and register as instance
            var settingsService = Container.Resolve<ISettingsService>();
            var appSettings = settingsService.LoadSettings();
            containerRegistry.RegisterInstance(appSettings);
        }

        /// <summary>
        /// Конфигурирует каталог модулей Prism.
        /// </summary>
        /// <param name="moduleCatalog">Каталог модулей.</param>
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Core.CoreModule>();
            moduleCatalog.AddModule<Modules.Dialogs.DialogsModule>();
            moduleCatalog.AddModule<Modules.Explorer.ExplorerModule>();
        }

        /// <summary>
        /// Инициализирует иконку в области уведомлений (трее).
        /// </summary>
        private void InitializeNotifyIcon()
        {
            _logger.Log("Initializing notify icon");
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Detail Viewer";

            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Заполнить форму", null, OnFillFormClick);
            _notifyIcon.ContextMenuStrip.Items.Add("Открыть программу", null, OnOpenProgramClick);
            _notifyIcon.ContextMenuStrip.Items.Add("Выход", null, OnExitClick);

            _notifyIcon.DoubleClick += OnOpenProgramClick;
        }

        /// <summary>
        /// Обработчик события клика по пункту меню "Заполнить форму" в трее.
        /// </summary>
        private void OnFillFormClick(object sender, EventArgs e)
        {
            _logger.Log("Fill form clicked.");
            var dialogService = Container.Resolve<IDialogService>();
            var settings = _settingsService.LoadSettings();
            var parameters = new DialogParameters { { "companyCode", settings.DefaultCompanyCode } };
            dialogService.ShowDialog("DocumentRecordForm", parameters, r => { });
        }

        /// <summary>
        /// Обработчик события клика по пункту меню "Открыть программу" в трее или двойного клика по иконке.
        /// </summary>
        private void OnOpenProgramClick(object sender, EventArgs e)
        {
            _logger.Log("Open program clicked.");
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow == null)
            {
                mainWindow = Container.Resolve<MainWindow>();
                Application.Current.MainWindow = mainWindow;
            }
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
        }

        /// <summary>
        /// Обработчик события клика по пункту меню "Выход" в трее.
        /// </summary>
        private void OnExitClick(object sender, EventArgs e)
        {
            _logger.Log("Exit clicked.");
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Вызывается при завершении работы приложения.
        /// </summary>
        /// <param name="e">Аргументы события выхода.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            _logger.Log("Application exiting.");
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}