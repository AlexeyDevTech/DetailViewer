using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class DocumentRecordFormViewModel : BindableBase, IDialogAware
    {
        private readonly IDocumentDataService _documentDataService;
        private readonly IActiveUserService _activeUserService;
        private readonly ILogger _logger;
        private List<DocumentRecord> _allRecords;
        private string _activeUserFullName;

        public string Title => "Форма записи документа";

        public event Action<IDialogResult> RequestClose;

        private DocumentRecord _documentRecord;
        public DocumentRecord DocumentRecord
        {
            get { return _documentRecord; }
            set { SetProperty(ref _documentRecord, value); }
        }

        private string _companyCode;
        public string CompanyCode
        {
            get => _companyCode;
            set => SetProperty(ref _companyCode, value, OnESKDNumberPartChanged);
        }

        private string _classNumberString;
        public string ClassNumberString
        {
            get => _classNumberString;
            set => SetProperty(ref _classNumberString, value, OnClassNumberStringChanged);
        }

        private int _detailNumber;
        public int DetailNumber
        {
            get => _detailNumber;
            set => SetProperty(ref _detailNumber, value, OnDetailNumberChanged);
        }

        private void OnDetailNumberChanged()
        {
            FilterRecords();
            OnESKDNumberPartChanged();
        }

        private int? _version;
        public int? Version
        {
            get => _version;
            set => SetProperty(ref _version, value, OnESKDNumberPartChanged);
        }

        private bool _isManualDetailNumberEnabled;
        public bool IsManualDetailNumberEnabled
        {
            get => _isManualDetailNumberEnabled;
            set => SetProperty(ref _isManualDetailNumberEnabled, value);
        }

        private bool _isNewVersionEnabled;
        public bool IsNewVersionEnabled
        {
            get => _isNewVersionEnabled;
            set => SetProperty(ref _isNewVersionEnabled, value, OnIsNewVersionEnabledChanged);
        }

        private void OnIsNewVersionEnabledChanged()
        {
            if (IsNewVersionEnabled)
            {
                IsManualDetailNumberEnabled = false;
            }
        }

        private bool _filterByFullName = true;
        public bool FilterByFullName
        {
            get => _filterByFullName;
            set => SetProperty(ref _filterByFullName, value, FilterRecords);
        }

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

        private ObservableCollection<ClassifierData> _allClassifiers;
        public ObservableCollection<ClassifierData> AllClassifiers
        {
            get => _allClassifiers;
            set => SetProperty(ref _allClassifiers, value);
        }

        private ObservableCollection<ClassifierData> _filteredClassifiers;
        public ObservableCollection<ClassifierData> FilteredClassifiers
        {
            get => _filteredClassifiers;
            set => SetProperty(ref _filteredClassifiers, value);
        }

        private ClassifierData _selectedClassifier;
        public ClassifierData SelectedClassifier
        {
            get => _selectedClassifier;
            set
            {
                SetProperty(ref _selectedClassifier, value);
                if (value != null)
                {
                    ClassNumberString = value.Code;
                }
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
                    CompanyCode = value.ESKDNumber.CompanyCode;
                    ClassNumberString = value.ESKDNumber.ClassNumber.Number.ToString("D6");
                    DetailNumber = value.ESKDNumber.DetailNumber;
                    DocumentRecord.YASTCode = value.YASTCode;
                    DocumentRecord.Name = value.Name;
                    DocumentRecord.AssemblyNumber = value.AssemblyNumber;
                    DocumentRecord.AssemblyName = value.AssemblyName;
                    DocumentRecord.ProductNumber = value.ProductNumber;
                    DocumentRecord.ProductName = value.ProductName;
                    FindNextVersionNumber();
                }
            }
        }

        private ObservableCollection<DocumentRecord> _filteredRecords;
        public ObservableCollection<DocumentRecord> FilteredRecords
        {
            get => _filteredRecords;
            set => SetProperty(ref _filteredRecords, value);
        }

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public DocumentRecordFormViewModel(IDocumentDataService documentDataService, ILogger logger, IActiveUserService activeUserService)
        {
            _documentDataService = documentDataService;
            _logger = logger;
            _activeUserService = activeUserService;

            _activeUserFullName = _activeUserService.CurrentUser?.ShortName;

            DocumentRecord = new DocumentRecord { Date = DateTime.Now, FullName = _activeUserFullName, ESKDNumber = new ESKDNumber() { ClassNumber = new Classifier() } };
            
            CompanyCode = DocumentRecord.ESKDNumber.CompanyCode;
            ClassNumberString = DocumentRecord.ESKDNumber.ClassNumber.Number.ToString("D6");
            DetailNumber = DocumentRecord.ESKDNumber.DetailNumber;
            Version = DocumentRecord.ESKDNumber.Version;

            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);

            LoadClassifiers();
            LoadRecords();
        }

        private async void LoadRecords()
        {
            _allRecords = await _documentDataService.GetAllRecordsAsync();
            FilterRecords();
        }

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

            if (IsNewVersionEnabled && DetailNumber > 0)
            {
                records = records.Where(r => r.ESKDNumber.DetailNumber == DetailNumber);
            }

            if (FilterByFullName)
            {
                records = records.Where(r => r.FullName == _activeUserFullName);
            }

            FilteredRecords = new ObservableCollection<DocumentRecord>(records.OrderBy(r => r.ESKDNumber.FullCode).ToList());
        }


        private void OnESKDNumberPartChanged()
        {
            RaisePropertyChanged(nameof(ESKDNumberString));
        }

        private void OnClassNumberStringChanged()
        {
            FilterClassifiers();
            FilterRecords();
            if (ClassNumberString?.Length == 6)
            {
                FindNextDetailNumber();
            }
            OnESKDNumberPartChanged();
        }

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
            if (_allRecords == null) return;

            var existingVersions = _allRecords
                .Where(r => r.ESKDNumber.ClassNumber.Number.ToString("D6") == ClassNumberString && r.ESKDNumber.DetailNumber == DetailNumber)
                .Select(r => r.ESKDNumber.Version)
                .ToHashSet();

            for (int i = 1; i <= 99; i++)
            {
                if (!existingVersions.Contains(i))
                {
                    Version = i;
                    return;
                }
            }
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

        private void Save()
        {
            try
            {
                DocumentRecord.ESKDNumber.CompanyCode = CompanyCode;
                DocumentRecord.ESKDNumber.ClassNumber = new Classifier { Number = int.Parse(ClassNumberString) };
                DocumentRecord.ESKDNumber.DetailNumber = DetailNumber;
                DocumentRecord.ESKDNumber.Version = Version;
                DocumentRecord.IsManualDetailNumber = IsNewVersionEnabled ? false : IsManualDetailNumberEnabled;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка валидации или формата: {ex.Message}", ex);
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

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("record"))
            {
                DocumentRecord = parameters.GetValue<DocumentRecord>("record");
                CompanyCode = DocumentRecord.ESKDNumber.CompanyCode;
                ClassNumberString = DocumentRecord.ESKDNumber.ClassNumber.Number.ToString("D6");
                DetailNumber = DocumentRecord.ESKDNumber.DetailNumber;
                Version = DocumentRecord.ESKDNumber.Version;
                IsManualDetailNumberEnabled = DocumentRecord.IsManualDetailNumber;
            }
        }
    }
}
