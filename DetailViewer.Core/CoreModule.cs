using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using DetailViewer.Core.Services;
using Microsoft.EntityFrameworkCore;
using Prism.Ioc;
using Prism.Modularity;
using Unity;

namespace DetailViewer.Core
{
    public class CoreModule : IModule
    {
        private readonly IUnityContainer _container;

        public CoreModule(IUnityContainer container)
        {
          _container = container;
        }
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IClassifierProvider, ClassifierProvider>();
            containerRegistry.Register<IDocumentDataService, SqliteDocumentDataService>();
            containerRegistry.Register<IExcelImportService, ExcelImportService>();
            containerRegistry.Register<IExcelExportService, ExcelExportService>();
            containerRegistry.Register<ISettingsService, JsonSettingsService>();

            var settingsService = _container.Resolve<ISettingsService>();
            var appSettings = settingsService.LoadSettings();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite($"Data Source={appSettings.DatabasePath}")
                .Options;

            containerRegistry.RegisterInstance(options);
            containerRegistry.RegisterSingleton<ApplicationDbContext>();
        }
    }
}