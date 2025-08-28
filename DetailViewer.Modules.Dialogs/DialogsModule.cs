using DetailViewer.Modules.Dialogs.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace DetailViewer.Modules.Dialogs
{
    /// <summary>
    /// Модуль Prism, отвечающий за регистрацию диалоговых окон приложения.
    /// </summary>
    public class DialogsModule : IModule
    {
        /// <summary>
        /// Вызывается после инициализации контейнера зависимостей.
        /// </summary>
        /// <param name="containerProvider">Провайдер контейнера зависимостей.</param>
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        /// <summary>
        /// Регистрирует типы в контейнере зависимостей.
        /// </summary>
        /// <param name="containerRegistry">Реестр контейнера зависимостей.</param>
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
            containerRegistry.RegisterDialog<SelectSheetDialog, ViewModels.SelectSheetDialogViewModel>();
            containerRegistry.RegisterDialog<ConflictResolutionView, ViewModels.ConflictResolutionViewModel>();
        }
    }
}