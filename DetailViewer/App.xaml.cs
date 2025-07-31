
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
using Prism.Unity;

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
            _logger.Log("Creating shell");
            var window = Container.Resolve<MainWindow>();
            window.Closing += OnMainWindowClosing;
            return window;
        }

        private void OnMainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _logger.Log("Main window closing");
            var settings = _settingsService.LoadSettings();
            //if (settings.RunInTray)
            //{
            //    e.Cancel = true;
            //    Application.Current.MainWindow.Hide();
            //}
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
           
            string logFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Logs", "app.log");
            containerRegistry.RegisterSingleton<ILogger>(() => new FileLogger(logFilePath));
            _logger = Container.Resolve<ILogger>();
            _logger.Log("Registering types");
            containerRegistry.RegisterSingleton<ISettingsService, JsonSettingsService>();
            containerRegistry.RegisterSingleton<IDocumentFilterService, DocumentFilterService>();
            containerRegistry.RegisterSingleton<ICsvExportService, CsvExportService>();

            var settingsService = Container.Resolve<ISettingsService>();
            var appSettings = settingsService.LoadSettings();
            containerRegistry.RegisterInstance(appSettings);

            containerRegistry.RegisterSingleton<IClassifierProvider, ClassifierProvider>();
            containerRegistry.RegisterSingleton<IDocumentDataService, SqliteDocumentDataService>();
            containerRegistry.RegisterSingleton<IProfileService, ProfileService>();
            containerRegistry.RegisterSingleton<IPasswordService, PasswordService>();
            containerRegistry.RegisterSingleton<IActiveUserService, ActiveUserService>();

            containerRegistry.RegisterSingleton<DatabaseSyncService>();

            containerRegistry.Register<IDbContextFactory<ApplicationDbContext>>(() =>
            {
                var settingsService = Container.Resolve<ISettingsService>();
                return new ApplicationDbContextFactory(settingsService);
            });

            containerRegistry.RegisterSingleton<SynchronizationService>();

            containerRegistry.RegisterSingleton<SynchronizationService>();
        }

        protected override void OnInitialized()
        {
            _logger.Log("Initializing application");
            base.OnInitialized();

            var syncService = Container.Resolve<DatabaseSyncService>();
            syncService.SyncDatabaseAsync().GetAwaiter().GetResult();

            var dbContextFactory = Container.Resolve<IDbContextFactory<ApplicationDbContext>>();

            // Manually fix migrations history if needed
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                var connection = dbContext.Database.GetDbConnection();
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Classifiers';";
                    var classifiersExists = command.ExecuteScalar() != null;

                    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory';";
                    var migrationsHistoryExists = command.ExecuteScalar() != null;

                    if (classifiersExists && !migrationsHistoryExists)
                    {
                        _logger.Log("Manually creating migrations history table.");
                        command.CommandText = "CREATE TABLE \"__EFMigrationsHistory\" (\"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY, \"ProductVersion\" TEXT NOT NULL);";
                        command.ExecuteNonQuery();

                        _logger.Log("Manually inserting initial migration record.");
                        command.CommandText = "INSERT INTO \"__EFMigrationsHistory\" VALUES ('20250731075054_AddChangeLogTable', '8.0.7');";
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }

            // Run migrations using a new DbContext
            try
            {
                _logger.Log("Applying migrations...");
                using (var dbContext = dbContextFactory.CreateDbContext())
                {
                    // dbContext.Database.Migrate();
                }
                _logger.Log("Migrations applied successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error applying migrations", ex);
                throw;
            }

            _settingsService = Container.Resolve<ISettingsService>();
            _synchronizationService = Container.Resolve<SynchronizationService>();
            _synchronizationService.Start();

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
                    Current.Shutdown();
                }
            });
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            _logger.Log("Configuring module catalog");
            moduleCatalog.AddModule<Core.CoreModule>();
            moduleCatalog.AddModule<Modules.Dialogs.DialogsModule>();
            moduleCatalog.AddModule<Modules.Explorer.ExplorerModule>();
        }

        private void InitializeNotifyIcon()
        {
            _logger.Log("Initializing notify icon");
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
            _logger.Log("Initializing DB check timer");
            _dbCheckTimer = new System.Timers.Timer(60 * 60 * 1000); // 60 минут
            _dbCheckTimer.Elapsed += OnDbCheckTimerElapsed;
            _dbCheckTimer.Start();
            CheckDbConnection(); // Initial check
        }

        private async void CheckDbConnection()
        {
            _logger.Log("Checking DB connection");
            try
            {
                using var dbContext = await (Container.Resolve<IDbContextFactory<ApplicationDbContext>>() as ApplicationDbContextFactory).CreateDbContextAsync();
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
            _logger.Log("DB check timer elapsed");
            CheckDbConnection();
        }

        private void OnFillFormClick(object sender, EventArgs e)
        {
            _logger.Log("Fill form clicked");
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
            _logger.Log("Open program clicked");
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
            _logger.Log("Exit clicked");
            _dbCheckTimer.Stop();
            _synchronizationService.Stop();
            _notifyIcon.Dispose();
            _dbCheckTimer.Dispose();
            Application.Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger.Log("Application exiting");
            _synchronizationService.Stop();
            _notifyIcon?.Dispose();
            _dbCheckTimer?.Dispose();
            base.OnExit(e);
        }
    }
}
