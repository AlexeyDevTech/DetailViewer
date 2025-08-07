using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Services;
using DetailViewer.ViewModels;
using DetailViewer.Views;
using Microsoft.EntityFrameworkCore;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Services.Dialogs;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace DetailViewer
{
    public partial class App
    {
        private ILogger _logger;
        private NotifyIcon _notifyIcon;
        private System.Timers.Timer _dbCheckTimer;
        private ISettingsService _settingsService;
        private SynchronizationService _synchronizationService;

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override async void OnInitialized()
        {
            var splashScreen = new SplashScreenView();
            splashScreen.Show();

            base.OnInitialized();

            _logger = Container.Resolve<ILogger>();
            var eventAggregator = Container.Resolve<IEventAggregator>();
            splashScreen.DataContext = new SplashScreenViewModel(eventAggregator);

            _logger.Log("Application initialization started.");

            await Task.Run(async () =>
            {
                var dbContextFactory = Container.Resolve<IDbContextFactory<ApplicationDbContext>>();
                using (var dbContext = dbContextFactory.CreateDbContext())
                {
                    await dbContext.Database.EnsureCreatedAsync();
                }

                var syncService = Container.Resolve<DatabaseSyncService>();
                await syncService.SyncDatabaseAsync();
            });

            _settingsService = Container.Resolve<ISettingsService>();
            _synchronizationService = Container.Resolve<SynchronizationService>();
            _synchronizationService.Start();

            InitializeNotifyIcon();
            InitializeDbCheckTimer();

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

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            string logFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Logs", "app.log");
            containerRegistry.RegisterSingleton<ILogger>(() => new FileLogger(logFilePath));
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<ISettingsService, JsonSettingsService>();
            containerRegistry.RegisterSingleton<IDocumentFilterService, DocumentFilterService>();
            containerRegistry.RegisterSingleton<ICsvExportService, CsvExportService>();
            containerRegistry.RegisterSingleton<IExcelImportService, ExcelImportService>();
            containerRegistry.RegisterSingleton<IExcelExportService, ExcelExportService>();

            containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();

            var settingsService = Container.Resolve<ISettingsService>();
            var appSettings = settingsService.LoadSettings();
            containerRegistry.RegisterInstance(appSettings);

            containerRegistry.RegisterSingleton<IClassifierService, ClassifierService>();
            containerRegistry.RegisterSingleton<IDocumentRecordService, DocumentRecordService>();
            containerRegistry.RegisterSingleton<IAssemblyService, AssemblyService>();
            containerRegistry.RegisterSingleton<IProductService, ProductService>();
            containerRegistry.RegisterSingleton<IProfileService, ProfileService>();
            containerRegistry.RegisterSingleton<IPasswordService, PasswordService>();
            containerRegistry.RegisterSingleton<IActiveUserService, ActiveUserService>();

            containerRegistry.RegisterSingleton<DatabaseSyncService>();
            containerRegistry.Register<IDbContextFactory<ApplicationDbContext>>(() => new ApplicationDbContextFactory(Container.Resolve<ISettingsService>()));
            containerRegistry.RegisterSingleton<SynchronizationService>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Core.CoreModule>();
            moduleCatalog.AddModule<Modules.Dialogs.DialogsModule>();
            moduleCatalog.AddModule<Modules.Explorer.ExplorerModule>();
        }

        private void InitializeNotifyIcon()
        {
            _logger.Log("Initializing notify icon");
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Detail Viewer - Загрузка...";

            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Заполнить форму", null, OnFillFormClick);
            _notifyIcon.ContextMenuStrip.Items.Add("Открыть программу", null, OnOpenProgramClick);
            _notifyIcon.ContextMenuStrip.Items.Add("Выход", null, OnExitClick);

            _notifyIcon.DoubleClick += OnOpenProgramClick;
        }

        private void InitializeDbCheckTimer()
        {
            _logger.Log("Initializing DB check timer");
            _dbCheckTimer = new System.Timers.Timer(60 * 60 * 1000); // 60 minutes
            _dbCheckTimer.Elapsed += OnDbCheckTimerElapsed;
            _dbCheckTimer.Start();
            CheckDbConnection(); // Initial check
        }

        private async void CheckDbConnection()
        {
            _logger.Log("Checking DB connection");
            try
            {
                using var dbContext = Container.Resolve<IDbContextFactory<ApplicationDbContext>>().CreateDbContext();
                bool canConnect = await dbContext.Database.CanConnectAsync();
                _notifyIcon.Text = canConnect ? "Detail Viewer - Подключено к БД" : "Detail Viewer - Ошибка подключения к БД";
            }
            catch (Exception ex)
            {
                _notifyIcon.Text = $"Detail Viewer - Ошибка: {ex.Message}";
                _logger.LogError($"Ошибка при проверке подключения к БД: {ex.Message}", ex);
            }
        }

        private void OnDbCheckTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _logger.Log("DB check timer elapsed");
            CheckDbConnection();
        }

        private void OnFillFormClick(object sender, EventArgs e)
        {
            _logger.Log("Fill form clicked");
            var dialogService = Container.Resolve<IDialogService>();
            var settings = _settingsService.LoadSettings();
            var parameters = new DialogParameters { { "companyCode", settings.DefaultCompanyCode } };
            dialogService.ShowDialog("DocumentRecordForm", parameters, r => { });
        }

        private void OnOpenProgramClick(object sender, EventArgs e)
        {
            _logger.Log("Open program clicked");
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

        private void OnExitClick(object sender, EventArgs e)
        {
            _logger.Log("Exit clicked");
            Application.Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger.Log("Application exiting");
            _synchronizationService?.Stop();
            _notifyIcon?.Dispose();
            _dbCheckTimer?.Dispose();
            base.OnExit(e);
        }
    }
}