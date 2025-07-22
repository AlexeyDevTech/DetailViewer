using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Services;
using DetailViewer.Views;
using Microsoft.EntityFrameworkCore;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Timers;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Prism.Services.Dialogs;
using DetailViewer.Core.Models;

namespace DetailViewer
{
    public partial class App
    {
        private NotifyIcon _notifyIcon;
        private System.Timers.Timer _dbCheckTimer;
        private ISettingsService _settingsService;

        protected override Window CreateShell()
        {
            var window = Container.Resolve<MainWindow>();
            window.Closing += OnMainWindowClosing;
            return window;
        }

        private void OnMainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var settings = _settingsService.LoadSettings();
            if (settings.RunInTray)
            {
                e.Cancel = true;
                Application.Current.MainWindow.Hide();
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            string logFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Logs", "app.log");
            containerRegistry.RegisterSingleton<ILogger>(() => new FileLogger(logFilePath));
            containerRegistry.RegisterSingleton<ISettingsService, JsonSettingsService>();
            containerRegistry.RegisterSingleton<IDocumentFilterService, DocumentFilterService>();
            containerRegistry.RegisterSingleton<ICsvExportService, CsvExportService>();

            // Register AppSettings as a singleton
            var settingsService = Container.Resolve<ISettingsService>();
            var appSettings = settingsService.LoadSettings();
            containerRegistry.RegisterInstance(appSettings);

            // Register and migrate the DbContext
            containerRegistry.RegisterSingleton<ApplicationDbContext>();
            var dbContext = Container.Resolve<ApplicationDbContext>();
            // dbContext.Database.Migrate();

            // Register the data service
            containerRegistry.RegisterSingleton<IClassifierProvider, ClassifierProvider>();
            containerRegistry.RegisterSingleton<IDocumentDataService, SqliteDocumentDataService>();
            containerRegistry.RegisterSingleton<IProfileService, ProfileService>();
            containerRegistry.RegisterSingleton<IPasswordService, PasswordService>();
            containerRegistry.RegisterSingleton<IActiveUserService, ActiveUserService>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _settingsService = Container.Resolve<ISettingsService>();
            InitializeNotifyIcon();
            InitializeDbCheckTimer();

            var dialogService = Container.Resolve<Prism.Services.Dialogs.IDialogService>();
            dialogService.ShowDialog("AuthorizationView", null, r =>
            {
                if (r.Result == Prism.Services.Dialogs.ButtonResult.OK)
                {
                    var activeUserService = Container.Resolve<IActiveUserService>();
                    activeUserService.CurrentUser = r.Parameters.GetValue<DetailViewer.Core.Models.Profile>("user");
                }
                else
                {
                    Application.Current.Shutdown();
                }
            });
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Core.CoreModule>();
            moduleCatalog.AddModule<Modules.Dialogs.DialogsModule>();
            moduleCatalog.AddModule<Modules.Explorer.ExplorerModule>();
        }

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
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
            _dbCheckTimer = new System.Timers.Timer(60 * 60 * 1000); // 60 минут
            _dbCheckTimer.Elapsed += OnDbCheckTimerElapsed;
            _dbCheckTimer.Start();
            CheckDbConnection(); // Initial check
        }

        private async void CheckDbConnection()
        {
            try
            {
                var dbContext = Container.Resolve<ApplicationDbContext>();
                bool canConnect = await dbContext.Database.CanConnectAsync();
                if (canConnect)
                {
                    _notifyIcon.Text = "Detail Viewer - Подключено к БД";
                }
                else
                {
                    _notifyIcon.Text = "Detail Viewer - Ошибка подключения к БД";
                }
            }
            catch (Exception ex)
            {
                _notifyIcon.Text = $"Detail Viewer - Ошибка: {ex.Message}";
                Container.Resolve<ILogger>().LogError($"Ошибка при проверке подключения к БД: {ex.Message}", ex);
            }
        }

        private void OnDbCheckTimerElapsed(object sender, ElapsedEventArgs e)
        {
            CheckDbConnection();
        }

        private void OnFillFormClick(object sender, EventArgs e)
        {
            var dialogService = Container.Resolve<Prism.Services.Dialogs.IDialogService>();
            var settings = _settingsService.LoadSettings();
            var parameters = new DialogParameters { { "companyCode", settings.DefaultCompanyCode } };
            dialogService.ShowDialog("DocumentRecordForm", parameters, r =>
            {
                if (r.Result == Prism.Services.Dialogs.ButtonResult.OK)
                {
                    // Optionally, refresh data in the main window if it's open
                    // This would require a mechanism to notify DashboardViewModel
                }
            });
        }

        private void OnOpenProgramClick(object sender, EventArgs e)
        {
            if (Application.Current.MainWindow == null)
            {
                // If main window is not created yet (e.g., after authorization), create it
                var mainWindow = Container.Resolve<MainWindow>();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
            }
            else
            {
                // If main window is minimized or hidden, restore it
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                Application.Current.MainWindow.Activate();
            }
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            _notifyIcon.Dispose();
            _dbCheckTimer.Dispose();
            Application.Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon?.Dispose();
            _dbCheckTimer?.Dispose();
            base.OnExit(e);
        }
    }
}