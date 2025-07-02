using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Diagnostics;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class DocumentRecordFormViewModel : BindableBase, IDialogAware
    {
        public string Title => "Форма записи документа";

        public event Action<IDialogResult> RequestClose;

        private DocumentRecord _documentRecord;
        public DocumentRecord DocumentRecord
        {
            get { return _documentRecord; }
            set { SetProperty(ref _documentRecord, value); }
        }

        private string _eskdNumberString;
        public string ESKDNumberString
        {
            get { return _eskdNumberString; }
            set { SetProperty(ref _eskdNumberString, value); }
        }

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public DocumentRecordFormViewModel()
        {
            DocumentRecord = new DocumentRecord { Date = DateTime.Now, ESKDNumber = new ESKDNumber() };
            ESKDNumberString = DocumentRecord.ESKDNumber.GetCode();
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
        }

        private void Save()
        {
            try
            {
                DocumentRecord.ESKDNumber.SetCode(ESKDNumberString);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"Ошибка валидации: {ex.Message}");
                return;
            }

            var result = new DialogResult(ButtonResult.OK);
            result.Parameters.Add("record", DocumentRecord);
            RequestClose?.Invoke(result);
        }

        private void Cancel()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            // Clean up if necessary
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("record"))
            {
                DocumentRecord = parameters.GetValue<DocumentRecord>("record");
                ESKDNumberString = DocumentRecord.ESKDNumber.GetCode();
            }
        }
    }
}