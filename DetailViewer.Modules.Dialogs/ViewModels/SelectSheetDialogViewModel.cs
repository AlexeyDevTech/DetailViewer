using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.Generic;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class SelectSheetDialogViewModel : BindableBase, IDialogAware
    {
        public DelegateCommand OkCommand { get; }
        public DelegateCommand CancelCommand { get; }

        private List<string> _sheetNames;
        public List<string> SheetNames
        {
            get { return _sheetNames; }
            set { SetProperty(ref _sheetNames, value); }
        }

        private string _selectedSheet;
        public string SelectedSheet
        {
            get { return _selectedSheet; }
            set { SetProperty(ref _selectedSheet, value); }
        }

        private bool _createRelationships;
        public bool CreateRelationships
        {
            get { return _createRelationships; }
            set { SetProperty(ref _createRelationships, value); }
        }

        public SelectSheetDialogViewModel()
        {
            OkCommand = new DelegateCommand(OnOk);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        private void OnOk()
        {
            var parameters = new DialogParameters
            {
                { "selectedSheet", SelectedSheet },
                { "createRelationships", CreateRelationships }
            };
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, parameters));
        }

        private void OnCancel()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        public string Title => "Выбор листа";

        public event System.Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            SheetNames = parameters.GetValue<List<string>>("sheetNames");
        }
    }
}
