#nullable enable

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
using ILogger = DetailViewer.Core.Interfaces.ILogger;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class AssemblyFormViewModel : BindableBase, IDialogAware
    {
        private readonly IAssemblyService _assemblyService;
        private readonly IProductService _productService;
        private readonly IClassifierService _classifierService;
        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private readonly IActiveUserService _activeUserService;
        private readonly IDialogService _dialogService;

        public string Title => "Форма сборки";

        public event Action<IDialogResult>? RequestClose;

        private Assembly _assembly;
        public Assembly Assembly
        {
            get { return _assembly; }
            set { SetProperty(ref _assembly, value); }
        }

        private ObservableCollection<Assembly> _parentAssemblies;
        public ObservableCollection<Assembly> ParentAssemblies
        {
            get { return _parentAssemblies; }
            set { SetProperty(ref _parentAssemblies, value); }
        }

        private ObservableCollection<Product> _relatedProducts;
        public ObservableCollection<Product> RelatedProducts
        {
            get { return _relatedProducts; }
            set { SetProperty(ref _relatedProducts, value); }
        }

        private string? _companyCode, _classNumberString;
        private int _detailNumber;
        private int? _version;

        public string? CompanyCode { get => _companyCode; set => SetProperty(ref _companyCode, value, OnESKDNumberPartChanged); }
        public string? ClassNumberString { get => _classNumberString; set => SetProperty(ref _classNumberString, value, OnClassNumberStringChanged); }
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

        private ObservableCollection<ClassifierData>? _allClassifiers;
        public ObservableCollection<ClassifierData>? AllClassifiers { get => _allClassifiers; set => SetProperty(ref _allClassifiers, value); }

        private ObservableCollection<ClassifierData>? _filteredClassifiers;
        public ObservableCollection<ClassifierData>? FilteredClassifiers { get => _filteredClassifiers; set => SetProperty(ref _filteredClassifiers, value); }

        private bool _isUpdatingFromSelection = false;
        private ClassifierData? _selectedClassifier;
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

        private List<Assembly>? _allAssemblies;
        private ObservableCollection<Assembly>? _filteredAssemblies;
        public ObservableCollection<Assembly>? FilteredAssemblies { get => _filteredAssemblies; set => SetProperty(ref _filteredAssemblies, value); }

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand AddParentAssemblyCommand { get; private set; }
        public DelegateCommand<Assembly> RemoveParentAssemblyCommand { get; private set; }
        public DelegateCommand AddRelatedProductCommand { get; private set; }
        public DelegateCommand<Product> RemoveRelatedProductCommand { get; private set; }


        public AssemblyFormViewModel(IAssemblyService assemblyService, IProductService productService, IClassifierService classifierService, ILogger logger, ISettingsService settingsService, IActiveUserService activeUserService, IDialogService dialogService)
        {
            _assemblyService = assemblyService;
            _productService = productService;
            _classifierService = classifierService;
            _logger = logger;
            _settingsService = settingsService;
            _activeUserService = activeUserService;
            _dialogService = dialogService;

            _assembly = new Assembly
            {
                EskdNumber = new ESKDNumber()
                {
                    ClassNumber = new Classifier()
                },
                Author = _activeUserService.CurrentUser?.ShortName
            };

            _parentAssemblies = new ObservableCollection<Assembly>();
            _relatedProducts = new ObservableCollection<Product>();

            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
            AddParentAssemblyCommand = new DelegateCommand(AddParentAssembly);
            RemoveParentAssemblyCommand = new DelegateCommand<Assembly>(RemoveParentAssembly);
            AddRelatedProductCommand = new DelegateCommand(AddRelatedProduct);
            RemoveRelatedProductCommand = new DelegateCommand<Product>(RemoveRelatedProduct);

            LoadAssemblies();
        }

        private void LoadClassifiers()
        {
            AllClassifiers = new ObservableCollection<ClassifierData>(_classifierService.GetAllClassifiers());
            FilterClassifiers();
        }

        private async void LoadAssemblies()
        {
            _allAssemblies = await _assemblyService.GetAssembliesAsync();
            FilterAssemblies();
        }

        private void FilterClassifiers()
        {
            if (AllClassifiers == null)
            {
                FilteredClassifiers = new ObservableCollection<ClassifierData>();
                return;
            }

            if (string.IsNullOrWhiteSpace(ClassNumberString))
            {
                FilteredClassifiers = new ObservableCollection<ClassifierData>(AllClassifiers);
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
                    if (r.EskdNumber != null && r.EskdNumber.ClassNumber != null)
                        return r.EskdNumber.ClassNumber.Number.ToString("D6").StartsWith(ClassNumberString);
                    else return false;
                    });
            }

            FilteredAssemblies = new ObservableCollection<Assembly>(records.OrderBy(r => r.EskdNumber.FullCode).ToList());
        }

        private void OnESKDNumberPartChanged() => RaisePropertyChanged(nameof(ESKDNumberString));

        private void OnClassNumberStringChanged()
        {
            if (_isUpdatingFromSelection) return;

            FilterClassifiers();
            FilterAssemblies();
            OnESKDNumberPartChanged();
        }

        private void AddParentAssembly()
        {
            _dialogService.ShowDialog("SelectAssemblyDialog", new DialogParameters(), r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var selectedAssemblies = r.Parameters.GetValue<List<Assembly>>(DialogParameterKeys.SelectedAssemblies);
                    foreach (var assembly in selectedAssemblies)
                    {
                        if (!ParentAssemblies.Any(p => p.Id == assembly.Id))
                        {
                            ParentAssemblies.Add(assembly);
                        }
                    }
                }
            });
        }

        private void RemoveParentAssembly(Assembly assembly)
        {
            if (assembly != null)
            {
                ParentAssemblies.Remove(assembly);
            }
        }

        private void AddRelatedProduct()
        {
            _dialogService.ShowDialog("SelectProductDialog", new DialogParameters(), r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var selectedProducts = r.Parameters.GetValue<List<Product>>(DialogParameterKeys.SelectedProducts);
                    foreach (var product in selectedProducts)
                    {
                        if (!RelatedProducts.Any(p => p.Id == product.Id))
                        {
                            RelatedProducts.Add(product);
                        }
                    }
                }
            });
        }

        private void RemoveRelatedProduct(Product product)
        {
            if (product != null)
            {
                RelatedProducts.Remove(product);
            }
        }

        private async void Save()
        {
            Assembly.EskdNumber.CompanyCode = CompanyCode;
            Assembly.EskdNumber.DetailNumber = DetailNumber;
            Assembly.EskdNumber.Version = Version;

            if (!string.IsNullOrWhiteSpace(ClassNumberString))
            {
                var classifier = _classifierService.GetClassifierByCode(ClassNumberString);
                if (classifier != null)
                {
                    Assembly.EskdNumber.ClassNumber = new Classifier { Number = int.Parse(classifier.Code), Description = classifier.Description ?? string.Empty };
                }
            }
            else
            {
                Assembly.EskdNumber.ClassNumber = null;
            }

            if (Assembly.Id == 0)
            {
                await _assemblyService.AddAssemblyAsync(Assembly);
            }
            else
            {
                await _assemblyService.UpdateAssemblyAsync(Assembly);
            }

            await _assemblyService.UpdateAssemblyParentAssembliesAsync(Assembly.Id, ParentAssemblies.ToList());
            await _assemblyService.UpdateAssemblyRelatedProductsAsync(Assembly.Id, RelatedProducts.ToList());

            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        private void Cancel() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public async void OnDialogOpened(IDialogParameters parameters)
        {
            await _classifierService.LoadClassifiersAsync();
            AllClassifiers = new ObservableCollection<ClassifierData>(_classifierService.GetAllClassifiers());

            if (parameters.ContainsKey("assembly"))
            {
                Assembly = parameters.GetValue<Assembly>("assembly");
                CompanyCode = Assembly.EskdNumber.CompanyCode;
                ClassNumberString = Assembly.EskdNumber.ClassNumber?.Number.ToString("D6");
                DetailNumber = Assembly.EskdNumber.DetailNumber;
                Version = Assembly.EskdNumber.Version;

                var parentAssemblies = await _assemblyService.GetParentAssembliesAsync(Assembly.Id);
                foreach(var item in parentAssemblies)
                {
                    ParentAssemblies.Add(item);
                }

                var relatedProducts = await _assemblyService.GetRelatedProductsAsync(Assembly.Id);
                foreach(var item in relatedProducts)
                {
                    RelatedProducts.Add(item);
                }
            }
            else
            {
                CompanyCode = _settingsService.LoadSettings().DefaultCompanyCode;
                Assembly.EskdNumber.CompanyCode = CompanyCode;
            }
        }
    }
}