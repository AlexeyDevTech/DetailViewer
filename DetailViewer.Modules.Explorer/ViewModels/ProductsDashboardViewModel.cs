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

namespace DetailViewer.Modules.Explorer.ViewModels
{
    public class ProductsDashboardViewModel : BindableBase
    {
        private readonly IDocumentDataService _documentDataService;
        private readonly IDialogService _dialogService;
        private readonly ILogger _logger;

        private string _statusText;
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

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        private List<Product> _allProducts;

        private string _eskdNumberFilter;
        public string EskdNumberFilter
        {
            get { return _eskdNumberFilter; }
            set { SetProperty(ref _eskdNumberFilter, value, ApplyFilters); }
        }

        private string _nameFilter;
        public string NameFilter
        {
            get { return _nameFilter; }
            set { SetProperty(ref _nameFilter, value, ApplyFilters); }
        }

        public DelegateCommand AddProductCommand { get; private set; }
        public DelegateCommand EditProductCommand { get; private set; }
        public DelegateCommand DeleteProductCommand { get; private set; }

        public ProductsDashboardViewModel(IDocumentDataService documentDataService, IDialogService dialogService, ILogger logger)
        {
            _documentDataService = documentDataService;
            _dialogService = dialogService;
            _logger = logger;

            Products = new ObservableCollection<Product>();
            StatusText = "Готово";

            AddProductCommand = new DelegateCommand(AddProduct);
            EditProductCommand = new DelegateCommand(EditProduct, () => SelectedProduct != null).ObservesProperty(() => SelectedProduct);
            DeleteProductCommand = new DelegateCommand(DeleteProduct, () => SelectedProduct != null).ObservesProperty(() => SelectedProduct);

            LoadData();
        }

        private void EditProduct()
        {
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
            _dialogService.ShowDialog("ConfirmationDialog", new DialogParameters { { "message", $"Вы уверены, что хотите удалить запись: {SelectedProduct.EskdNumber.FullCode}?" } }, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await _documentDataService.DeleteProductAsync(SelectedProduct.Id);
                    await LoadData();
                }
            });
        }

        private void AddProduct()
        {
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
            IsBusy = true;
            StatusText = "Загрузка данных...";
            try
            {
                _allProducts = await _documentDataService.GetProductsAsync();
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
