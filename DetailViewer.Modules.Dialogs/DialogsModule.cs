using DetailViewer.Modules.Dialogs.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace DetailViewer.Modules.Dialogs
{
    public class DialogsModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<DocumentRecordForm, ViewModels.DocumentRecordFormViewModel>();
            containerRegistry.RegisterDialog<SettingsView, ViewModels.SettingsViewModel>();
        }
    }
}