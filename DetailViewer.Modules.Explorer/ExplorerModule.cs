using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Services;
using DetailViewer.Modules.Explorer.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace DetailViewer.Modules.Explorer
{
    public class ExplorerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<IRegionManager>().RequestNavigate("ContentRegion", "DashboardView");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IExcelImportService, ExcelImportService>();
            containerRegistry.RegisterForNavigation<DashboardView, ViewModels.DashboardViewModel>();
            containerRegistry.RegisterForNavigation<AssembliesDashboardView, ViewModels.AssembliesDashboardViewModel>();
            containerRegistry.RegisterForNavigation<ProductsDashboardView, ViewModels.ProductsDashboardViewModel>();
        }
    }
}