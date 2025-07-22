
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
    public class AssemblyFormViewModel : BindableBase, IDialogAware
    {
        private readonly IDocumentDataService _documentDataService;
        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private readonly IActiveUserService _activeUserService;

        public string Title => "Форма сборки";

        public event Action<IDialogResult> RequestClose;

        private Assembly _assembly;
        public Assembly Assembly
        {
            get { return _assembly; }
            set { SetProperty(ref _assembly, value); }
        }

        private string _companyCode, _classNumberString;
        private int _detailNumber;
        private int? _version;

        public string CompanyCode { get => _companyCode; set => SetProperty(ref _companyCode, value, OnESKDNumberPartChanged); }
        public string ClassNumberString { get => _classNumberString; set => SetProperty(ref _classNumberString, value, OnClassNumberStringChanged); }
        public int DetailNumber { get => _detailNumber; set => SetProperty(ref _detailNumber, value, OnESKDNumberPartChanged); }
        public int? Version { get => _version; set => SetProperty(ref _version, value, OnESKDNumberPartChanged); }

        public string ESKDNumberString
        {
            get
            {
                if (string.IsNullOrEmpty(CompanyCode) || string.IsNullOrEmpty(ClassNumberString) || DetailNumber == 0)
                {
                    return string.Empty;
                }

                try
                {
                    string baseCode = $"{CompanyCode}.{int.Parse(ClassNumberString):D6}.{DetailNumber:D3}";
                    return Version.HasValue ? $"{baseCode}-{Version.Value:D2}" : baseCode;
                }
                catch (FormatException)
                {
                    return "Invalid ClassNumber format";
                }
            }
        }

        private ObservableCollection<ClassifierData> _allClassifiers;
        public ObservableCollection<ClassifierData> AllClassifiers { get => _allClassifiers; set => SetProperty(ref _allClassifiers, value); }

        private ObservableCollection<ClassifierData> _filteredClassifiers;
        public ObservableCollection<ClassifierData> FilteredClassifiers { get => _filteredClassifiers; set => SetProperty(ref _filteredClassifiers, value); }

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

        private List<Assembly> _allAssemblies;
        private ObservableCollection<Assembly> _filteredAssemblies;
        public ObservableCollection<Assembly> FilteredAssemblies { get => _filteredAssemblies; set => SetProperty(ref _filteredAssemblies, value); }

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public AssemblyFormViewModel(IDocumentDataService documentDataService, ILogger logger, ISettingsService settingsService, IActiveUserService activeUserService)
        {
            _documentDataService = documentDataService;
            _logger = logger;
            _settingsService = settingsService;
            _activeUserService = activeUserService;

            Assembly = new Assembly
            {
                EskdNumber = new ESKDNumber()
                {
                    ClassNumber = new Classifier()
                },
                Author = _activeUserService.CurrentUser?.ShortName
            };

            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);

            LoadClassifiers();
            LoadAssemblies();
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

        private async void LoadAssemblies()
        {
            _allAssemblies = await _documentDataService.GetAssembliesAsync();
            FilterAssemblies();
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

        private void FilterAssemblies()
        {
            if (_allAssemblies == null || ClassNumberString?.Length != 6)
            {
                FilteredAssemblies = new ObservableCollection<Assembly>();
                return;
            }

            var records = _allAssemblies.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(ClassNumberString))
            {

                records = records.Where(r => {
                    if (r.EskdNumber != null)
                        return r.EskdNumber.ClassNumber.Number.ToString("D6").StartsWith(ClassNumberString);
                    else return false;
                    });
            }

            FilteredAssemblies = new ObservableCollection<Assembly>(records.OrderBy(r => r.EskdNumber.FullCode).ToList());
        }

        private void OnESKDNumberPartChanged() => RaisePropertyChanged(nameof(ESKDNumberString));

        private void OnClassNumberStringChanged()
        {
            FilterClassifiers();
            FilterAssemblies();
            OnESKDNumberPartChanged();
        }

        private async void Save()
        {
            Assembly.EskdNumber.CompanyCode = CompanyCode;
            Assembly.EskdNumber.DetailNumber = DetailNumber;
            Assembly.EskdNumber.Version = Version;

            if (!string.IsNullOrWhiteSpace(ClassNumberString))
            {
                var classifier = await _documentDataService.GetOrCreateClassifierAsync(ClassNumberString);
                Assembly.EskdNumber.ClassNumber = classifier;
            }
            else
            {
                Assembly.EskdNumber.ClassNumber = null;
            }

            if (Assembly.Id == 0)
            {
                await _documentDataService.AddAssemblyAsync(Assembly);
            }
            else
            {
                await _documentDataService.UpdateAssemblyAsync(Assembly);
            }

            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        private void Cancel() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("assembly"))
            {
                Assembly = parameters.GetValue<Assembly>("assembly");
                CompanyCode = Assembly.EskdNumber.CompanyCode;
                ClassNumberString = Assembly.EskdNumber.ClassNumber?.Number.ToString("D6");
                DetailNumber = Assembly.EskdNumber.DetailNumber;
                Version = Assembly.EskdNumber.Version;
            }
            else
            {
                CompanyCode = _settingsService.LoadSettings().DefaultCompanyCode;
                Assembly.EskdNumber.CompanyCode = CompanyCode;
            }
        }
    }
}

