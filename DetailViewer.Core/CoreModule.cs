using DetailViewer.Core.Interfaces;

using Prism.Ioc;
using Prism.Modularity;

namespace DetailViewer.Core
{
    public class CoreModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Registrations are now handled in the main App.xaml.cs
        }
    }
}
