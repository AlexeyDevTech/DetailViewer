using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace DetailViewer.Modules.Explorer.ViewModels
{
    public class DashboardViewModel : BindableBase
    {
        private readonly IDocumentDataService _documentDataService;
        private readonly IDialogService _dialogService;
        private ILogger _logger;

        private string _statusText;
        public string StatusText
        {
            get { return _statusText; }
            set { SetProperty(ref _statusText, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        private ObservableCollection<DocumentRecord> _documentRecords;
        public ObservableCollection<DocumentRecord> DocumentRecords
        {
            get { return _documentRecords; }
            set { SetProperty(ref _documentRecords, value); }
        }

        public DelegateCommand FillFormCommand { get; private set; }
        public DelegateCommand LoadDataCommand { get; private set; }

        public DashboardViewModel(IDocumentDataService documentDataService, IDialogService dialogService, ILogger logger)
        {
            _documentDataService = documentDataService;
            _dialogService = dialogService;
            _logger = logger;

            DocumentRecords = new ObservableCollection<DocumentRecord>();
            StatusText = "Готово";

            FillFormCommand = new DelegateCommand(FillForm);
            LoadDataCommand = new DelegateCommand(async () => await LoadData());
        }

        private void FillForm()
        {
            _dialogService.ShowDialog("DocumentRecordForm", new DialogParameters(), async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var newRecord = r.Parameters.GetValue<DocumentRecord>("record");
                    await _documentDataService.AddRecordAsync(newRecord);
                    await LoadData();
                }
            });
        }

        private async Task LoadData()
        {
            IsBusy = true;
            StatusText = "Загрузка данных...";
            try
            {
                var records = await _documentDataService.GetAllRecordsAsync();
                DocumentRecords.Clear();
                foreach (var record in records)
                {
                    DocumentRecords.Add(record);
                }
                StatusText = $"Данные успешно загружены. Записей: {DocumentRecords.Count}";
                _logger.LogInformation("Data loaded successfully.");
            }
            catch (Exception ex)
            {
                StatusText = "Ошибка при загрузке данных.";
                _logger.LogError($"Error loading data: {ex.Message}", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
