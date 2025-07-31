
using DetailViewer.Core.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class ConflictResolutionViewModel : BindableBase, IDialogAware
    {
        private readonly ILogger _logger;
        private object _localEntity;
        public object LocalEntity
        {
            get { return _localEntity; }
            set { SetProperty(ref _localEntity, value); }
        }

        private object _remoteEntity;
        public object RemoteEntity
        {
            get { return _remoteEntity; }
            set { SetProperty(ref _remoteEntity, value); }
        }

        public DelegateCommand KeepLocalCommand { get; }
        public DelegateCommand KeepRemoteCommand { get; }
        public DelegateCommand PostponeCommand { get; }

        public ConflictResolutionViewModel(ILogger logger)
        {
            _logger = logger;
            KeepLocalCommand = new DelegateCommand(() => CloseDialog(ButtonResult.Yes));
            KeepRemoteCommand = new DelegateCommand(() => CloseDialog(ButtonResult.No));
            PostponeCommand = new DelegateCommand(() => CloseDialog(ButtonResult.Cancel));
        }

        private void CloseDialog(ButtonResult result)
        {
            RequestClose?.Invoke(new DialogResult(result));
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            _logger.Log("Dialog opened: ConflictResolution");
            LocalEntity = parameters.GetValue<object>("localEntity");
            RemoteEntity = parameters.GetValue<object>("remoteEntity");
        }

        public string Title => "Разрешение конфликта";

        public event Action<IDialogResult> RequestClose;
    }
}
