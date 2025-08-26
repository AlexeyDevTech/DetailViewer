#nullable enable

using DetailViewer.Core.Services;
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
    public class DocumentRecordFormViewModel : BindableBase, IDialogAware
    {
        private readonly IDocumentRecordService _documentRecordService;
        private readonly IAssemblyService _assemblyService;
        private readonly IClassifierService _classifierService;
        private readonly IActiveUserService _activeUserService;
        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;

        private List<DocumentDetailRecord>? _allRecords;
        private ObservableCollection<Classifier>? _allClassifiers;
        private string? _activeUserFullName;

        public ObservableCollection<Classifier>? AllClassifiers { get => _allClassifiers; set => SetProperty(ref _allClassifiers, value); }
        public string Title => "Форма записи документа";
        public event Action<IDialogResult>? RequestClose;

        private DocumentDetailRecord _documentRecord;
        public DocumentDetailRecord DocumentRecord { get => _documentRecord; set => SetProperty(ref _documentRecord, value); }

        private string? _companyCode, _classNumberString;
        private int _detailNumber;
        private int? _version;

        public string? CompanyCode { get => _companyCode; set => SetProperty(ref _companyCode, value, OnESKDNumberPartChanged); }
        public string? ClassNumberString { get => _classNumberString; set => SetProperty(ref _classNumberString, value, OnClassNumberStringChanged); }
        public int DetailNumber { get => _detailNumber; set => SetProperty(ref _detailNumber, value, OnDetailNumberChanged); }
        public int? Version { get => _version; set => SetProperty(ref _version, value, OnESKDNumberPartChanged); }

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
        public bool IsManualDetailNumberEnabled { get => _isManualDetailNumberEnabled; set => SetProperty(ref _isManualDetailNumberEnabled, value); }
        public bool IsNewVersionEnabled { get => _isNewVersionEnabled; set => SetProperty(ref _isNewVersionEnabled, value, OnIsNewVersionEnabledChanged); }
        public bool FilterByFullName { get => _filterByFullName; set => SetProperty(ref _filterByFullName, value, FilterRecords); }

        private string? _userMessage;
        public string? UserMessage { get => _userMessage; set => SetProperty(ref _userMessage, value); }

        private ObservableCollection<Classifier>? _filteredClassifiers;
        public ObservableCollection<Classifier>? FilteredClassifiers { get => _filteredClassifiers; set => SetProperty(ref _filteredClassifiers, value); }

        private ObservableCollection<DocumentDetailRecord>? _filteredRecords;
        public ObservableCollection<DocumentDetailRecord>? FilteredRecords { get => _filteredRecords; set => SetProperty(ref _filteredRecords, value); }

        private bool _isUpdatingFromSelection = false;
        private Classifier? _selectedClassifier;
        public Classifier? SelectedClassifier
        {
            get => _selectedClassifier;
            set
            {
                SetProperty(ref _selectedClassifier, value);
                if (value != null)
                {
                    _isUpdatingFromSelection = true;
                    ClassNumberString = value.Number.ToString("D6");
                    _isUpdatingFromSelection = false;
                }
            }
        }

        private ObservableCollection<Assembly> _linkedAssemblies;
        public ObservableCollection<Assembly> LinkedAssemblies { get => _linkedAssemblies; set => SetProperty(ref _linkedAssemblies, value); }

        private Assembly? _selectedLinkedAssembly;
        public Assembly? SelectedLinkedAssembly { get => _selectedLinkedAssembly; set => SetProperty(ref _selectedLinkedAssembly, value); }

        private DocumentDetailRecord? _selectedRecordToCopy;
        public DocumentDetailRecord? SelectedRecordToCopy
        {
            get => _selectedRecordToCopy;
            set
            {
                SetProperty(ref _selectedRecordToCopy, value);
                if (value != null && IsNewVersionEnabled) CopyDataFromSelectedRecord(value);
            }
        }

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand AddAssemblyLinkCommand { get; private set; }
        public DelegateCommand RemoveAssemblyLinkCommand { get; private set; }

        public DocumentRecordFormViewModel(IDocumentRecordService documentRecordService, IAssemblyService assemblyService, IClassifierService classifierService, ILogger logger, IActiveUserService activeUserService, ISettingsService settingsService, IDialogService dialogService)
        {
            _documentRecordService = documentRecordService;
            _assemblyService = assemblyService;
            _classifierService = classifierService;
            _logger = logger;
            _activeUserService = activeUserService;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _activeUserFullName = _activeUserService.CurrentUser?.ShortName;

            _documentRecord = new DocumentDetailRecord
            {
                Date = DateTime.Now,
                FullName = _activeUserFullName,
                ESKDNumber = new ESKDNumber() { CompanyCode = _settingsService.LoadSettings().DefaultCompanyCode }
            };

            _linkedAssemblies = new ObservableCollection<Assembly>();

            SaveCommand = new DelegateCommand(Save, CanSave).ObservesProperty(() => ClassNumberString).ObservesProperty(() => DetailNumber);
            CancelCommand = new DelegateCommand(Cancel);
            AddAssemblyLinkCommand = new DelegateCommand(AddAssemblyLink);
            RemoveAssemblyLinkCommand = new DelegateCommand(RemoveAssemblyLink, () => SelectedLinkedAssembly != null).ObservesProperty(() => SelectedLinkedAssembly);

            LoadRecords();
        }

        private bool CanSave() => !string.IsNullOrWhiteSpace(ClassNumberString) && ClassNumberString.Length == 6 && DetailNumber > 0;

        private async void LoadRecords() => _allRecords = await _documentRecordService.GetAllRecordsAsync();

        private void LoadClassifiers() => AllClassifiers = new ObservableCollection<Classifier>(_classifierService.GetAllClassifiers());

        private void FilterRecords()
        {
            if (_allRecords == null || ClassNumberString?.Length != 6) { FilteredRecords = new ObservableCollection<DocumentDetailRecord>(); return; }
            var records = _allRecords.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(ClassNumberString)) records = records.Where(r => r.ESKDNumber?.ClassNumber?.Number.ToString("D6").StartsWith(ClassNumberString) ?? false);
            if (IsNewVersionEnabled && DetailNumber > 0 && SelectedRecordToCopy == null) records = records.Where(r => r.ESKDNumber?.ClassNumber?.Number.ToString("D6").StartsWith(ClassNumberString ?? string.Empty) == true);
            if (FilterByFullName && !string.IsNullOrEmpty(_activeUserFullName)) records = records.Where(r => r.FullName == _activeUserFullName);
            FilteredRecords = new ObservableCollection<DocumentDetailRecord>(records.OrderBy(r => r.ESKDNumber?.FullCode).ToList());
        }

        private void FilterClassifiers()
        {
            if (AllClassifiers == null) { FilteredClassifiers = new ObservableCollection<Classifier>(); return; }
            if (string.IsNullOrWhiteSpace(ClassNumberString)) { FilteredClassifiers = new ObservableCollection<Classifier>(AllClassifiers); return; }
            FilteredClassifiers = new ObservableCollection<Classifier>(AllClassifiers.Where(c => c.Number.ToString("D6").StartsWith(ClassNumberString, StringComparison.OrdinalIgnoreCase)).OrderBy(c => c.Number).ToList());
        }

        private void OnESKDNumberPartChanged() => RaisePropertyChanged(nameof(ESKDNumberString));

        private void OnClassNumberStringChanged()
        {
            if (_isUpdatingFromSelection) return;
            FilterClassifiers();
            FilterRecords();
            if (ClassNumberString?.Length == 6) FindNextDetailNumber();
            OnESKDNumberPartChanged();
            SaveCommand.RaiseCanExecuteChanged();
        }

        private void OnDetailNumberChanged()
        {
            FilterRecords();
            OnESKDNumberPartChanged();
            SaveCommand.RaiseCanExecuteChanged();
        }

        private void OnIsNewVersionEnabledChanged()
        {
            if (IsNewVersionEnabled) { IsManualDetailNumberEnabled = true; UserMessage = "Выберите запись для исполнения"; DocumentRecord.YASTCode = null; DocumentRecord.Name = null; SelectedRecordToCopy = null; } else { UserMessage = null; }
        }

        private void FindNextDetailNumber() => DetailNumber = (_allRecords == null || string.IsNullOrWhiteSpace(ClassNumberString)) ? 0 : EskdNumberHelper.FindNextDetailNumber(_allRecords, ClassNumberString);

        private void FindNextVersionNumber() => Version = (_allRecords == null || SelectedRecordToCopy == null) ? null : EskdNumberHelper.FindNextVersionNumber(_allRecords, SelectedRecordToCopy);

        private void CopyDataFromSelectedRecord(DocumentDetailRecord sourceRecord)
        {
            if (sourceRecord.ESKDNumber == null) return;
            DocumentRecord.YASTCode = sourceRecord.YASTCode;
            DocumentRecord.Name = sourceRecord.Name;
            CompanyCode = sourceRecord.ESKDNumber.CompanyCode;
            ClassNumberString = sourceRecord.ESKDNumber.ClassNumber?.Number.ToString("D6");
            DetailNumber = sourceRecord.ESKDNumber.DetailNumber;
            FindNextVersionNumber();
            RaisePropertyChanged(string.Empty);
            UserMessage = null;
        }

        private async void Save()
        {
            if (DocumentRecord.ESKDNumber == null) DocumentRecord.ESKDNumber = new ESKDNumber();
            DocumentRecord.ESKDNumber.CompanyCode = CompanyCode;
            DocumentRecord.ESKDNumber.DetailNumber = DetailNumber;
            DocumentRecord.ESKDNumber.Version = Version;
            if (int.TryParse(ClassNumberString, out int classNumberValue))
            {
                var classifier = _classifierService.GetClassifierByNumber(classNumberValue);
                if (classifier != null) { DocumentRecord.ESKDNumber.ClassifierId = classifier.Id; DocumentRecord.ESKDNumber.ClassNumber = null; }
            }
            if (DocumentRecord.Id == 0) await _documentRecordService.AddRecordAsync(DocumentRecord, DocumentRecord.ESKDNumber, LinkedAssemblies.Select(a => a.Id).ToList());
            else await _documentRecordService.UpdateRecordAsync(DocumentRecord, LinkedAssemblies.Select(a => a.Id).ToList());
            RequestClose?.Invoke(BuildDialogResult());
        }

        private IDialogResult BuildDialogResult()
        {
            var result = new DialogResult(ButtonResult.OK);
            result.Parameters.Add("record", DocumentRecord);
            result.Parameters.Add("linkedAssemblies", LinkedAssemblies?.ToList() ?? new List<Assembly>());
            return result;
        }

        private void Cancel() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));

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

        private void RemoveAssemblyLink() { if (SelectedLinkedAssembly != null) LinkedAssemblies.Remove(SelectedLinkedAssembly); }

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }

        public async void OnDialogOpened(IDialogParameters parameters)
        {
            await _classifierService.LoadClassifiersAsync();
            LoadClassifiers();
            if (parameters.ContainsKey(DialogParameterKeys.Record))
            {
                var record = parameters.GetValue<DocumentDetailRecord>(DialogParameterKeys.Record);
                if (record != null)
                {
                    if (parameters.ContainsKey(DialogParameterKeys.ActiveUserFullName)) HandleFillBasedOnScenario(record);
                    else await HandleEditScenario(record);
                }
            }
            else HandleNewRecordScenario(parameters);
            if (IsNewVersionEnabled) UserMessage = "Выберите запись для копирования";
        }

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
                LinkedAssemblies = new ObservableCollection<Assembly>(linkedAssemblies);
            }
        }

        private void HandleFillBasedOnScenario(DocumentDetailRecord record)
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
            FindNextDetailNumber();
            Version = null;
            IsManualDetailNumberEnabled = false;
        }

        private void HandleNewRecordScenario(IDialogParameters parameters)
        {
            if (parameters.ContainsKey(DialogParameterKeys.CompanyCode)) { CompanyCode = parameters.GetValue<string>(DialogParameterKeys.CompanyCode); if (DocumentRecord.ESKDNumber != null) DocumentRecord.ESKDNumber.CompanyCode = CompanyCode; }
        }
    }
}