using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;

using OfficeOpenXml;
using System.IO;

namespace DetailViewer.Modules.Explorer.ViewModels
{
    public class DashboardViewModel : BindableBase
    {
        private readonly IDocumentDataService _documentDataService;
        private readonly IExcelImportService _excelImportService;
        private readonly IExcelExportService _excelExportService;
        private readonly IDialogService _dialogService;
        private readonly ILogger _logger;
        private readonly IActiveUserService _activeUserService;
        private string _activeUserFullName;

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

        private ObservableCollection<DocumentDetailRecord> _documentRecords;
        public ObservableCollection<DocumentDetailRecord> DocumentRecords
        {
            get { return _documentRecords; }
            set { SetProperty(ref _documentRecords, value); }
        }

        private DocumentDetailRecord _selectedRecord;
        public DocumentDetailRecord SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                if (SetProperty(ref _selectedRecord, value))
                {
                    LoadParentAssembliesAndProducts();
                }
            }
        }

        private ObservableCollection<Assembly> _parentAssemblies;
        public ObservableCollection<Assembly> ParentAssemblies
        {
            get => _parentAssemblies;
            set => SetProperty(ref _parentAssemblies, value);
        }

        private ObservableCollection<DocumentDetailRecord> _parentProducts;
        public ObservableCollection<DocumentDetailRecord> ParentProducts
        {
            get => _parentProducts;
            set => SetProperty(ref _parentProducts, value);
        }

        private List<DocumentDetailRecord> _allRecords;

        private string _eskdNumberFilter;
        public string EskdNumberFilter
        {
            get { return _eskdNumberFilter; }
            set { SetProperty(ref _eskdNumberFilter, value, ApplyFilters); }
        }

        private string _nameFilter;
        public string NameFilter
        {
            get { return _nameFilter; }
            set { SetProperty(ref _nameFilter, value, ApplyFilters); }
        }

        private string _fullNameFilter;
        public string FullNameFilter
        {
            get { return _fullNameFilter; }
            set { SetProperty(ref _fullNameFilter, value, ApplyFilters); }
        }

        private string _yastCodeFilter;
        public string YastCodeFilter
        {
            get { return _yastCodeFilter; }
            set { SetProperty(ref _yastCodeFilter, value, ApplyFilters); }
        }

        private bool _onlyMyRecordsFilter;
        public bool OnlyMyRecordsFilter
        {
            get { return _onlyMyRecordsFilter; }
            set { SetProperty(ref _onlyMyRecordsFilter, value, ApplyFilters); }
        }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get { return _selectedDate; }
            set { SetProperty(ref _selectedDate, value, ApplyFilters); }
        }

        private ObservableCollection<string> _uniqueFullNames;
        public ObservableCollection<string> UniqueFullNames
        {
            get { return _uniqueFullNames; }
            set { SetProperty(ref _uniqueFullNames, value); }
        }

        private string _selectedFullName;
        public string SelectedFullName
        {
            get { return _selectedFullName; }
            set { SetProperty(ref _selectedFullName, value, ApplyFilters); }
        }

        public DelegateCommand FillFormCommand { get; private set; }
        public DelegateCommand FillBasedOnCommand { get; private set; }
        public DelegateCommand EditRecordCommand { get; private set; }
        public DelegateCommand DeleteRecordCommand { get; private set; }
        private double _importProgress;
        public double ImportProgress
        {
            get { return _importProgress; }
            set { SetProperty(ref _importProgress, value); }
        }

        private string _importStatus;
        public string ImportStatus
        {
            get { return _importStatus; }
            set { SetProperty(ref _importStatus, value); }
        }

        private bool _isImporting;
        public bool IsImporting
        {
            get { return _isImporting; }
            set { SetProperty(ref _isImporting, value); }
        }

        public DelegateCommand ImportFromExcelCommand { get; private set; }
        public DelegateCommand ExportToExcelCommand { get; private set; }

        private readonly ISettingsService _settingsService;

        public DashboardViewModel(IDocumentDataService documentDataService, IExcelExportService excelExportService, IExcelImportService excelImportService, IDialogService dialogService, ILogger logger, IActiveUserService activeUserService, ISettingsService settingsService)
        {
            _documentDataService = documentDataService;
            _excelExportService = excelExportService;
            _excelImportService = excelImportService;
            _dialogService = dialogService;
            _dialogService = dialogService;
            _logger = logger;
            _activeUserService = activeUserService;
            _settingsService = settingsService;

            _activeUserService.CurrentUserChanged += OnCurrentUserChanged;
            OnCurrentUserChanged();

            DocumentRecords = new ObservableCollection<DocumentDetailRecord>();
            StatusText = "Готово";

            ParentAssemblies = new ObservableCollection<Assembly>();
            ParentProducts = new ObservableCollection<DocumentDetailRecord>();

            FillFormCommand = new DelegateCommand(FillForm);
            FillBasedOnCommand = new DelegateCommand(FillBasedOn, () => SelectedRecord != null).ObservesProperty(() => SelectedRecord);
            EditRecordCommand = new DelegateCommand(EditRecord, () => SelectedRecord != null && SelectedRecord.FullName == _activeUserFullName).ObservesProperty(() => SelectedRecord);
            DeleteRecordCommand = new DelegateCommand(DeleteRecord, () => SelectedRecord != null && SelectedRecord.FullName == _activeUserFullName).ObservesProperty(() => SelectedRecord);
            ImportFromExcelCommand = new DelegateCommand(ImportFromExcel);
            ExportToExcelCommand = new DelegateCommand(ExportToExcel);
            LoadData();
        }

        private async void LoadParentAssembliesAndProducts()
        {
            ParentAssemblies.Clear();
            ParentProducts.Clear();

            if (SelectedRecord == null)
                return;

            var parentAssemblies = await _documentDataService.GetParentAssembliesForDetailAsync(SelectedRecord.Id);
            foreach(var item in parentAssemblies)
            {
                ParentAssemblies.Add(item);
            }
        }

        private List<string> GetSheetNames(string filePath)
        {
            var sheetNames = new List<string>();
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    sheetNames.Add(worksheet.Name);
                }
            }
            return sheetNames;
        }

        private async void ImportFromExcel()
        {
            var dialogParameters = new DialogParameters();
            dialogParameters.Add("filter", "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*");
            dialogParameters.Add("title", "Import from Excel");

            _dialogService.ShowDialog("OpenFileDialog", dialogParameters, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var filePath = r.Parameters.GetValue<string>("filePath");
                    _dialogService.ShowDialog("ConfirmationDialog", new DialogParameters { { "message", "Создавать связи между деталями и сборками?" } }, async r2 =>
                    {
                        bool createRelationships = r2.Result == ButtonResult.OK;
                        var progress = new Progress<double>(p => StatusText = $"Импорт... {p:F2}%");
                        var sheetNames = GetSheetNames(filePath);
                        dialogParameters.Add("sheetNames", sheetNames);
                        _dialogService.ShowDialog("SelectSheetDialog", dialogParameters, async (result) =>
                        {
                            if (result.Result == ButtonResult.OK)
                            {
                                var selectedSheet = result.Parameters.GetValue<string>("selectedSheet");
                                var createRelationships = result.Parameters.GetValue<bool>("createRelationships");

                                var progress = new Progress<Tuple<double, string>>(p => 
                                {
                                    ImportProgress = p.Item1;
                                    ImportStatus = p.Item2;
                                });

                                IsImporting = true;
                                await _excelImportService.ImportFromExcelAsync(filePath, selectedSheet, progress, createRelationships);
                                IsImporting = false;
                            }
                        });
                        await LoadData();
                    });
                }
            });
        }

        private async void ExportToExcel()
        {
            var dialogParameters = new DialogParameters();
            dialogParameters.Add("filter", "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*");
            dialogParameters.Add("title", "Export to Excel");

            _dialogService.ShowDialog("SaveFileDialog", dialogParameters, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var filePath = r.Parameters.GetValue<string>("filePath");
                    await _excelExportService.ExportToExcelAsync(filePath);
                }
            });
        }

        private void OnCurrentUserChanged()
        {
            _activeUserFullName = _activeUserService.CurrentUser?.ShortName;
            ApplyFilters();
        }

        private void EditRecord()
        {
            var parameters = new DialogParameters { { "record", SelectedRecord } };
            _dialogService.ShowDialog("DocumentRecordForm", parameters, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var updatedRecord = r.Parameters.GetValue<DocumentDetailRecord>("record");
                    var linkedAssemblies = r.Parameters.GetValue<List<Assembly>>("linkedAssemblies");
                    await _documentDataService.UpdateRecordAsync(updatedRecord, linkedAssemblies.Select(a => a.Id).ToList());
                    await LoadData();
                }
            });
        }

        private void DeleteRecord()
        {
            _dialogService.ShowDialog("ConfirmationDialog", new DialogParameters { { "message", $"Вы уверены, что хотите удалить запись: {SelectedRecord.ESKDNumber.FullCode}?" } }, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await _documentDataService.DeleteRecordAsync(SelectedRecord.Id);
                    await LoadData();
                }
            });
        }

        private void FillBasedOn()
        {
            var parameters = new DialogParameters { { "record", SelectedRecord }, { "activeUserFullName", _activeUserFullName } };
            _dialogService.ShowDialog("DocumentRecordForm", parameters, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var newRecord = r.Parameters.GetValue<DocumentDetailRecord>("record");
                    var linkedAssemblies = r.Parameters.GetValue<List<Assembly>>("linkedAssemblies");
                    await _documentDataService.AddRecordAsync(newRecord, linkedAssemblies.Select(a => a.Id).ToList());
                    await LoadData();
                }
            });
        }

        private void FillForm()
        {
            var settings = _settingsService.LoadSettings();
            var parameters = new DialogParameters { { "companyCode", settings.DefaultCompanyCode } };
            _dialogService.ShowDialog("DocumentRecordForm", parameters, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var newRecord = r.Parameters.GetValue<DocumentDetailRecord>("record");
                    var linkedAssemblies = r.Parameters.GetValue<List<Assembly>>("linkedAssemblies");
                    await _documentDataService.AddRecordAsync(newRecord, linkedAssemblies.Select(a => a.Id).ToList());
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
                _allRecords = await _documentDataService.GetAllRecordsAsync();
                UniqueFullNames = new ObservableCollection<string>(_allRecords.Select(r => r.FullName).Distinct().OrderBy(n => n));
                ApplyFilters();
                StatusText = $"Данные успешно загружены.";
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

        private void ApplyFilters()
        {
            if (_allRecords == null) return;

            var filteredRecords = _allRecords.AsEnumerable();

            if (SelectedDate.HasValue)
            {
                filteredRecords = filteredRecords.Where(r => r.Date.Date == SelectedDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(EskdNumberFilter))
            {
                filteredRecords = filteredRecords.Where(r => r.ESKDNumber != null && r.ESKDNumber.FullCode != null && r.ESKDNumber.FullCode.Contains(EskdNumberFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(YastCodeFilter))
            {
                filteredRecords = filteredRecords.Where(r => !string.IsNullOrEmpty(r.YASTCode) && r.YASTCode.Contains(YastCodeFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(NameFilter))
            {
                filteredRecords = filteredRecords.Where(r => !string.IsNullOrEmpty(r.Name) && r.Name.Contains(NameFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(FullNameFilter))
            {
                filteredRecords = filteredRecords.Where(r => !string.IsNullOrEmpty(r.FullName) && r.FullName.Contains(FullNameFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SelectedFullName))
            {
                filteredRecords = filteredRecords.Where(r => r.FullName == SelectedFullName);
            }

            if (OnlyMyRecordsFilter)
            {
                if (_activeUserService.CurrentUser != null)
                {
                    filteredRecords = filteredRecords.Where(r => !string.IsNullOrEmpty(r.FullName) && r.FullName == _activeUserFullName);
                }
            }

            DocumentRecords.Clear();
            foreach (var record in filteredRecords)
            {
                DocumentRecords.Add(record);
            }

            StatusText = $"Отобрано записей: {DocumentRecords.Count}";
        }
    }
}
