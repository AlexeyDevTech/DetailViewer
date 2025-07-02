using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Linq;
using System.Diagnostics;

namespace DetailViewer.Modules.Explorer.ViewModels
{
    public class DashboardViewModel : BindableBase
    {
        private readonly IDocumentDataService _documentDataService;
        private readonly IDialogService _dialogService;

        private string _currentFilePath;

        private ObservableCollection<DocumentRecord> _documentRecords;
        public ObservableCollection<DocumentRecord> DocumentRecords
        {
            get { return _documentRecords; }
            set { SetProperty(ref _documentRecords, value); }
        }

        public DelegateCommand FillFormCommand { get; private set; }
        public DelegateCommand OpenTableCommand { get; private set; }
        public DelegateCommand LoadDataCommand { get; private set; }
        public DelegateCommand SaveDataCommand { get; private set; }

        public DashboardViewModel(IDocumentDataService documentDataService, IDialogService dialogService)
        {
            _documentDataService = documentDataService;
            _dialogService = dialogService;

            DocumentRecords = new ObservableCollection<DocumentRecord>();

            FillFormCommand = new DelegateCommand(FillForm);
            OpenTableCommand = new DelegateCommand(async () => await OpenTable());
            LoadDataCommand = new DelegateCommand(async () => await LoadData());
            SaveDataCommand = new DelegateCommand(async () => await SaveData());
        }

        private async Task SaveData()
        {
            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                try
                {
                    await _documentDataService.WriteRecordsAsync(_currentFilePath, DocumentRecords.ToList());
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine($"Ошибка сохранения данных: {ex.Message}");
                }
            }
            else
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                    Title = "Select Excel File to Save"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        await _documentDataService.WriteRecordsAsync(saveFileDialog.FileName, DocumentRecords.ToList());
                        _currentFilePath = saveFileDialog.FileName;
                    }
                    catch (System.Exception ex)
                    {
                        System.Console.WriteLine($"Ошибка сохранения данных: {ex.Message}");
                    }
                }
            }
        }

        private async void FillForm()
        {
            _dialogService.ShowDialog("DocumentRecordForm", new DialogParameters(), async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var newRecord = r.Parameters.GetValue<DocumentRecord>("record");
                    // Add the new record to the collection
                    DocumentRecords.Add(newRecord);
                    if (!string.IsNullOrEmpty(_currentFilePath))
                    {
                        await _documentDataService.WriteRecordsAsync(_currentFilePath, DocumentRecords.ToList());
                    }
                }
            });
        }

        private async Task OpenTable()
        {
            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                try
                {
                    var records = await _documentDataService.ReadRecordsAsync(_currentFilePath);
                    DocumentRecords.Clear();
                    foreach (var record in records)
                    {
                        DocumentRecords.Add(record);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
                }
            }
            else
            {
                await LoadData();
            }
        }

        private async Task LoadData()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Select Excel File to Load"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var records = await _documentDataService.ReadRecordsAsync(openFileDialog.FileName);
                    DocumentRecords.Clear();
                    foreach (var record in records)
                    {
                        DocumentRecords.Add(record);
                    }
                    _currentFilePath = openFileDialog.FileName;
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
                }
            }
        }
    }
}