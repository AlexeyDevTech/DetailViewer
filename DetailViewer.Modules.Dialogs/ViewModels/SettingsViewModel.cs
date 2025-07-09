using DetailViewer.Core.Interfaces;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class SettingsViewModel : BindableBase, IDialogAware
    {
        private readonly IDocumentDataService _documentDataService;
        private string _databasePath;

        public string DatabasePath
        {
            get { return _databasePath; }
            set { SetProperty(ref _databasePath, value); }
        }

        public DelegateCommand ImportCommand { get; private set; }
        public DelegateCommand ExportCommand { get; private set; }

        public string Title => "Настройки";

        public event Action<IDialogResult> RequestClose;

        public SettingsViewModel(IDocumentDataService documentDataService)
        {
            _documentDataService = documentDataService;
            DatabasePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "detailviewer.db");

            ImportCommand = new DelegateCommand(Import);
            ExportCommand = new DelegateCommand(Export);
        }

        private async void Import()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await _documentDataService.ImportFromExcelAsync(openFileDialog.FileName);
            }
        }

        private async void Export()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await _documentDataService.ExportToExcelAsync(saveFileDialog.FileName);
            }
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            // Clean up resources if necessary
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            // Handle parameters passed to the dialog if necessary
        }
    }
}