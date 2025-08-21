#nullable enable

using DetailViewer.Core.Events;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ILogger = DetailViewer.Core.Interfaces.ILogger;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Modules.Explorer.ViewModels
{
    public class ProductsDashboardViewModel : BindableBase
    {
        private readonly IProductService _productService;
        private readonly IDialogService _dialogService;
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;

        private string _statusText = string.Empty;
        public string StatusText
        {
            get { return _statusText; }
            set { SetProperty(ref _statusText, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get { return _products; }
            set { SetProperty(ref _products, value); }
        }

        private Product? _selectedProduct;
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        private List<Product> _allProducts = new List<Product>();

        private string _eskdNumberFilter = string.Empty;
        public string EskdNumberFilter
        {
            get { return _eskdNumberFilter; }
            set { SetProperty(ref _eskdNumberFilter, value, ApplyFilters); }
        }

        private string _nameFilter = string.Empty;
        public string NameFilter
        {
            get { return _nameFilter; }
            set { SetProperty(ref _nameFilter, value, ApplyFilters); }
        }

        public DelegateCommand AddProductCommand { get; private set; }
        public DelegateCommand EditProductCommand { get; private set; }
        public DelegateCommand DeleteProductCommand { get; private set; }

        public ProductsDashboardViewModel(IProductService productService, IDialogService dialogService, ILogger logger, IEventAggregator eventAggregator)
        {
            _productService = productService;
            _dialogService = dialogService;
            _logger = logger;
            _eventAggregator = eventAggregator;

            _products = new ObservableCollection<Product>();
            StatusText = "Готово";

            AddProductCommand = new DelegateCommand(AddProduct);
            EditProductCommand = new DelegateCommand(EditProduct, () => SelectedProduct != null).ObservesProperty(() => SelectedProduct);
            DeleteProductCommand = new DelegateCommand(DeleteProduct, () => SelectedProduct != null).ObservesProperty(() => SelectedProduct);

            _eventAggregator.GetEvent<SyncCompletedEvent>().Subscribe(OnSyncCompleted, ThreadOption.UIThread);

            _ = LoadData();
        }

        private async void OnSyncCompleted()
        {
            await LoadData();
        }

        private void EditProduct()
        {
            _logger.Log("Editing product");
            var parameters = new DialogParameters { { "product", SelectedProduct } };
            _dialogService.ShowDialog("ProductForm", parameters, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await LoadData();
                }
            });
        }

        private void DeleteProduct()
{
    if (SelectedProduct == null)
    {
        return;
    }

    _logger.Log("Deleting product");
    _dialogService.ShowDialog("ConfirmationDialog", new DialogParameters { { "message", $"Вы уверены, что хотите удалить запись: {SelectedProduct.EskdNumber.FullCode}?" } }, async r =>
    {
        if (r.Result == ButtonResult.OK)
        {
            await _productService.DeleteProductAsync(SelectedProduct.Id);
            await LoadData();
        }
    });
}

        private void AddProduct()
        {
            _logger.Log("Adding product");
            _dialogService.ShowDialog("ProductForm", null, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await LoadData();
                }
            });
        }

        private async Task LoadData()
        {
            _logger.Log("Loading data for ProductsDashboard");
            IsBusy = true;
            StatusText = "Загрузка данных...";
            try
            {
                _allProducts = await _productService.GetProductsAsync();
                ApplyFilters();
                StatusText = $"Данные успешно загружены.";
                _logger.LogInfo("Products loaded successfully.");
            }
            catch (Exception ex)
            {
                StatusText = "Ошибка при загрузке данных.";
                _logger.LogError($"Error loading products: {ex.Message}", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilters()
        {
            _logger.Log("Applying filters to products");
            if (_allProducts == null) return;

            var filteredProducts = _allProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(EskdNumberFilter))
            {
                filteredProducts = filteredProducts.Where(p => p.EskdNumber != null && p.EskdNumber.FullCode != null && p.EskdNumber.FullCode.Contains(EskdNumberFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(NameFilter))
            {
                filteredProducts = filteredProducts.Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(NameFilter, StringComparison.OrdinalIgnoreCase));
            }

            Products.Clear();
            foreach (var product in filteredProducts)
            {
                Products.Add(product);
            }

            StatusText = $"Отобрано записей: {Products.Count}";
        }
    }
}