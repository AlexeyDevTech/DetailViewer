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
    /// <summary>
    /// ViewModel для основной панели управления, отображающей записи документов и управляющей их фильтрацией, добавлением, редактированием и удалением.
    /// </summary>
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
        /// <summary>
        /// Полное имя текущего активного пользователя.
        /// </summary>
        public string? ActiveUserFullName { get => _activeUserFullName; set => SetProperty(ref _activeUserFullName, value); }

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

        /// <summary>
        /// Текст статуса, отображаемый на панели.
        /// </summary>
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

        /// <summary>
        /// Флаг, указывающий, занято ли приложение выполнением операции.
        /// </summary>
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        /// <summary>
        /// Коллекция записей документов, отображаемых на панели.
        /// </summary>
        public ObservableCollection<DocumentDetailRecord> DocumentRecords { get => _documentRecords; set => SetProperty(ref _documentRecords, value); }

        /// <summary>
        /// Выбранная запись документа.
        /// </summary>
        public DocumentDetailRecord? SelectedRecord { get => _selectedRecord; set { if (SetProperty(ref _selectedRecord, value)) LoadParentAssembliesAndProducts(); } }

        /// <summary>
        /// Коллекция родительских сборок для выбранной записи.
        /// </summary>
        public ObservableCollection<Assembly> ParentAssemblies { get => _parentAssemblies; set => SetProperty(ref _parentAssemblies, value); }

        /// <summary>
        /// Коллекция родительских продуктов для выбранной записи.
        /// </summary>
        public ObservableCollection<DocumentDetailRecord> ParentProducts { get => _parentProducts; set => SetProperty(ref _parentProducts, value); }

        /// <summary>
        /// Фильтр по децимальному номеру.
        /// </summary>
        public string EskdNumberFilter { get => _eskdNumberFilter; set => SetProperty(ref _eskdNumberFilter, value, ApplyFilters); }

        /// <summary>
        /// Фильтр по наименованию.
        /// </summary>
        public string NameFilter { get => _nameFilter; set => SetProperty(ref _nameFilter, value, ApplyFilters); }

        /// <summary>
        /// Фильтр по полному имени автора.
        /// </summary>
        public string FullNameFilter { get => _fullNameFilter; set => SetProperty(ref _fullNameFilter, value, ApplyFilters); }

        /// <summary>
        /// Фильтр по коду ЯСТ.
        /// </summary>
        public string YastCodeFilter { get => _yastCodeFilter; set => SetProperty(ref _yastCodeFilter, value, ApplyFilters); }

        /// <summary>
        /// Флаг, указывающий, нужно ли отображать только записи текущего пользователя.
        /// </summary>
        public bool OnlyMyRecordsFilter { get => _onlyMyRecordsFilter; set => SetProperty(ref _onlyMyRecordsFilter, value, ApplyFilters); }

        /// <summary>
        /// Выбранная дата для фильтрации.
        /// </summary>
        public DateTime? SelectedDate { get => _selectedDate; set => SetProperty(ref _selectedDate, value, ApplyFilters); }

        /// <summary>
        /// Коллекция уникальных полных имен авторов.
        /// </summary>
        public ObservableCollection<string> UniqueFullNames { get => _uniqueFullNames; set => SetProperty(ref _uniqueFullNames, value); }

        /// <summary>
        /// Выбранное полное имя автора для фильтрации.
        /// </summary>
        public string SelectedFullName { get => _selectedFullName; set => SetProperty(ref _selectedFullName, value, ApplyFilters); }

        /// <summary>
        /// Прогресс импорта данных (от 0 до 100).
        /// </summary>
        public double ImportProgress { get => _importProgress; set => SetProperty(ref _importProgress, value); }

        /// <summary>
        /// Статус импорта данных.
        /// </summary>
        public string ImportStatus { get => _importStatus; set => SetProperty(ref _importStatus, value); }

        /// <summary>
        /// Флаг, указывающий, идет ли процесс импорта.
        /// </summary>
        public bool IsImporting { get => _isImporting; set => SetProperty(ref _isImporting, value); }

        /// <summary>
        /// Команда для заполнения формы новой записи.
        /// </summary>
        public DelegateCommand FillFormCommand { get; } 

        /// <summary>
        /// Команда для заполнения формы новой записи на основе выбранной.
        /// </summary>
        public DelegateCommand FillBasedOnCommand { get; } 

        /// <summary>
        /// Команда для редактирования выбранной записи.
        /// </summary>
        public DelegateCommand EditRecordCommand { get; } 

        /// <summary>
        /// Команда для удаления выбранной записи.
        /// </summary>
        public DelegateCommand DeleteRecordCommand { get; } 

        /// <summary>
        /// Команда для импорта данных из Excel.
        /// </summary>
        public DelegateCommand ImportFromExcelCommand { get; } 

        /// <summary>
        /// Команда для экспорта данных в Excel.
        /// </summary>
        public DelegateCommand ExportToExcelCommand { get; } 

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DashboardViewModel"/>.
        /// </summary>
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

            _eventAggregator.GetEvent<UserChangedEvent>().Subscribe(OnCurrentUserChanged);
            FillFormCommand = new DelegateCommand(FillForm);
            FillBasedOnCommand = new DelegateCommand(FillBasedOn, () => SelectedRecord != null).ObservesProperty(() => SelectedRecord);
            EditRecordCommand = new DelegateCommand(EditRecord, () => SelectedRecord != null && SelectedRecord.FullName == ActiveUserFullName).ObservesProperty(() => SelectedRecord).ObservesProperty(() => ActiveUserFullName);
            DeleteRecordCommand = new DelegateCommand(DeleteRecord, () => SelectedRecord != null && SelectedRecord.FullName == ActiveUserFullName).ObservesProperty(() => SelectedRecord).ObservesProperty(() => ActiveUserFullName);
            ImportFromExcelCommand = new DelegateCommand(ImportFromExcel);
            ExportToExcelCommand = new DelegateCommand(ExportToExcel);

            _eventAggregator.GetEvent<SyncCompletedEvent>().Subscribe(OnSyncCompleted, ThreadOption.UIThread);

            LoadData();
        }

        /// <summary>
        /// Вызывается при завершении синхронизации данных.
        /// </summary>
        private async void OnSyncCompleted()
        {
            _logger.Log("Sync completed event received. Reloading data.");
            await LoadData();
        }

        /// <summary>
        /// Асинхронно загружает данные для панели управления.
        /// </summary>
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

        /// <summary>
        /// Применяет фильтры к списку записей документов.
        /// </summary>
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
                filteredRecords = filteredRecords.Where(r => r.FullName == ActiveUserFullName);
            }

            DocumentRecords.Clear();
            foreach (var record in filteredRecords)
            {
                DocumentRecords.Add(record);
            }
            StatusText = $"Отобрано записей: {DocumentRecords.Count}";
        }

        /// <summary>
        /// Асинхронно загружает родительские сборки и продукты для выбранной записи.
        /// </summary>
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

        /// <summary>
        /// Вызывается при изменении текущего пользователя.
        /// </summary>
        /// <param name="p">Профиль нового пользователя.</param>
        private void OnCurrentUserChanged(Profile p)
        {
            _logger.Log("Current user changed");
            ActiveUserFullName = p?.ShortName;
            ApplyFilters();
            EditRecordCommand.RaiseCanExecuteChanged();
            DeleteRecordCommand.RaiseCanExecuteChanged();
        }

        #region Commands Logic

        /// <summary>
        /// Открывает форму для создания новой записи документа.
        /// </summary>
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

        /// <summary>
        /// Открывает форму для создания новой записи на основе выбранной.
        /// </summary>
        private void FillBasedOn()
        {
            _logger.Log("FillBasedOn command executed.");
            var parameters = new DialogParameters { { "record", SelectedRecord }, { "activeUserFullName", ActiveUserFullName } };
            _dialogService.ShowDialog("DocumentRecordForm", parameters, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await LoadData();
                }
            });
        }

        /// <summary>
        /// Открывает форму для редактирования выбранной записи.
        /// </summary>
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

        /// <summary>
        /// Удаляет выбранную запись документа.
        /// </summary>
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

        /// <summary>
        /// Запускает процесс импорта данных из Excel.
        /// </summary>
        private void ImportFromExcel()
        {
            _logger.Log("ImportFromExcel command executed.");
            // ... Implementation ...
        }

        /// <summary>
        /// Запускает процесс экспорта данных в Excel.
        /// </summary>
        private void ExportToExcel()
        {
            _logger.Log("ExportToExcel command executed.");
            // ... Implementation ...
        }

        #endregion
    }
}