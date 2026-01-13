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
using Google.Apis.Sheets.v4.Data;

namespace DetailViewer.Modules.Explorer.ViewModels
{
    /// <summary>
    /// ViewModel для панели управления продуктами.
    /// </summary>
    public class ProductsDashboardViewModel : BindableBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IProductService _productService;
        private readonly IDialogService _dialogService;
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;

        private string _statusText = string.Empty;
        /// <summary>
        /// Текст статуса, отображаемый на панели.
        /// </summary>
        public string StatusText
        {
            get { return _statusText; }
            set { SetProperty(ref _statusText, value); }
        }

        private bool _isBusy;
        /// <summary>
        /// Флаг, указывающий, занято ли приложение выполнением операции.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        private ObservableCollection<Product> _products;
        /// <summary>
        /// Коллекция продуктов, отображаемых на панели.
        /// </summary>
        public ObservableCollection<Product> Products
        {
            get { return _products; }
            set { SetProperty(ref _products, value); }
        }

        private Product? _selectedProduct;
        /// <summary>
        /// Выбранный продукт.
        /// </summary>
        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }

        private List<Product> _allProducts = new List<Product>();

        private string _eskdNumberFilter = string.Empty;
        /// <summary>
        /// Фильтр по децимальному номеру.
        /// </summary>
        public string EskdNumberFilter
        {
            get { return _eskdNumberFilter; }
            set { SetProperty(ref _eskdNumberFilter, value, ApplyFilters); }
        }

        private string _nameFilter = string.Empty;
        /// <summary>
        /// Фильтр по наименованию.
        /// </summary>
        public string NameFilter
        {
            get { return _nameFilter; }
            set { SetProperty(ref _nameFilter, value, ApplyFilters); }
        }

        /// <summary>
        /// Команда для добавления нового продукта.
        /// </summary>
        public DelegateCommand AddProductCommand { get; private set; }

        /// <summary>
        /// Команда для редактирования выбранного продукта.
        /// </summary>
        public DelegateCommand EditProductCommand { get; private set; }

        /// <summary>
        /// Команда для удаления выбранного продукта.
        /// </summary>
        public DelegateCommand DeleteProductCommand { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProductsDashboardViewModel"/>.
        /// </summary>
        public ProductsDashboardViewModel(IProductService productService, ISettingsService settingsService, IDialogService dialogService, ILogger logger, IEventAggregator eventAggregator)
        {
            _settingsService = settingsService;
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

        /// <summary>
        /// Вызывается при завершении синхронизации данных.
        /// </summary>
        private async void OnSyncCompleted()
        {
            await LoadData();
        }

        /// <summary>
        /// Открывает диалог редактирования продукта.
        /// </summary>
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

        /// <summary>
        /// Удаляет выбранный продукт.
        /// </summary>
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

        /// <summary>
        /// Открывает диалог добавления нового продукта.
        /// </summary>
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

        /// <summary>
        /// Асинхронно загружает данные продуктов.
        /// </summary>
        private async Task LoadData()
        {
            _logger.Log("Loading data for ProductsDashboard");
            IsBusy = true;
            StatusText = "Загрузка данных...";
            try
            {
                var records = await _productService.GetProductsAsync();
                _allProducts = records.Where(r => r.EskdNumber.CompanyCode == _settingsService.LoadSettings().DefaultCompanyCode).ToList();
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

        /// <summary>
        /// Применяет фильтры к списку продуктов.
        /// </summary>
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
