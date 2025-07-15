using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class AboutViewModel : BindableBase, IDialogAware
    {
        public string Title => "О программе";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) { }

        public string Version => "1.0.0";
        public string Description => "Интерактивный реестр записей деталей";

        public DelegateCommand CloseDialogCommand { get; }

        public AboutViewModel()
        {
            CloseDialogCommand = new DelegateCommand(() => RequestClose?.Invoke(new DialogResult(ButtonResult.OK)));
        }
    }
}
