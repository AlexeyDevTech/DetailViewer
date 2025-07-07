using System;
using System.IO;
using System.Reflection;
using System.Windows;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using DetailViewer.Core.Services;
using DetailViewer.Views;
using Google.Apis.Sheets.v4.Data;
using Prism;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;

namespace DetailViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
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

            // Register ExcelDocumentDataService and GoogleSheetsDocumentDataService
            containerRegistry.RegisterSingleton<ExcelDocumentDataService>();
            containerRegistry.RegisterSingleton<GoogleSheetsDocumentDataService>();

            // Register the factory
            containerRegistry.RegisterSingleton<IDocumentDataServiceFactory, DocumentDataServiceFactory>();

            // Register IDocumentDataService using the factory
            containerRegistry.RegisterSingleton<IDocumentDataService>(() =>
            {
                var factory = Container.Resolve<IDocumentDataServiceFactory>();
                return factory.CreateService(appSettings.CurrentDataSourceType);
            });
        }
        protected override void ConfigureModuleCatalog(Prism.Modularity.IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Core.CoreModule>();
            moduleCatalog.AddModule<Modules.Dialogs.DialogsModule>();
            moduleCatalog.AddModule<Modules.Explorer.ExplorerModule>();
        }
    }
}
