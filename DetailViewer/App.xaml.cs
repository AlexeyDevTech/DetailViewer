using System.IO;
using System.Windows;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Services;
using DetailViewer.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace DetailViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            string logFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Logs", "app.log");
            containerRegistry.RegisterSingleton<ILogger, FileLogger>(() => new FileLogger(logFilePath));
            containerRegistry.RegisterSingleton<IDocumentDataService, ExcelDocumentDataService>();
            containerRegistry.RegisterSingleton<IDocumentFilterService, DocumentFilterService>();
            containerRegistry.RegisterSingleton<ICsvExportService, CsvExportService>();
            containerRegistry.RegisterSingleton<ISettingsService, JsonSettingsService>();
        }
        protected override void ConfigureModuleCatalog(Prism.Modularity.IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Core.CoreModule>();
            moduleCatalog.AddModule<Modules.Dialogs.DialogsModule>();
            moduleCatalog.AddModule<Modules.Explorer.ExplorerModule>();
        }
    }
}
