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

        // Data collections
        private List<DocumentRecord> _allRecords;
        private ObservableCollection<ClassifierData> _allClassifiers;

        // Active User Info
        private string _activeUserFullName;

        public ObservableCollection<ClassifierData> AllClassifiers { get => _allClassifiers; set => SetProperty(ref _allClassifiers, value); }

        public string Title => "Форма записи документа";
        public event Action<IDialogResult> RequestClose;

        private DocumentRecord _documentRecord;
        public DocumentRecord DocumentRecord
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

        // --- Assembly Number Parts ---
        private string _assemblyPart1, _assemblyPart2, _assemblyPart3, _assemblyPart4;
        public string AssemblyPart1 { get => _assemblyPart1; set => SetProperty(ref _assemblyPart1, value, UpdateAssemblyNumber); }
        public string AssemblyPart2 { get => _assemblyPart2; set => SetProperty(ref _assemblyPart2, value, UpdateAssemblyNumber); }
        public string AssemblyPart3 { get => _assemblyPart3; set => SetProperty(ref _assemblyPart3, value, UpdateAssemblyNumber); }
        public string AssemblyPart4 { get => _assemblyPart4; set => SetProperty(ref _assemblyPart4, value, UpdateAssemblyNumber); }

        // --- Product Number Parts ---
        private string _productPart1, _productPart2, _productPart3, _productPart4;
        public string ProductPart1 { get => _productPart1; set => SetProperty(ref _productPart1, value, UpdateProductNumber); }
        public string ProductPart2 { get => _productPart2; set => SetProperty(ref _productPart2, value, UpdateProductNumber); }
        public string ProductPart3 { get => _productPart3; set => SetProperty(ref _productPart3, value, UpdateProductNumber); }
        public string ProductPart4 { get => _productPart4; set => SetProperty(ref _productPart4, value, UpdateProductNumber); }

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

        // --- Filtered Collections for UI ---
        private ObservableCollection<ClassifierData> _filteredClassifiers;
        public ObservableCollection<ClassifierData> FilteredClassifiers { get => _filteredClassifiers; set => SetProperty(ref _filteredClassifiers, value); }

        private ObservableCollection<DocumentRecord> _filteredRecords;
        public ObservableCollection<DocumentRecord> FilteredRecords { get => _filteredRecords; set => SetProperty(ref _filteredRecords, value); }

        // --- Selected Items ---
        private ClassifierData _selectedClassifier;
        public ClassifierData SelectedClassifier
        {
            get => _selectedClassifier;
            set
            {
                SetProperty(ref _selectedClassifier, value);
                if (value != null) ClassNumberString = value.Code;
            }
        }

        private DocumentRecord _selectedRecordToCopy;
        public DocumentRecord SelectedRecordToCopy
        {
            get => _selectedRecordToCopy;
            set
            {
                SetProperty(ref _selectedRecordToCopy, value);
                if (value != null && IsNewVersionEnabled)
                {
                    // Logic to copy from selected record
                }
            }
        }

        // --- Commands ---
        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        // --- Constructor ---
        public DocumentRecordFormViewModel(IDocumentDataService documentDataService, ILogger logger, IActiveUserService activeUserService, ISettingsService settingsService)
        {
            _documentDataService = documentDataService;
            _logger = logger;
            _activeUserService = activeUserService;
            _settingsService = settingsService;
            _activeUserFullName = _activeUserService.CurrentUser?.ShortName;

            DocumentRecord = new DocumentRecord
            {
                Date = DateTime.Now,
                FullName = _activeUserFullName,
                ESKDNumber = new ESKDNumber()
                {
                    ClassNumber = new Classifier(),
                    CompanyCode = _settingsService.LoadSettings().DefaultCompanyCode // Set default company code here
                }
            };

            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);

            LoadClassifiers();
            LoadRecords();
        }

        // --- Methods for Assembling/Disassembling Composite Numbers ---
        private void UpdateAssemblyNumber() => DocumentRecord.AssemblyNumber = $"{AssemblyPart1}.{AssemblyPart2}.{AssemblyPart3}-{AssemblyPart4}";
        private void UpdateProductNumber() => DocumentRecord.ProductNumber = $"{ProductPart1}.{ProductPart2}.{ProductPart3}-{ProductPart4}";

        private void ParseAndSetParts(string fullNumber, Action<string, string, string, string> setter)
        {
            if (string.IsNullOrWhiteSpace(fullNumber))
            {
                setter(null, null, null, null);
                return;
            }
            var parts = fullNumber.Replace('-', '.').Split('.');
            setter(
                parts.Length > 0 ? parts[0] : null,
                parts.Length > 1 ? parts[1] : null,
                parts.Length > 2 ? parts[2] : null,
                parts.Length > 3 ? parts[3] : null
            );
        }

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
                FilteredRecords = new ObservableCollection<DocumentRecord>();
                return;
            }

            var records = _allRecords.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(ClassNumberString))
            {
                records = records.Where(r => r.ESKDNumber.ClassNumber.Number.ToString("D6").StartsWith(ClassNumberString));
            }

            if (IsNewVersionEnabled && DetailNumber > 0 && SelectedRecordToCopy == null)
            {
                records = records.Where(r => r.ESKDNumber.DetailNumber == DetailNumber);
            }

            if (FilterByFullName)
            {
                records = records.Where(r => r.FullName == _activeUserFullName);
            }

            FilteredRecords = new ObservableCollection<DocumentRecord>(records.OrderBy(r => r.ESKDNumber.FullCode).ToList());
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
            }
        }

        // --- Business Logic ---
        private void FindNextDetailNumber()
        {
            if (_allRecords == null) return;
            var existingDetailNumbers = _allRecords
                .Where(r => r.ESKDNumber.ClassNumber.Number.ToString("D6") == ClassNumberString)
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

        // --- Dialog-related Methods ---
        private void Save()
        {
            UpdateAssemblyNumber();
            UpdateProductNumber();
            // ... other save logic ...
            var result = new DialogResult(ButtonResult.OK);
            result.Parameters.Add("record", DocumentRecord);
            RequestClose?.Invoke(result);
        }

        private void Cancel() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("record"))
            {
                var record = parameters.GetValue<DocumentRecord>("record");
                bool isFillBasedOn = parameters.ContainsKey("activeUserFullName");

                if (isFillBasedOn)
                {
                    // Create a NEW record, copying data from the passed one.
                    DocumentRecord = new DocumentRecord
                    {
                        Date = DateTime.Now,
                        FullName = _activeUserFullName, // Use active user's name
                        YASTCode = record.YASTCode,
                        Name = record.Name,
                        AssemblyNumber = record.AssemblyNumber,
                        AssemblyName = record.AssemblyName,
                        ProductNumber = record.ProductNumber,
                        ProductName = record.ProductName,
                        ESKDNumber = new ESKDNumber()
                        {
                            CompanyCode = _settingsService.LoadSettings().DefaultCompanyCode, // Set default company code here
                            ClassNumber = new Classifier { Number = record.ESKDNumber.ClassNumber.Number },
                        }
                    };

                    // Find the next available detail number instead of copying it
                    FindNextDetailNumber();
                    Version = null; // It's a new detail, not a new version
                    IsManualDetailNumberEnabled = false;
                }
                else // This is the "Edit" case
                {
                    DocumentRecord = record;
                }
            }

            // Populate the ViewModel properties from the DocumentRecord (always executed)
            CompanyCode = DocumentRecord.ESKDNumber.CompanyCode;
            ClassNumberString = DocumentRecord.ESKDNumber.ClassNumber?.Number.ToString("D6");
            DetailNumber = DocumentRecord.ESKDNumber.DetailNumber;
            Version = DocumentRecord.ESKDNumber.Version;
            IsManualDetailNumberEnabled = DocumentRecord.IsManualDetailNumber;

            // Parse the composite numbers for Assembly and Product (always executed)
            ParseAndSetParts(DocumentRecord.AssemblyNumber, (p1, p2, p3, p4) => { AssemblyPart1 = p1; AssemblyPart2 = p2; AssemblyPart3 = p3; AssemblyPart4 = p4; });
            ParseAndSetParts(DocumentRecord.ProductNumber, (p1, p2, p3, p4) => { ProductPart1 = p1; ProductPart2 = p2; ProductPart3 = p3; ProductPart4 = p4; });
        }
    }
}
