using DetailViewer.Core.Interfaces;

using DetailViewer.Modules.Explorer.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace DetailViewer.Modules.Explorer
{
    /// <summary>
    /// Модуль Prism, отвечающий за функциональность проводника (отображение списков деталей, сборок, продуктов).
    /// </summary>
    public class ExplorerModule : IModule
    {
        /// <summary>
        /// Вызывается после инициализации контейнера зависимостей.
        /// Осуществляет навигацию к DashboardView при запуске.
        /// </summary>
        /// <param name="containerProvider">Провайдер контейнера зависимостей.</param>
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<IRegionManager>().RequestNavigate("ContentRegion", "DashboardView");
        }

        /// <summary>
        /// Регистрирует типы в контейнере зависимостей.
        /// </summary>
        /// <param name="containerRegistry">Реестр контейнера зависимостей.</param>
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            
            containerRegistry.RegisterForNavigation<DashboardView, ViewModels.DashboardViewModel>();
            containerRegistry.RegisterForNavigation<AssembliesDashboardView, ViewModels.AssembliesDashboardViewModel>();
            containerRegistry.RegisterForNavigation<ProductsDashboardView, ViewModels.ProductsDashboardViewModel>();
        }
    }
}
