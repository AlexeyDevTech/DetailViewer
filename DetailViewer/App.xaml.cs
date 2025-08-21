using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Services;
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
    public partial class App
    {
        private ILogger _logger;
        private NotifyIcon _notifyIcon;
        private ISettingsService _settingsService;

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

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

            InitializeNotifyIcon();

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
            //containerRegistry.RegisterSingleton<IExcelImportService, ExcelImportService>();
            //containerRegistry.RegisterSingleton<IExcelExportService, ExcelExportService>();

            containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();

            var settingsService = Container.Resolve<ISettingsService>();
            var appSettings = settingsService.LoadSettings();
            containerRegistry.RegisterInstance(appSettings);

            // Register all services from CoreModule implicitly via the module catalog

            containerRegistry.RegisterSingleton<IClassifierService, ClassifierService>();
            containerRegistry.RegisterSingleton<IDocumentRecordService, DocumentRecordService>();
            containerRegistry.RegisterSingleton<IAssemblyService, AssemblyService>();
            containerRegistry.RegisterSingleton<IProductService, ProductService>();
            containerRegistry.RegisterSingleton<IProfileService, ProfileService>();
            containerRegistry.RegisterSingleton<IPasswordService, PasswordService>();
            containerRegistry.RegisterSingleton<IActiveUserService, ActiveUserService>();
            containerRegistry.RegisterSingleton<IApiClient, ApiClient>();
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
            _notifyIcon.Text = "Detail Viewer";

            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Заполнить форму", null, OnFillFormClick);
            _notifyIcon.ContextMenuStrip.Items.Add("Открыть программу", null, OnOpenProgramClick);
            _notifyIcon.ContextMenuStrip.Items.Add("Выход", null, OnExitClick);

            _notifyIcon.DoubleClick += OnOpenProgramClick;
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
            _logger.Log("Application exiting.");
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
