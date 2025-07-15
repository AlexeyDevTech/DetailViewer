using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Services;
using DetailViewer.Views;
using Microsoft.EntityFrameworkCore;
using Prism.Ioc;
using Prism.Modularity;
using System.IO;
using System.Reflection;
using System.Windows;

namespace DetailViewer
{
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            string logFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Logs", "app.log");
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
            dbContext.Database.Migrate();

            // Register the data service
            containerRegistry.RegisterSingleton<IDocumentDataService, SqliteDocumentDataService>();
            containerRegistry.RegisterSingleton<IProfileService, ProfileService>();
            containerRegistry.RegisterSingleton<IPasswordService, PasswordService>();
            containerRegistry.RegisterSingleton<IActiveUserService, ActiveUserService>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
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
    }
}
