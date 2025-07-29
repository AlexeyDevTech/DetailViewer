using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class DocumentRecordFormViewModel : BindableBase, IDialogAware
    {
        // Injected Services
        private readonly IDocumentDataService _documentDataService;
        private readonly IActiveUserService _activeUserService;
        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;

        // Data collections
        private List<DocumentDetailRecord> _allRecords;
        private ObservableCollection<ClassifierData> _allClassifiers;

        // Active User Info
        private string _activeUserFullName;

        public ObservableCollection<ClassifierData> AllClassifiers { get => _allClassifiers; set => SetProperty(ref _allClassifiers, value); }

        public string Title => "Форма записи документа";
        public event Action<IDialogResult> RequestClose;

        private DocumentDetailRecord _documentRecord;
        public DocumentDetailRecord DocumentRecord
        {
            get { return _documentRecord; }
            set { SetProperty(ref _documentRecord, value); }
        }

        // --- ESKD Number Properties ---
        private string _companyCode, _classNumberString;
        private int _detailNumber;
        private int? _version;

        public string CompanyCode { get => _companyCode; set => SetProperty(ref _companyCode, value, OnESKDNumberPartChanged); }
        public string ClassNumberString { get => _classNumberString; set => SetProperty(ref _classNumberString, value, OnClassNumberStringChanged); }
        public int DetailNumber { get => _detailNumber; set => SetProperty(ref _detailNumber, value, OnDetailNumberChanged); }
        public int? Version { get => _version; set => SetProperty(ref _version, value, OnESKDNumberPartChanged); }

        public string ESKDNumberString
        {
            get
            {
                if (string.IsNullOrEmpty(CompanyCode) || string.IsNullOrEmpty(ClassNumberString) || DetailNumber == 0)
                {
                    return string.Empty;
                }

                string baseCode = $"{CompanyCode}.{int.Parse(ClassNumberString):D6}.{DetailNumber:D3}";
                return Version.HasValue ? $"{baseCode}-{Version.Value:D2}" : baseCode;
            }
        }

        // --- UI State Properties ---
        private bool _isManualDetailNumberEnabled, _isNewVersionEnabled, _filterByFullName = true;
        public bool IsManualDetailNumberEnabled { get => _isManualDetailNumberEnabled; set => SetProperty(ref _isManualDetailNumberEnabled, value); }
        public bool IsNewVersionEnabled { get => _isNewVersionEnabled; set => SetProperty(ref _isNewVersionEnabled, value, OnIsNewVersionEnabledChanged); }
        public bool FilterByFullName { get => _filterByFullName; set => SetProperty(ref _filterByFullName, value, FilterRecords); }

        private string _userMessage;
        public string UserMessage { get => _userMessage; set => SetProperty(ref _userMessage, value); }

        // --- Filtered Collections for UI ---
        private ObservableCollection<ClassifierData> _filteredClassifiers;
        public ObservableCollection<ClassifierData> FilteredClassifiers { get => _filteredClassifiers; set => SetProperty(ref _filteredClassifiers, value); }

        private ObservableCollection<DocumentDetailRecord> _filteredRecords;
        public ObservableCollection<DocumentDetailRecord> FilteredRecords { get => _filteredRecords; set => SetProperty(ref _filteredRecords, value); }

        private ObservableCollection<Assembly> _linkedAssemblies;
        public ObservableCollection<Assembly> LinkedAssemblies
        {
            get { return _linkedAssemblies; }
            set { SetProperty(ref _linkedAssemblies, value); }
        }

        private Assembly _selectedLinkedAssembly;
        public Assembly SelectedLinkedAssembly
        {
            get { return _selectedLinkedAssembly; }
            set { SetProperty(ref _selectedLinkedAssembly, value); }
        }

        private DocumentDetailRecord _selectedRecordToCopy;
        public DocumentDetailRecord SelectedRecordToCopy
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

        // --- Commands ---
        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand AddAssemblyLinkCommand { get; private set; }
        public DelegateCommand RemoveAssemblyLinkCommand { get; private set; }

        // --- Constructor ---
        public DocumentRecordFormViewModel(IDocumentDataService documentDataService, ILogger logger, IActiveUserService activeUserService, ISettingsService settingsService, IDialogService dialogService)
        {
            _documentDataService = documentDataService;
            _logger = logger;
            _activeUserService = activeUserService;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _activeUserFullName = _activeUserService.CurrentUser?.ShortName;

            DocumentRecord = new DocumentDetailRecord
            {
                Date = DateTime.Now,
                FullName = _activeUserFullName,
                ESKDNumber = new ESKDNumber()
                {
                    ClassNumber = new Classifier(),
                    CompanyCode = _settingsService.LoadSettings().DefaultCompanyCode // Set default company code here
                }
            };

            LinkedAssemblies = new ObservableCollection<Assembly>();

            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
            AddAssemblyLinkCommand = new DelegateCommand(AddAssemblyLink);
            RemoveAssemblyLinkCommand = new DelegateCommand(RemoveAssemblyLink, () => SelectedLinkedAssembly != null).ObservesProperty(() => SelectedLinkedAssembly);

            LoadClassifiers();
            LoadRecords();
        }

        // --- Methods for Assembling/Disassembling Composite Numbers ---

        // --- Data Loading ---
        private async void LoadRecords()
        {
            _allRecords = await _documentDataService.GetAllRecordsAsync();
            FilterRecords();
        }

        private async void LoadClassifiers()
        {
            try
            {
                string jsonFilePath = "eskd_classifiers.json";
                if (File.Exists(jsonFilePath))
                {
                    string jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                    var rootClassifiers = JsonSerializer.Deserialize<List<ClassifierData>>(jsonContent);
                    AllClassifiers = new ObservableCollection<ClassifierData>(FlattenClassifiers(rootClassifiers));
                    FilterClassifiers();
                }
                else
                {
                    _logger.LogWarning($"eskd_classifiers.json not found at {jsonFilePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading classifiers: {ex.Message}", ex);
            }
        }

        private List<ClassifierData> FlattenClassifiers(List<ClassifierData> classifiers)
        {
            var flattenedList = new List<ClassifierData>();
            foreach (var classifier in classifiers)
            {
                flattenedList.Add(classifier);
                if (classifier.Children != null && classifier.Children.Any())
                {
                    flattenedList.AddRange(FlattenClassifiers(classifier.Children));
                }
            }
            return flattenedList;
        }

        

        

        // --- Filtering Logic ---
        private void FilterRecords()
        {
            if (_allRecords == null || ClassNumberString?.Length != 6)
            {
                FilteredRecords = new ObservableCollection<DocumentDetailRecord>();
                return;
            }

            var records = _allRecords.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(ClassNumberString))
            {
                records = records.Where(r => 
                {
                    if (r.ESKDNumber.ClassNumber != null)
                        return r.ESKDNumber.ClassNumber.Number.ToString("D6").StartsWith(ClassNumberString);
                    else return false;
                        
                });
            }

            if (IsNewVersionEnabled && DetailNumber > 0 && SelectedRecordToCopy == null)
            {
                //records = records.Where(r => r.ESKDNumber.DetailNumber == DetailNumber);
                records.Where(r => r.ESKDNumber.ClassNumber.Number.ToString("D6").StartsWith(ClassNumberString));
            }

            if (FilterByFullName)
            {
                records = records.Where(r => r.FullName == _activeUserFullName);
            }

            FilteredRecords = new ObservableCollection<DocumentDetailRecord>(records.OrderBy(r => r.ESKDNumber.FullCode).ToList());
        }

        private void FilterClassifiers()
        {
            if (AllClassifiers == null || string.IsNullOrWhiteSpace(ClassNumberString))
            {
                FilteredClassifiers = new ObservableCollection<ClassifierData>();
                return;
            }
            FilteredClassifiers = new ObservableCollection<ClassifierData>(
                AllClassifiers.Where(c => c.Code.StartsWith(ClassNumberString, StringComparison.OrdinalIgnoreCase))
                              .OrderBy(c => c.Code.Length)
                              .ThenBy(c => c.Code)
                              .ToList()
            );
        }

        // --- Event Handlers for UI changes ---
        private void OnESKDNumberPartChanged() => RaisePropertyChanged(nameof(ESKDNumberString));
        private void OnClassNumberStringChanged()
        {
            FilterClassifiers();
            FilterRecords();
            if (ClassNumberString?.Length == 6) FindNextDetailNumber();
            OnESKDNumberPartChanged();
        }
        private void OnDetailNumberChanged()
        {
            FilterRecords();
            OnESKDNumberPartChanged();
        }
        private void OnIsNewVersionEnabledChanged()
        {
            if (IsNewVersionEnabled)
            {
                IsManualDetailNumberEnabled = true;
                UserMessage = "Выберите запись для исполнения";
                // Clear relevant fields when 'New Version' is checked
                DocumentRecord.YASTCode = null;
                DocumentRecord.Name = null;
                // ESKD number parts will be copied from selected record, not cleared here.
                SelectedRecordToCopy = null; // Clear selected record
            }
            else
            {
                UserMessage = null;
            }
            //FilterRecords();
        }

        // --- Business Logic ---
        private void FindNextDetailNumber()
        {
            if (_allRecords == null) return;
            var existingDetailNumbers = _allRecords
                .Where(r => {
                    if (r.ESKDNumber.ClassNumber != null)
                        return r.ESKDNumber.ClassNumber.Number.ToString("D6") == ClassNumberString;
                    else return false;
                    })
                .Select(r => r.ESKDNumber.DetailNumber)
                .ToHashSet();
            for (int i = 1; i <= 999; i++)
            {
                if (!existingDetailNumbers.Contains(i))
                {
                    DetailNumber = i;
                    return;
                }
            }
        }
        private void FindNextVersionNumber()
        {
            if (_allRecords == null || string.IsNullOrEmpty(CompanyCode) || string.IsNullOrEmpty(ClassNumberString) || DetailNumber == 0)
            {
                Version = null; // No valid ESKD number to find a version for
                return;
            }

            var existingVersions = _allRecords
                .Where(r => r.ESKDNumber.CompanyCode == CompanyCode &&
                            r.ESKDNumber.ClassNumber.Number.ToString("D6") == ClassNumberString &&
                            r.ESKDNumber.DetailNumber == DetailNumber &&
                            r.ESKDNumber.Version.HasValue)
                .Select(r => r.ESKDNumber.Version.Value)
                .ToList();

            if (existingVersions.Any())
            {
                Version = existingVersions.Max() + 1;
            }
            else
            {
                Version = 1; // First version for this ESKD number
            }
        }

        private void CopyDataFromSelectedRecord(DocumentDetailRecord sourceRecord)
        {
            // Copy all fields except Date and FullName
            DocumentRecord.YASTCode = sourceRecord.YASTCode;
            DocumentRecord.Name = sourceRecord.Name;

            // ESKD Number parts
            CompanyCode = sourceRecord.ESKDNumber.CompanyCode;
            ClassNumberString = sourceRecord.ESKDNumber.ClassNumber?.Number.ToString("D6");
            DetailNumber = sourceRecord.ESKDNumber.DetailNumber;
            FindNextVersionNumber(); // Find next version based on copied ESKD number

            RaisePropertyChanged(nameof(DocumentRecord));
            UserMessage = null; // Clear message after selection
        }

        // --- Dialog-related Methods ---
        private void Save()
        {
            DocumentRecord.ESKDNumber.CompanyCode = CompanyCode;
            if (int.TryParse(ClassNumberString, out int classNumber))
            {
                if (DocumentRecord.ESKDNumber.ClassNumber == null)
                {
                    DocumentRecord.ESKDNumber.ClassNumber = new Classifier();
                }
                DocumentRecord.ESKDNumber.ClassNumber.Number = classNumber;
            }
            DocumentRecord.ESKDNumber.DetailNumber = DetailNumber;
            DocumentRecord.ESKDNumber.Version = Version;

            RequestClose?.Invoke(BuildDialogResult());
        }

        private IDialogResult BuildDialogResult()
        {
            var result = new DialogResult(ButtonResult.OK);
            result.Parameters.Add("record", DocumentRecord);
            result.Parameters.Add("linkedAssemblies", LinkedAssemblies.ToList());
            return result;
        }

        private void Cancel() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));

        private void AddAssemblyLink()
        {
            _dialogService.ShowDialog("SelectAssemblyDialog", new DialogParameters(), r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var selectedAssemblies = r.Parameters.GetValue<List<Assembly>>("selectedAssemblies");
                    foreach (var assembly in selectedAssemblies)
                    {
                        if (!LinkedAssemblies.Any(a => a.Id == assembly.Id))
                        {
                            LinkedAssemblies.Add(assembly);
                        }
                    }
                }
            });
        }

        private void RemoveAssemblyLink()
        {
            if (SelectedLinkedAssembly != null)
            {
                LinkedAssemblies.Remove(SelectedLinkedAssembly);
            }
        }

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey(DialogParameterKeys.Record))
            {
                var record = parameters.GetValue<DocumentDetailRecord>(DialogParameterKeys.Record);
                if (parameters.ContainsKey(DialogParameterKeys.ActiveUserFullName))
                {
                    HandleFillBasedOnScenario(record);
                }
                else
                {
                    HandleEditScenario(record);
                }
            }
            else
            {
                HandleNewRecordScenario(parameters);
            }

            if (IsNewVersionEnabled)
            {
                UserMessage = "Выберите запись для копирования";
            }
        }

        private async void HandleEditScenario(DocumentDetailRecord record)
        {
            DocumentRecord = record;
            CompanyCode = DocumentRecord.ESKDNumber.CompanyCode;
            ClassNumberString = DocumentRecord.ESKDNumber.ClassNumber?.Number.ToString("D6");
            DetailNumber = DocumentRecord.ESKDNumber.DetailNumber;
            Version = DocumentRecord.ESKDNumber.Version;
            IsManualDetailNumberEnabled = DocumentRecord.IsManualDetailNumber;

            var linkedAssemblies = await _documentDataService.GetParentAssembliesForDetailAsync(DocumentRecord.Id);
            LinkedAssemblies = new ObservableCollection<Assembly>(linkedAssemblies);
        }

        private void HandleFillBasedOnScenario(DocumentDetailRecord record)
        {
            DocumentRecord = new DocumentDetailRecord
            {
                Date = DateTime.Now,
                FullName = _activeUserFullName,
                YASTCode = record.YASTCode,
                Name = record.Name,
                ESKDNumber = new ESKDNumber()
                {
                    CompanyCode = _settingsService.LoadSettings().DefaultCompanyCode,
                    ClassNumber = new Classifier { Number = record.ESKDNumber.ClassNumber.Number },
                }
            };

            CompanyCode = DocumentRecord.ESKDNumber.CompanyCode;
            ClassNumberString = DocumentRecord.ESKDNumber.ClassNumber?.Number.ToString("D6");
            FindNextDetailNumber();
            Version = null;
            IsManualDetailNumberEnabled = false;
        }

        private void HandleNewRecordScenario(IDialogParameters parameters)
        {
            if (parameters.ContainsKey(DialogParameterKeys.CompanyCode))
            {
                CompanyCode = parameters.GetValue<string>(DialogParameterKeys.CompanyCode);
                DocumentRecord.ESKDNumber.CompanyCode = CompanyCode;
            }
        }

        
    }
}

