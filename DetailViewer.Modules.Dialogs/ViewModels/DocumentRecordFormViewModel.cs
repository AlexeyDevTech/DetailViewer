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
using System.Threading.Tasks; // Added for async operations

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class DocumentRecordFormViewModel : BindableBase, IDialogAware
    {
        public string Title => "Форма записи документа";

        public event Action<IDialogResult> RequestClose;

        private DocumentRecord _documentRecord;
        public DocumentRecord DocumentRecord
        {
            get { return _documentRecord; }
            set { SetProperty(ref _documentRecord, value); }
        }

        // New properties for ESKD number breakdown
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
            set => SetProperty(ref _detailNumber, value, OnESKDNumberPartChanged);
        }

        private int? _version;
        public int? Version
        {
            get => _version;
            set => SetProperty(ref _version, value, OnESKDNumberPartChanged);
        }

        // Computed property for the full ESKD number string
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

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public DocumentRecordFormViewModel(IProfileService profileService, ISettingsService settingsService)
        {
            var settings = settingsService.LoadSettings();
            var activeProfile = profileService.GetAllProfilesAsync().Result.FirstOrDefault(p => p.Id == settings.ActiveProfileId);

            DocumentRecord = new DocumentRecord { Date = DateTime.Now, ESKDNumber = new ESKDNumber() { ClassNumber = new Classifier() } };
            if (activeProfile != null)
            {
                DocumentRecord.FullName = $"{activeProfile.LastName} {activeProfile.FirstName.FirstOrDefault()}.{activeProfile.MiddleName.FirstOrDefault()}.";
            }

            // Initialize new properties from DocumentRecord.ESKDNumber
            CompanyCode = DocumentRecord.ESKDNumber.CompanyCode;

            ClassNumberString = DocumentRecord.ESKDNumber.ClassNumber.Number.ToString("D6");
            DetailNumber = DocumentRecord.ESKDNumber.DetailNumber;
            Version = DocumentRecord.ESKDNumber.Version;

            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);

            LoadClassifiers();
        }

        private void OnESKDNumberPartChanged()
        {
            RaisePropertyChanged(nameof(ESKDNumberString));
        }

        private void OnClassNumberStringChanged()
        {
            FilterClassifiers();
            OnESKDNumberPartChanged();
        }

        private async void LoadClassifiers()
        {
            try
            {
                // Assuming eskd_classifiers.json is in the root directory C:\AI
                string jsonFilePath = "eskd_classifiers.json";
                if (File.Exists(jsonFilePath))
                {
                    string jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                    var rootClassifiers = JsonSerializer.Deserialize<List<ClassifierData>>(jsonContent);
                    AllClassifiers = new ObservableCollection<ClassifierData>(FlattenClassifiers(rootClassifiers));
                    FilterClassifiers(); // Initial filtering
                }
                else
                {
                    Debug.WriteLine($"eskd_classifiers.json not found at {jsonFilePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading classifiers: {ex.Message}");
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
                              .OrderBy(c => c.Code.Length) // Sort by length of the code
                              .ThenBy(c => c.Code) // Then by the code itself
                              .ToList()
            );
        }

        private void Save()
        {
            try
            {
                // Construct the ESKDNumber from individual fields
                DocumentRecord.ESKDNumber.CompanyCode = CompanyCode;
                DocumentRecord.ESKDNumber.ClassNumber = new Classifier { Number = int.Parse(ClassNumberString) };
                DocumentRecord.ESKDNumber.DetailNumber = DetailNumber;
                DocumentRecord.ESKDNumber.Version = Version;

                // Validate the constructed ESKD number
                // This might be redundant if individual fields are validated, but good for a final check
                // DocumentRecord.ESKDNumber.SetCode(ESKDNumberString); // This line is no longer needed as we are setting properties directly

            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"Ошибка валидации: {ex.Message}");
                return;
            }
            catch (FormatException ex)
            {
                Debug.WriteLine($"Ошибка формата числа: {ex.Message}");
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

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            // Clean up if necessary
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("record"))
            {
                DocumentRecord = parameters.GetValue<DocumentRecord>("record");
                // Populate individual fields from existing DocumentRecord.ESKDNumber
                CompanyCode = DocumentRecord.ESKDNumber.CompanyCode;
                ClassNumberString = DocumentRecord.ESKDNumber.ClassNumber.Number.ToString("D6");
                DetailNumber = DocumentRecord.ESKDNumber.DetailNumber;
                Version = DocumentRecord.ESKDNumber.Version;
            }
        }
    }
}