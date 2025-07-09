using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Services;
using DetailViewer.Views;
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

            // Register the DbContext
            containerRegistry.RegisterSingleton<ApplicationDbContext>();

            // Register the data service
            containerRegistry.RegisterSingleton<IDocumentDataService, SqliteDocumentDataService>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Core.CoreModule>();
            moduleCatalog.AddModule<Modules.Dialogs.DialogsModule>();
            moduleCatalog.AddModule<Modules.Explorer.ExplorerModule>();
        }
    }
}
