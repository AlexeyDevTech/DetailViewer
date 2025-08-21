using DetailViewer.Core.Events;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using ILogger = DetailViewer.Core.Interfaces.ILogger;

namespace DetailViewer.Modules.Explorer.ViewModels
{
    public class DashboardViewModel : BindableBase
    {
        private readonly IDocumentRecordService _documentRecordService;
        private readonly IExcelImportService _excelImportService;
        private readonly IExcelExportService _excelExportService;
        private readonly IDialogService _dialogService;
        private readonly ILogger _logger;
        private readonly IActiveUserService _activeUserService;
        private readonly ISettingsService _settingsService;
        private readonly IEventAggregator _eventAggregator;

        private string? _activeUserFullName;
        private string _statusText;
        private bool _isBusy;
        private ObservableCollection<DocumentDetailRecord> _documentRecords;
        private DocumentDetailRecord _selectedRecord;
        private ObservableCollection<Assembly> _parentAssemblies;
        private ObservableCollection<DocumentDetailRecord> _parentProducts;
        private List<DocumentDetailRecord> _allRecords;
        private string _eskdNumberFilter;
        private string _nameFilter;
        private string _fullNameFilter;
        private string _yastCodeFilter;
        private bool _onlyMyRecordsFilter;
        private DateTime? _selectedDate;
        private ObservableCollection<string> _uniqueFullNames;
        private string _selectedFullName;
        private double _importProgress;
        private string _importStatus;
        private bool _isImporting;

        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
        public ObservableCollection<DocumentDetailRecord> DocumentRecords { get => _documentRecords; set => SetProperty(ref _documentRecords, value); }
        public DocumentDetailRecord? SelectedRecord { get => _selectedRecord; set { if (SetProperty(ref _selectedRecord, value)) LoadParentAssembliesAndProducts(); } }
        public ObservableCollection<Assembly> ParentAssemblies { get => _parentAssemblies; set => SetProperty(ref _parentAssemblies, value); }
        public ObservableCollection<DocumentDetailRecord> ParentProducts { get => _parentProducts; set => SetProperty(ref _parentProducts, value); }
        public string EskdNumberFilter { get => _eskdNumberFilter; set => SetProperty(ref _eskdNumberFilter, value, ApplyFilters); }
        public string NameFilter { get => _nameFilter; set => SetProperty(ref _nameFilter, value, ApplyFilters); }
        public string FullNameFilter { get => _fullNameFilter; set => SetProperty(ref _fullNameFilter, value, ApplyFilters); }
        public string YastCodeFilter { get => _yastCodeFilter; set => SetProperty(ref _yastCodeFilter, value, ApplyFilters); }
        public bool OnlyMyRecordsFilter { get => _onlyMyRecordsFilter; set => SetProperty(ref _onlyMyRecordsFilter, value, ApplyFilters); }
        public DateTime? SelectedDate { get => _selectedDate; set => SetProperty(ref _selectedDate, value, ApplyFilters); }
        public ObservableCollection<string> UniqueFullNames { get => _uniqueFullNames; set => SetProperty(ref _uniqueFullNames, value); }
        public string SelectedFullName { get => _selectedFullName; set => SetProperty(ref _selectedFullName, value, ApplyFilters); }
        public double ImportProgress { get => _importProgress; set => SetProperty(ref _importProgress, value); }
        public string ImportStatus { get => _importStatus; set => SetProperty(ref _importStatus, value); }
        public bool IsImporting { get => _isImporting; set => SetProperty(ref _isImporting, value); }

        public DelegateCommand FillFormCommand { get; } 
        public DelegateCommand FillBasedOnCommand { get; } 
        public DelegateCommand EditRecordCommand { get; } 
        public DelegateCommand DeleteRecordCommand { get; } 
        public DelegateCommand ImportFromExcelCommand { get; } 
        public DelegateCommand ExportToExcelCommand { get; } 

        public DashboardViewModel(IDocumentRecordService documentRecordService, IExcelExportService excelExportService, IExcelImportService excelImportService, IDialogService dialogService, ILogger logger, IActiveUserService activeUserService, ISettingsService settingsService, IEventAggregator eventAggregator)
        {
            _documentRecordService = documentRecordService;
            _excelExportService = excelExportService;
            _excelImportService = excelImportService;
            _dialogService = dialogService;
            _logger = logger;
            _activeUserService = activeUserService;
            _settingsService = settingsService;
            _eventAggregator = eventAggregator;

            _documentRecords = new ObservableCollection<DocumentDetailRecord>();
            _parentAssemblies = new ObservableCollection<Assembly>();
            _parentProducts = new ObservableCollection<DocumentDetailRecord>();
            StatusText = "Готово";

            _activeUserService.CurrentUserChanged += OnCurrentUserChanged;
            OnCurrentUserChanged();

            FillFormCommand = new DelegateCommand(FillForm);
            FillBasedOnCommand = new DelegateCommand(FillBasedOn, () => SelectedRecord != null).ObservesProperty(() => SelectedRecord);
            EditRecordCommand = new DelegateCommand(EditRecord, () => SelectedRecord != null && SelectedRecord.FullName == _activeUserFullName).ObservesProperty(() => SelectedRecord);
            DeleteRecordCommand = new DelegateCommand(DeleteRecord, () => SelectedRecord != null && SelectedRecord.FullName == _activeUserFullName).ObservesProperty(() => SelectedRecord);
            ImportFromExcelCommand = new DelegateCommand(ImportFromExcel);
            ExportToExcelCommand = new DelegateCommand(ExportToExcel);

            _eventAggregator.GetEvent<SyncCompletedEvent>().Subscribe(OnSyncCompleted, ThreadOption.UIThread);
        }

        private async void OnSyncCompleted()
        {
            _logger.Log("Sync completed event received. Reloading data.");
            await LoadData();
        }

        private async Task LoadData()
        {
            _logger.Log("Loading data for Dashboard.");
            IsBusy = true;
            StatusText = "Загрузка данных...";
            try
            {
                _allRecords = await _documentRecordService.GetAllRecordsAsync();
                if (_allRecords != null)
                {
                    UniqueFullNames = new ObservableCollection<string>(_allRecords.Select(r => r.FullName).Distinct().OrderBy(n => n));
                    ApplyFilters();
                    StatusText = $"Данные успешно загружены. Записей: {_allRecords.Count}";
                    _logger.LogInfo("Data loaded successfully.");
                }
                else
                {
                    StatusText = "Данные не были загружены.";
                    _logger.LogWarning("LoadData completed but _allRecords is null.");
                }
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

        private void ApplyFilters()
        {
            _logger.Log("Applying filters to records");
            if (_allRecords == null) return;

            var filteredRecords = _allRecords.AsEnumerable();

            if (SelectedDate.HasValue)
            {
                filteredRecords = filteredRecords.Where(r => r.Date.Date == SelectedDate.Value.Date);
            }
            if (!string.IsNullOrWhiteSpace(EskdNumberFilter))
            {
                filteredRecords = filteredRecords.Where(r => r.ESKDNumber?.FullCode?.Contains(EskdNumberFilter, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(YastCodeFilter))
            {
                filteredRecords = filteredRecords.Where(r => r.YASTCode?.Contains(YastCodeFilter, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(NameFilter))
            {
                filteredRecords = filteredRecords.Where(r => r.Name?.Contains(NameFilter, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(FullNameFilter))
            {
                filteredRecords = filteredRecords.Where(r => r.FullName?.Contains(FullNameFilter, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(SelectedFullName))
            {
                filteredRecords = filteredRecords.Where(r => r.FullName == SelectedFullName);
            }
            if (OnlyMyRecordsFilter && _activeUserService.CurrentUser != null)
            {
                filteredRecords = filteredRecords.Where(r => r.FullName == _activeUserFullName);
            }

            DocumentRecords.Clear();
            foreach (var record in filteredRecords)
            {
                DocumentRecords.Add(record);
            }
            StatusText = $"Отобрано записей: {DocumentRecords.Count}";
        }

        private async void LoadParentAssembliesAndProducts()
        {
            if (SelectedRecord == null) return;
            _logger.Log("Loading parent assemblies and products");
            ParentAssemblies.Clear();
            ParentProducts.Clear();
            var parentAssemblies = await _documentRecordService.GetParentAssembliesForDetailAsync(SelectedRecord.Id);
            foreach(var item in parentAssemblies)
            {
                ParentAssemblies.Add(item);
            }
        }

        private void OnCurrentUserChanged()
        {
            _logger.Log("Current user changed");
            _activeUserFullName = _activeUserService.CurrentUser?.ShortName;
            ApplyFilters();
        }

        #region Commands Logic

        private void FillForm()
        {
            _logger.Log("FillForm command executed.");
            var settings = _settingsService.LoadSettings();
            var parameters = new DialogParameters { { "companyCode", settings.DefaultCompanyCode } };
            _dialogService.ShowDialog("DocumentRecordForm", parameters, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await LoadData();
                }
            });
        }

        private void FillBasedOn()
        {
            _logger.Log("FillBasedOn command executed.");
            var parameters = new DialogParameters { { "record", SelectedRecord }, { "activeUserFullName", _activeUserFullName } };
            _dialogService.ShowDialog("DocumentRecordForm", parameters, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await LoadData();
                }
            });
        }

        private void EditRecord()
        {
            _logger.Log("EditRecord command executed.");
            var parameters = new DialogParameters { { "record", SelectedRecord } };
            _dialogService.ShowDialog("DocumentRecordForm", parameters, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await LoadData();
                }
            });
        }

        private void DeleteRecord()
        {
            if (SelectedRecord == null)
            {
                return;
            }

            _logger.Log("DeleteRecord command executed.");
            _dialogService.ShowDialog("ConfirmationDialog", new DialogParameters { { "message", $"Вы уверены, что хотите удалить запись: {SelectedRecord.ESKDNumber.FullCode}?" } }, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (SelectedRecord != null)
                    {
                        await _documentRecordService.DeleteRecordAsync(SelectedRecord.Id);
                        await LoadData();
                    }
                }
            });
        }

        private void ImportFromExcel()
        {
            _logger.Log("ImportFromExcel command executed.");
            // ... Implementation ...
        }

        private void ExportToExcel()
        {
            _logger.Log("ExportToExcel command executed.");
            // ... Implementation ...
        }

        #endregion
    }
}
