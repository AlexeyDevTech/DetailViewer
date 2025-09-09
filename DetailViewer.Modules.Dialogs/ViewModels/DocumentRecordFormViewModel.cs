#nullable enable

using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ILogger = DetailViewer.Core.Interfaces.ILogger;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    /// <summary>
    /// ViewModel для формы создания/редактирования записи документа (детали).
    /// </summary>
    public class DocumentRecordFormViewModel : BindableBase, IDialogAware
    {
        private readonly IDocumentRecordService _documentRecordService;
        private readonly IAssemblyService _assemblyService;
        private readonly IClassifierService _classifierService;
        private readonly IActiveUserService _activeUserService;
        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IEskdNumberService _eskdNumberService;

        private List<DocumentDetailRecord>? _allRecords;
        private ObservableCollection<ClassifierData>? _allClassifiers;
        private string? _activeUserFullName;

        /// <summary>
        /// Все доступные классификаторы.
        /// </summary>
        public ObservableCollection<ClassifierData>? AllClassifiers { get => _allClassifiers; set => SetProperty(ref _allClassifiers, value); }

        /// <summary>
        /// Заголовок диалогового окна.
        /// </summary>
        public string Title => "Форма записи документа";

        /// <summary>
        /// Событие, запрашивающее закрытие диалогового окна.
        /// </summary>
        public event Action<IDialogResult>? RequestClose;

        private DocumentDetailRecord _documentRecord;
        /// <summary>
        /// Редактируемая или создаваемая запись документа.
        /// </summary>
        public DocumentDetailRecord DocumentRecord { get => _documentRecord; set => SetProperty(ref _documentRecord, value); }

        private string? _companyCode, _classNumberString;
        private int _detailNumber;
        private int? _version;

        /// <summary>
        /// Код компании для децимального номера.
        /// </summary>
        public string? CompanyCode { get => _companyCode; set => SetProperty(ref _companyCode, value, OnESKDNumberPartChanged); }

        /// <summary>
        /// Строковое представление номера класса для децимального номера.
        /// </summary>
        public string? ClassNumberString { get => _classNumberString; set => SetProperty(ref _classNumberString, value, OnClassNumberStringChanged); }

        /// <summary>
        /// Порядковый номер детали для децимального номера.
        /// </summary>
        public int DetailNumber { get => _detailNumber; set => SetProperty(ref _detailNumber, value, OnDetailNumberChanged); }

        /// <summary>
        /// Номер версии для децимального номера.
        /// </summary>
        public int? Version { get => _version; set => SetProperty(ref _version, value, OnESKDNumberPartChanged); }

        /// <summary>
        /// Полное строковое представление децимального номера.
        /// </summary>
        public string ESKDNumberString
        {
            get
            {
                if (string.IsNullOrEmpty(CompanyCode) || string.IsNullOrEmpty(ClassNumberString) || DetailNumber == 0) return string.Empty;
                string baseCode = $"{CompanyCode}.{ClassNumberString}.{DetailNumber:D3}";
                return Version.HasValue ? $"{baseCode}-{Version.Value:D2}" : baseCode;
            }
        }

        private bool _isManualDetailNumberEnabled, _isNewVersionEnabled, _filterByFullName = true;
        /// <summary>
        /// Флаг, указывающий, разрешен ли ручной ввод номера детали.
        /// </summary>
        public bool IsManualDetailNumberEnabled { get => _isManualDetailNumberEnabled; set => SetProperty(ref _isManualDetailNumberEnabled, value); }

        /// <summary>
        /// Флаг, указывающий, создается ли новая версия документа.
        /// </summary>
        public bool IsNewVersionEnabled { get => _isNewVersionEnabled; set => SetProperty(ref _isNewVersionEnabled, value, OnIsNewVersionEnabledChanged); }

        /// <summary>
        /// Флаг, указывающий, нужно ли фильтровать записи по полному имени пользователя.
        /// </summary>
        public bool FilterByFullName { get => _filterByFullName; set => SetProperty(ref _filterByFullName, value, FilterRecords); }

        private string? _userMessage;
        /// <summary>
        /// Сообщение для пользователя.
        /// </summary>
        public string? UserMessage { get => _userMessage; set => SetProperty(ref _userMessage, value); }

        private ObservableCollection<ClassifierData>? _filteredClassifiers;
        /// <summary>
        /// Отфильтрованные классификаторы.
        /// </summary>
        public ObservableCollection<ClassifierData>? FilteredClassifiers { get => _filteredClassifiers; set => SetProperty(ref _filteredClassifiers, value); }

        private ObservableCollection<DocumentDetailRecord>? _filteredRecords;
        /// <summary>
        /// Отфильтрованные записи документов.
        /// </summary>
        public ObservableCollection<DocumentDetailRecord>? FilteredRecords { get => _filteredRecords; set => SetProperty(ref _filteredRecords, value); }

        private bool _isUpdatingFromSelection = false;
        private ClassifierData? _selectedClassifier;
        /// <summary>
        /// Выбранный классификатор.
        /// </summary>
        public ClassifierData? SelectedClassifier
        {
            get => _selectedClassifier;
            set
            {
                SetProperty(ref _selectedClassifier, value);
                if (value != null)
                {
                    _isUpdatingFromSelection = true;
                    ClassNumberString = value.Code;
                    _isUpdatingFromSelection = false;
                }
            }
        }

        private ObservableCollection<Assembly> _linkedAssemblies;
        /// <summary>
        /// Коллекция связанных сборок.
        /// </summary>
        public ObservableCollection<Assembly> LinkedAssemblies { get => _linkedAssemblies; set => SetProperty(ref _linkedAssemblies, value); }

        private Assembly? _selectedLinkedAssembly;
        /// <summary>
        /// Выбранная связанная сборка.
        /// </summary>
        public Assembly? SelectedLinkedAssembly { get => _selectedLinkedAssembly; set => SetProperty(ref _selectedLinkedAssembly, value); }

        public ObservableCollection<Product> LinkedProducts
        {
            get => _linkedProducts;
            set => SetProperty(ref _linkedProducts, value);
        }
        public Product? SelectedLinkedProduct { get => _selectedLinkedProduct; set => SetProperty(ref _selectedLinkedProduct, value); }

        private DocumentDetailRecord? _selectedRecordToCopy;
        private ObservableCollection<Product> _linkedProducts;
        private Product? _selectedLinkedProduct;

        /// <summary>
        /// Выбранная запись для копирования (для создания новой версии).
        /// </summary>
        public DocumentDetailRecord? SelectedRecordToCopy
        {
            get => _selectedRecordToCopy;
            set
            {
                SetProperty(ref _selectedRecordToCopy, value);
                if (value != null && IsNewVersionEnabled)
                {
                    CopyDataFromSelectedRecord(value);
                }
            }
        }

        /// <summary>
        /// Команда для сохранения записи документа.
        /// </summary>
        public DelegateCommand SaveCommand { get; private set; }

        /// <summary>
        /// Команда для отмены изменений.
        /// </summary>
        public DelegateCommand CancelCommand { get; private set; }

        /// <summary>
        /// Команда для добавления связи со сборкой.
        /// </summary>
        public DelegateCommand AddAssemblyLinkCommand { get; private set; }

        /// <summary>
        /// Команда для удаления связи со сборкой.
        /// </summary>
        public DelegateCommand RemoveAssemblyLinkCommand { get; private set; }
        public DelegateCommand AddProductLinkCommand { get; private set; }
        public DelegateCommand RemoveProductLinkCommand { get; private set; }


        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DocumentRecordFormViewModel"/>.
        /// </summary>
        public DocumentRecordFormViewModel(IDocumentRecordService documentRecordService, IAssemblyService assemblyService, IClassifierService classifierService, ILogger logger, IActiveUserService activeUserService, ISettingsService settingsService, IDialogService dialogService, IEskdNumberService eskdNumberService)
        {
            _documentRecordService = documentRecordService;
            _assemblyService = assemblyService;
            _classifierService = classifierService;
            _logger = logger;
            _activeUserService = activeUserService;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _eskdNumberService = eskdNumberService;
            _activeUserFullName = _activeUserService.CurrentUser?.ShortName;

            _documentRecord = new DocumentDetailRecord
            {
                Date = DateTime.Now,
                FullName = _activeUserFullName,
                ESKDNumber = new ESKDNumber() { CompanyCode = _settingsService.LoadSettings().DefaultCompanyCode }
            };

            _linkedAssemblies = new ObservableCollection<Assembly>();
            _linkedProducts = new ObservableCollection<Product>();

            SaveCommand = new DelegateCommand(Save, CanSave).ObservesProperty(() => ClassNumberString).ObservesProperty(() => DetailNumber);
            CancelCommand = new DelegateCommand(Cancel);
            AddAssemblyLinkCommand = new DelegateCommand(AddAssemblyLink);
            RemoveAssemblyLinkCommand = new DelegateCommand(RemoveAssemblyLink, () => SelectedLinkedAssembly != null).ObservesProperty(() => SelectedLinkedAssembly);

            AddProductLinkCommand = new DelegateCommand(AddProductLink);
            RemoveProductLinkCommand = new DelegateCommand(RemoveProductLink, () => SelectedLinkedProduct != null).ObservesProperty(() => SelectedLinkedProduct);

            LoadRecords();
        }

        private void RemoveProductLink()
        {
            
        }

        private void AddProductLink()
        {
            _dialogService.ShowDialog("SelectProductDialog", new DialogParameters() { { "SelectProducts", LinkedProducts.ToList() } }, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var selectedProducts = r.Parameters.GetValue<List<Product>>(DialogParameterKeys.SelectedProducts);
                    if (selectedProducts != null) foreach (var product in selectedProducts) if (!LinkedProducts.Any(p => p.Id == product.Id)) LinkedProducts.Add(product);
                }
            });
        }

        /// <summary>
        /// Определяет, можно ли сохранить запись.
        /// </summary>
        /// <returns>True, если запись может быть сохранена, иначе false.</returns>
        private bool CanSave() => !string.IsNullOrWhiteSpace(ClassNumberString) && ClassNumberString.Length == 6 && DetailNumber > 0;

        /// <summary>
        /// Асинхронно загружает все записи документов.
        /// </summary>
        private async void LoadRecords() => _allRecords = await _documentRecordService.GetAllRecordsAsync();

        /// <summary>
        /// Загружает все классификаторы.
        /// </summary>
        private void LoadClassifiers() => AllClassifiers = new ObservableCollection<ClassifierData>(_classifierService.GetAllClassifiers());

        /// <summary>
        /// Фильтрует записи документов на основе текущих значений фильтров.
        /// </summary>
        private void FilterRecords()
        {
            if (_allRecords == null || ClassNumberString?.Length != 6) { FilteredRecords = new ObservableCollection<DocumentDetailRecord>(); return; }
            var records = _allRecords.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(ClassNumberString)) records = records.Where(r => r.ESKDNumber?.ClassNumber?.Number.ToString("D6").StartsWith(ClassNumberString) ?? false);
            if (IsNewVersionEnabled && DetailNumber > 0 && SelectedRecordToCopy == null) records = records.Where(r => r.ESKDNumber?.ClassNumber?.Number.ToString("D6").StartsWith(ClassNumberString ?? string.Empty) == true);
            if (FilterByFullName && !string.IsNullOrEmpty(_activeUserFullName)) records = records.Where(r => r.FullName == _activeUserFullName);
            FilteredRecords = new ObservableCollection<DocumentDetailRecord>(records.OrderBy(r => r.ESKDNumber?.FullCode).ToList());
        }

        /// <summary>
        /// Фильтрует классификаторы на основе текущих значений фильтров.
        /// </summary>
        private void FilterClassifiers()
        {
            if (AllClassifiers == null) { FilteredClassifiers = new ObservableCollection<ClassifierData>(); return; }
            if (string.IsNullOrWhiteSpace(ClassNumberString)) { FilteredClassifiers = new ObservableCollection<ClassifierData>(AllClassifiers); return; }
            FilteredClassifiers = new ObservableCollection<ClassifierData>(AllClassifiers.Where(c => c.Code.StartsWith(ClassNumberString, StringComparison.OrdinalIgnoreCase)).OrderBy(c => c.Code).ToList());
        }

        /// <summary>
        /// Вызывается при изменении части децимального номера для обновления свойства ESKDNumberString.
        /// </summary>
        private void OnESKDNumberPartChanged() => RaisePropertyChanged(nameof(ESKDNumberString));

        /// <summary>
        /// Вызывается при изменении строки номера класса.
        /// </summary>
        private async void OnClassNumberStringChanged()
        {
            if (_isUpdatingFromSelection) return;
            FilterClassifiers();
            FilterRecords();
            if (ClassNumberString?.Length == 6) await FindNextDetailNumber();
            OnESKDNumberPartChanged();
            SaveCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Вызывается при изменении номера детали.
        /// </summary>
        private void OnDetailNumberChanged()
        {
            FilterRecords();
            OnESKDNumberPartChanged();
            SaveCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Вызывается при изменении флага IsNewVersionEnabled.
        /// </summary>
        private void OnIsNewVersionEnabledChanged()
        {
            if (IsNewVersionEnabled) { IsManualDetailNumberEnabled = true; UserMessage = "Выберите запись для исполнения"; DocumentRecord.YASTCode = null; DocumentRecord.Name = null; SelectedRecordToCopy = null; } else { UserMessage = null; }
        }

        /// <summary>
        /// Асинхронно находит следующий доступный номер детали.
        /// </summary>
        private async Task FindNextDetailNumber()
        {
            if (string.IsNullOrWhiteSpace(ClassNumberString))
            {
                DetailNumber = 0;
                return;
            }
            DetailNumber = await _eskdNumberService.GetNextDetailNumberAsync(ClassNumberString);
        }

        /// <summary>
        /// Асинхронно находит следующий доступный номер версии.
        /// </summary>
        private async Task FindNextVersionNumber()
        {
            if (SelectedRecordToCopy == null)
            {
                Version = null;
                return;
            }
            Version = await _eskdNumberService.GetNextVersionNumberAsync(SelectedRecordToCopy);
        }

        /// <summary>
        /// Копирует данные из выбранной записи для создания новой версии.
        /// </summary>
        /// <param name="sourceRecord">Исходная запись для копирования.</param>
        private async void CopyDataFromSelectedRecord(DocumentDetailRecord sourceRecord)
        {
            if (sourceRecord.ESKDNumber == null) return;
            DocumentRecord.YASTCode = sourceRecord.YASTCode;
            DocumentRecord.Name = sourceRecord.Name;
            CompanyCode = sourceRecord.ESKDNumber.CompanyCode;
            ClassNumberString = sourceRecord.ESKDNumber.ClassNumber?.Number.ToString("D6");
            DetailNumber = sourceRecord.ESKDNumber.DetailNumber;
            await FindNextVersionNumber();
            RaisePropertyChanged(string.Empty);
            UserMessage = null;
        }

        /// <summary>
        /// Сохраняет запись документа (добавляет или обновляет).
        /// </summary>
        private async void Save()
        {
            if (DocumentRecord.ESKDNumber == null) DocumentRecord.ESKDNumber = new ESKDNumber();
            DocumentRecord.ESKDNumber.CompanyCode = CompanyCode;
            DocumentRecord.ESKDNumber.DetailNumber = DetailNumber;
            DocumentRecord.ESKDNumber.Version = Version;

            if (DocumentRecord.ESKDNumber.ClassNumber == null) DocumentRecord.ESKDNumber.ClassNumber = new Classifier();

            if (SelectedClassifier != null)
            {
                DocumentRecord.ESKDNumber.ClassNumber.Number = int.Parse(SelectedClassifier.Code);
                DocumentRecord.ESKDNumber.ClassNumber.Name = SelectedClassifier.Description;
            }
            else if (int.TryParse(ClassNumberString, out int classNumberValue))
            {
                DocumentRecord.ESKDNumber.ClassNumber.Number = classNumberValue;
                var classifierData = _classifierService.GetClassifierByCode(ClassNumberString);
                DocumentRecord.ESKDNumber.ClassNumber.Name = classifierData?.Description ?? "<неопознанный код>";
            }

            if (DocumentRecord.Id == 0) await _documentRecordService.AddRecordAsync(DocumentRecord, DocumentRecord.ESKDNumber, LinkedAssemblies.Select(a => a.Id).ToList(), LinkedProducts.Select(a => a.Id).ToList());
            else await _documentRecordService.UpdateRecordAsync(DocumentRecord, LinkedAssemblies.Select(a => a.Id).ToList(), LinkedProducts.Select(a => a.Id).ToList());
            RequestClose?.Invoke(BuildDialogResult());
        }

        /// <summary>
        /// Формирует результат диалогового окна.
        /// </summary>
        /// <returns>Объект IDialogResult.</returns>
        private IDialogResult BuildDialogResult()
        {
            var result = new DialogResult(ButtonResult.OK);
            result.Parameters.Add("record", DocumentRecord);
            result.Parameters.Add("linkedAssemblies", LinkedAssemblies?.ToList() ?? new List<Assembly>());
            return result;
        }

        /// <summary>
        /// Отменяет изменения и закрывает диалоговое окно.
        /// </summary>
        private void Cancel() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));

        /// <summary>
        /// Открывает диалог выбора сборки и добавляет выбранные сборки в список связанных.
        /// </summary>
        private void AddAssemblyLink()
        {
            _dialogService.ShowDialog("SelectAssemblyDialog", new DialogParameters(), r =>
            {
                if (r.Result == ButtonResult.OK && r.Parameters.ContainsKey("selectedAssemblies"))
                {
                    var selectedAssemblies = r.Parameters.GetValue<List<Assembly>>("selectedAssemblies");
                    if (selectedAssemblies != null) foreach (var assembly in selectedAssemblies) if (!LinkedAssemblies.Any(a => a.Id == assembly.Id)) LinkedAssemblies.Add(assembly);
                }
            });
        }



        /// <summary>
        /// Удаляет выбранную связанную сборку.
        /// </summary>
        private void RemoveAssemblyLink() { if (SelectedLinkedAssembly != null) LinkedAssemblies.Remove(SelectedLinkedAssembly); }


        /// <summary>
        /// Определяет, можно ли закрыть диалоговое окно.
        /// </summary>
        /// <returns>Всегда true.</returns>
        public bool CanCloseDialog() => true;

        /// <summary>
        /// Вызывается после закрытия диалогового окна.
        /// </summary>
        public void OnDialogClosed() { }

        /// <summary>
        /// Вызывается при открытии диалогового окна.
        /// </summary>
        /// <param name="parameters">Параметры диалогового окна.</param>
        public async void OnDialogOpened(IDialogParameters parameters)
        {
            await _classifierService.LoadClassifiersAsync();
            LoadClassifiers();
            if (parameters.ContainsKey(DialogParameterKeys.Record))
            {
                var record = parameters.GetValue<DocumentDetailRecord>(DialogParameterKeys.Record);
                if (record != null)
                {
                    if (parameters.ContainsKey(DialogParameterKeys.ActiveUserFullName))
                    {
                        await HandleFillBasedOnScenario(record);
                    }
                    else
                    {
                        await HandleEditScenario(record);
                    }
                }
            }
            else
            {
                HandleNewRecordScenario(parameters);
            }
            if (IsNewVersionEnabled) UserMessage = "Выберите запись для копирования";
        }

        /// <summary>
        /// Обрабатывает сценарий редактирования существующей записи.
        /// </summary>
        /// <param name="record">Запись для редактирования.</param>
        private async Task HandleEditScenario(DocumentDetailRecord record)
        {
            DocumentRecord = record;
            if (DocumentRecord.ESKDNumber != null)
            {
                CompanyCode = DocumentRecord.ESKDNumber.CompanyCode;
                ClassNumberString = DocumentRecord.ESKDNumber.ClassNumber?.Number.ToString("D6");
                DetailNumber = DocumentRecord.ESKDNumber.DetailNumber;
                Version = DocumentRecord.ESKDNumber.Version;
                IsManualDetailNumberEnabled = DocumentRecord.IsManualDetailNumber;
                var linkedAssemblies = await _documentRecordService.GetParentAssembliesForDetailAsync(DocumentRecord.Id);
                var linkedProducts = await _documentRecordService.GetParentProductsForDetailAsync(DocumentRecord.Id);
                LinkedAssemblies = new ObservableCollection<Assembly>(linkedAssemblies);
                LinkedProducts = new ObservableCollection<Product>(linkedProducts);
            }
        }

        /// <summary>
        /// Обрабатывает сценарий заполнения новой записи на основе существующей.
        /// </summary>
        /// <param name="record">Исходная запись для заполнения.</param>
        private async Task HandleFillBasedOnScenario(DocumentDetailRecord record)
        {
            if (record.ESKDNumber?.ClassNumber == null) return;
            DocumentRecord = new DocumentDetailRecord
            {
                Date = DateTime.Now,
                FullName = _activeUserFullName,
                YASTCode = record.YASTCode,
                Name = record.Name,
                ESKDNumber = new ESKDNumber() { CompanyCode = _settingsService.LoadSettings().DefaultCompanyCode, ClassNumber = new Classifier { Number = record.ESKDNumber.ClassNumber.Number }, }
            };
            CompanyCode = DocumentRecord.ESKDNumber.CompanyCode;
            ClassNumberString = DocumentRecord.ESKDNumber.ClassNumber?.Number.ToString("D6");
            await FindNextDetailNumber();
            Version = null;
            IsManualDetailNumberEnabled = false;
        }

        /// <summary>
        /// Обрабатывает сценарий создания новой записи.
        /// </summary>
        /// <param name="parameters">Параметры диалогового окна.</param>
        private void HandleNewRecordScenario(IDialogParameters parameters)
        {
            if (parameters.ContainsKey(DialogParameterKeys.CompanyCode)) { CompanyCode = parameters.GetValue<string>(DialogParameterKeys.CompanyCode); if (DocumentRecord.ESKDNumber != null) DocumentRecord.ESKDNumber.CompanyCode = CompanyCode; }
        }
    }
}
