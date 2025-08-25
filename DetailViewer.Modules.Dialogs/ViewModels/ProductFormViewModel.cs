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
    public class ProductFormViewModel : BindableBase, IDialogAware
    {
        private readonly IProductService _productService;
        private readonly IAssemblyService _assemblyService;
        private readonly IClassifierService _classifierService;
        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private readonly IActiveUserService _activeUserService;
        private readonly IDialogService _dialogService;

        public string Title => "Форма изделия";
        public event Action<IDialogResult>? RequestClose;

        private Product _product;
        public Product Product { get => _product; set => SetProperty(ref _product, value); }

        private ObservableCollection<Assembly> _parentAssemblies;
        public ObservableCollection<Assembly> ParentAssemblies { get => _parentAssemblies; set => SetProperty(ref _parentAssemblies, value); }

        private ObservableCollection<Product> _parentProducts;
        public ObservableCollection<Product> ParentProducts { get => _parentProducts; set => SetProperty(ref _parentProducts, value); }

        private string? _companyCode, _classNumberString, _productName, _productMaterial;
        private int _detailNumber;
        private int? _version;

        public string? CompanyCode { get => _companyCode; set => SetProperty(ref _companyCode, value, OnESKDNumberPartChanged); }
        public string? ClassNumberString { get => _classNumberString; set => SetProperty(ref _classNumberString, value, OnClassNumberStringChanged); }
        public int DetailNumber { get => _detailNumber; set => SetProperty(ref _detailNumber, value, OnESKDNumberPartChanged); }
        public int? Version { get => _version; set => SetProperty(ref _version, value, OnESKDNumberPartChanged); }
        public string? ProductName { get => _productName; set => SetProperty(ref _productName, value); }
        public string? ProductMaterial { get => _productMaterial; set => SetProperty(ref _productMaterial, value); }

        public string ESKDNumberString
        {
            get
            {
                if (string.IsNullOrEmpty(CompanyCode) || string.IsNullOrEmpty(ClassNumberString) || DetailNumber == 0) return string.Empty;
                string baseCode = $"{CompanyCode}.{ClassNumberString}.{DetailNumber:D3}";
                return Version.HasValue ? $"{baseCode}-{Version.Value:D2}" : baseCode;
            }
        }

        private ObservableCollection<Classifier>? _allClassifiers;
        public ObservableCollection<Classifier>? AllClassifiers { get => _allClassifiers; set => SetProperty(ref _allClassifiers, value); }

        private ObservableCollection<Classifier>? _filteredClassifiers;
        public ObservableCollection<Classifier>? FilteredClassifiers { get => _filteredClassifiers; set => SetProperty(ref _filteredClassifiers, value); }

        private bool _isUpdatingFromSelection = false;
        private Classifier? _selectedClassifier;
        public Classifier? SelectedClassifier
        {
            get => _selectedClassifier;
            set
            {
                SetProperty(ref _selectedClassifier, value);
                if (value != null) { _isUpdatingFromSelection = true; ClassNumberString = value.Number.ToString("D6"); _isUpdatingFromSelection = false; }
            }
        }

        private List<Product>? _allProducts;
        private ObservableCollection<Product>? _filteredProducts;
        public ObservableCollection<Product>? FilteredProducts { get => _filteredProducts; set => SetProperty(ref _filteredProducts, value); }

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand AddParentAssemblyCommand { get; private set; }
        public DelegateCommand<Assembly> RemoveParentAssemblyCommand { get; private set; }
        public DelegateCommand AddParentProductCommand { get; private set; }
        public DelegateCommand<Product> RemoveParentProductCommand { get; private set; }

        public ProductFormViewModel(IProductService productService, IAssemblyService assemblyService, IClassifierService classifierService, ILogger logger, ISettingsService settingsService, IActiveUserService activeUserService, IDialogService dialogService)
        {
            _productService = productService;
            _assemblyService = assemblyService;
            _classifierService = classifierService;
            _logger = logger;
            _settingsService = settingsService;
            _activeUserService = activeUserService;
            _dialogService = dialogService;

            _product = new Product { EskdNumber = new ESKDNumber(), Author = _activeUserService.CurrentUser?.ShortName };
            _parentAssemblies = new ObservableCollection<Assembly>();
            _parentProducts = new ObservableCollection<Product>();

            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
            AddParentAssemblyCommand = new DelegateCommand(AddParentAssembly);
            RemoveParentAssemblyCommand = new DelegateCommand<Assembly>(RemoveParentAssembly);
            AddParentProductCommand = new DelegateCommand(AddParentProduct);
            RemoveParentProductCommand = new DelegateCommand<Product>(RemoveParentProduct);

            LoadProducts();
        }

        private void LoadClassifiers() => AllClassifiers = new ObservableCollection<Classifier>(_classifierService.GetAllClassifiers());

        private async void LoadProducts() => _allProducts = await _productService.GetProductsAsync();

        private void FilterClassifiers()
        {
            if (AllClassifiers == null) { FilteredClassifiers = new ObservableCollection<Classifier>(); return; }
            if (string.IsNullOrWhiteSpace(ClassNumberString)) { FilteredClassifiers = new ObservableCollection<Classifier>(AllClassifiers); return; }
            FilteredClassifiers = new ObservableCollection<Classifier>(AllClassifiers.Where(c => c.Number.ToString("D6").StartsWith(ClassNumberString, StringComparison.OrdinalIgnoreCase)).OrderBy(c => c.Number).ToList());
        }

        private void FilterProducts()
        {
            if (_allProducts == null || ClassNumberString?.Length != 6) { FilteredProducts = new ObservableCollection<Product>(); return; }
            var records = _allProducts.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(ClassNumberString)) records = records.Where(r => r.EskdNumber?.ClassNumber?.Number.ToString("D6").StartsWith(ClassNumberString) == true);
            FilteredProducts = new ObservableCollection<Product>(records.OrderBy(r => r.EskdNumber.FullCode).ToList());
        }

        private void OnESKDNumberPartChanged() => RaisePropertyChanged(nameof(ESKDNumberString));

        private void OnClassNumberStringChanged()
        {
            if (_isUpdatingFromSelection) return;
            FilterClassifiers();
            FilterProducts();
            OnESKDNumberPartChanged();
        }

        private void AddParentAssembly()
        {
            _dialogService.ShowDialog("SelectAssemblyDialog", new DialogParameters(), r =>
            {
                if (r.Result == ButtonResult.OK) { var selectedAssemblies = r.Parameters.GetValue<List<Assembly>>(DialogParameterKeys.SelectedAssemblies); if (selectedAssemblies != null) foreach (var assembly in selectedAssemblies) if (!ParentAssemblies.Any(p => p.Id == assembly.Id)) ParentAssemblies.Add(assembly); }
            });
        }

        private void RemoveParentAssembly(Assembly assembly) { if (assembly != null) ParentAssemblies.Remove(assembly); }

        private void AddParentProduct()
        {
            _dialogService.ShowDialog("SelectProductDialog", new DialogParameters(), r =>
            {
                if (r.Result == ButtonResult.OK) { var selectedProducts = r.Parameters.GetValue<List<Product>>(DialogParameterKeys.SelectedProducts); if (selectedProducts != null) foreach (var product in selectedProducts) if (!ParentProducts.Any(p => p.Id == product.Id)) ParentProducts.Add(product); }
            });
        }

        private void RemoveParentProduct(Product product) { if (product != null) ParentProducts.Remove(product); }

        private async void Save()
        {
            Product.EskdNumber.CompanyCode = CompanyCode;
            Product.EskdNumber.DetailNumber = DetailNumber;
            Product.EskdNumber.Version = Version;
            if (int.TryParse(ClassNumberString, out int classNumberValue))
            {
                var classifier = _classifierService.GetClassifierByNumber(classNumberValue);
                if (classifier != null) { Product.EskdNumber.ClassifierId = classifier.Id; Product.EskdNumber.ClassNumber = null; }
            }

            try
            {
                if (ParentProducts.Any())
                {
                    if (Product.Id == 0) await _productService.AddProductAsync(Product);
                    await _assemblyService.ConvertProductToAssemblyAsync(Product.Id, ParentProducts.ToList());
                }
                else
                {
                    if (Product.Id == 0) await _productService.CreateProductWithAssembliesAsync(Product, ParentAssemblies.Select(a => a.Id).ToList());
                    else { await _productService.UpdateProductAsync(Product); await _productService.UpdateProductParentAssembliesAsync(Product.Id, ParentAssemblies.ToList()); }
                }
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при сохранении продукта: {ex.Message}", ex);
                _dialogService.ShowDialog("ErrorDialog", new DialogParameters($"Message=Произошла ошибка: {ex.Message}"), null);
            }
        }

        private void Cancel() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public async void OnDialogOpened(IDialogParameters parameters)
        {
            await _classifierService.LoadClassifiersAsync();
            LoadClassifiers();
            if (parameters.ContainsKey(DialogParameterKeys.Product))
            {
                Product = parameters.GetValue<Product>(DialogParameterKeys.Product);
                CompanyCode = Product.EskdNumber.CompanyCode;
                ClassNumberString = Product.EskdNumber.ClassNumber?.Number.ToString("D6");
                DetailNumber = Product.EskdNumber.DetailNumber;
                Version = Product.EskdNumber.Version;
                ProductName = Product.Name;
                ProductMaterial = Product.Material;
                var parentAssemblies = await _productService.GetProductParentAssembliesAsync(Product.Id);
                foreach (var item in parentAssemblies) ParentAssemblies.Add(item);
            }
            else { CompanyCode = _settingsService.LoadSettings().DefaultCompanyCode; Product.EskdNumber.CompanyCode = CompanyCode; }
        }
    }
}