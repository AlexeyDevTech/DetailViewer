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
            containerRegistry.RegisterDialog<AuthorizationView, ViewModels.AuthorizationViewModel>();
            containerRegistry.RegisterDialog<AboutView, ViewModels.AboutViewModel>();
            containerRegistry.RegisterDialog<ConfirmationDialog, ViewModels.ConfirmationDialogViewModel>();
            containerRegistry.RegisterDialog<AssemblyForm, ViewModels.AssemblyFormViewModel>();
            containerRegistry.RegisterDialog<ProductForm, ViewModels.ProductFormViewModel>();
            containerRegistry.RegisterDialog<SelectAssemblyDialog, ViewModels.SelectAssemblyDialogViewModel>();
            containerRegistry.RegisterDialog<SelectProductDialog, ViewModels.SelectProductDialogViewModel>();
        }
    }
}