
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
    public class ProductFormViewModel : BindableBase, IDialogAware
    {
        private readonly IDocumentDataService _documentDataService;
        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private readonly IActiveUserService _activeUserService;
        private readonly IDialogService _dialogService;

        public string Title => "Форма изделия";

        public event Action<IDialogResult> RequestClose;

        private Product _product;
        public Product Product
        {
            get { return _product; }
            set { SetProperty(ref _product, value); }
        }

        private ObservableCollection<Assembly> _parentAssemblies;
        public ObservableCollection<Assembly> ParentAssemblies
        {
            get { return _parentAssemblies; }
            set { SetProperty(ref _parentAssemblies, value); }
        }

        private ObservableCollection<Product> _parentProducts;
        public ObservableCollection<Product> ParentProducts
        {
            get { return _parentProducts; }
            set { SetProperty(ref _parentProducts, value); }
        }

        private string _companyCode, _classNumberString, _productName, _productMaterial;
        private int _detailNumber;
        private int? _version;

        public string CompanyCode { get => _companyCode; set => SetProperty(ref _companyCode, value, OnESKDNumberPartChanged); }
        public string ClassNumberString { get => _classNumberString; set => SetProperty(ref _classNumberString, value, OnClassNumberStringChanged); }
        public int DetailNumber { get => _detailNumber; set => SetProperty(ref _detailNumber, value, OnESKDNumberPartChanged); }
        public int? Version { get => _version; set => SetProperty(ref _version, value, OnESKDNumberPartChanged); }
        public string ProductName { get => _productName; set => SetProperty(ref _productName, value); }
        public string ProductMaterial { get => _productMaterial; set => SetProperty(ref _productMaterial, value); }

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

        private List<Product> _allProducts;
        private ObservableCollection<Product> _filteredProducts;
        public ObservableCollection<Product> FilteredProducts { get => _filteredProducts; set => SetProperty(ref _filteredProducts, value); }

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand AddParentAssemblyCommand { get; private set; }
        public DelegateCommand<Assembly> RemoveParentAssemblyCommand { get; private set; }
        public DelegateCommand AddParentProductCommand { get; private set; }
        public DelegateCommand<Product> RemoveParentProductCommand { get; private set; }

        public ProductFormViewModel(IDocumentDataService documentDataService, ILogger logger, ISettingsService settingsService, IActiveUserService activeUserService, IDialogService dialogService)
        {
            _documentDataService = documentDataService;
            _logger = logger;
            _settingsService = settingsService;
            _activeUserService = activeUserService;
            _dialogService = dialogService;

            Product = new Product
            {
                EskdNumber = new ESKDNumber()
                {
                    ClassNumber = new Classifier()
                },
                Author = _activeUserService.CurrentUser?.ShortName
            };

            ParentAssemblies = new ObservableCollection<Assembly>();
            ParentProducts = new ObservableCollection<Product>();

            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
            AddParentAssemblyCommand = new DelegateCommand(AddParentAssembly);
            RemoveParentAssemblyCommand = new DelegateCommand<Assembly>(RemoveParentAssembly);
            AddParentProductCommand = new DelegateCommand(AddParentProduct);
            RemoveParentProductCommand = new DelegateCommand<Product>(RemoveParentProduct);

            LoadClassifiers();
            LoadProducts();
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

        private async void LoadProducts()
        {
            _allProducts = await _documentDataService.GetProductsAsync();
            FilterProducts();
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

        private void FilterProducts()
        {
            if (_allProducts == null || ClassNumberString?.Length != 6)
            {
                FilteredProducts = new ObservableCollection<Product>();
                return;
            }

            var records = _allProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(ClassNumberString))
            {
                records = records.Where(r => r.EskdNumber.ClassNumber.Number.ToString("D6").StartsWith(ClassNumberString));
            }

            FilteredProducts = new ObservableCollection<Product>(records.OrderBy(r => r.EskdNumber.FullCode).ToList());
        }

        private void OnESKDNumberPartChanged() => RaisePropertyChanged(nameof(ESKDNumberString));

        private void OnClassNumberStringChanged()
        {
            FilterClassifiers();
            FilterProducts();
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

        private void AddParentProduct()
        {
            _dialogService.ShowDialog("SelectProductDialog", new DialogParameters(), r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var selectedProducts = r.Parameters.GetValue<List<Product>>(DialogParameterKeys.SelectedProducts);
                    foreach (var product in selectedProducts)
                    {
                        if (!ParentProducts.Any(p => p.Id == product.Id))
                        {
                            ParentProducts.Add(product);
                        }
                    }
                }
            });
        }

        private void RemoveParentProduct(Product product)
        {
            if (product != null)
            {
                ParentProducts.Remove(product);
            }
        }

        private async void Save()
        {
            var eskdNumber = new ESKDNumber
            {
                CompanyCode = CompanyCode,
                DetailNumber = DetailNumber,
                Version = Version
            };

            if (!string.IsNullOrWhiteSpace(ClassNumberString))
            {
                eskdNumber.ClassNumber = await _documentDataService.GetOrCreateClassifierAsync(ClassNumberString);
                Product.EskdNumber = eskdNumber;
            }

            if (ParentProducts.Any())
            {
                // Конвертация продукта в сборку
                var newAssembly = new Assembly
                {
                    Name = Product.Name,
                    Material = Product.Material,
                    Author = _activeUserService.CurrentUser?.ShortName,
                    EskdNumber = eskdNumber
                };

                await _documentDataService.AddAssemblyAsync(newAssembly);

                if (Product.Id != 0)
                    await _documentDataService.DeleteProductAsync(Product.Id);
                await Task.WhenAll(
                    _documentDataService.UpdateAssemblyParentAssembliesAsync(newAssembly.Id, ParentAssemblies.ToList()),
                    _documentDataService.UpdateAssemblyRelatedProductsAsync(newAssembly.Id, ParentProducts.ToList())
                );
            }
            else
            {
                if (Product.Id == 0)
                    await _documentDataService.CreateProductWithAssembliesAsync(Product, ParentAssemblies.ToList());
                else
                {
                    await _documentDataService.UpdateProductAsync(Product);
                    await _documentDataService.UpdateProductParentAssembliesAsync(Product.Id, ParentAssemblies.ToList());
                }
            }

            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        private void Cancel() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public async void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey(DialogParameterKeys.Product))
            {
                Product = parameters.GetValue<Product>(DialogParameterKeys.Product);
                CompanyCode = Product.EskdNumber.CompanyCode;
                ClassNumberString = Product.EskdNumber.ClassNumber?.Number.ToString("D6");
                DetailNumber = Product.EskdNumber.DetailNumber;
                Version = Product.EskdNumber.Version;
                ProductName = Product.Name;
                ProductMaterial = Product.Material;

                var parentAssemblies = await _documentDataService.GetProductParentAssembliesAsync(Product.Id);
                foreach(var item in parentAssemblies)
                {
                    ParentAssemblies.Add(item);
                }

                //var parentProducts = await _documentDataService.GetProductParentProductsAsync(Product.Id);
                //foreach(var item in parentProducts)
                //{
                //    ParentProducts.Add(item);
                //}
            }
            else
            {
                CompanyCode = _settingsService.LoadSettings().DefaultCompanyCode;
                Product.EskdNumber.CompanyCode = CompanyCode;
            }
        }
    }
}

