using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DetailViewer.Modules.Explorer.ViewModels
{
    public class ProductsDashboardViewModel : BindableBase
    {
        private readonly IDocumentDataService _documentDataService;
        private readonly IDialogService _dialogService;

        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get { return _products; }
            set { SetProperty(ref _products, value); }
        }

        public DelegateCommand AddProductCommand { get; private set; }
        public DelegateCommand EditProductCommand { get; private set; }
        public DelegateCommand DeleteProductCommand { get; private set; }

        public ProductsDashboardViewModel(IDocumentDataService documentDataService, IDialogService dialogService)
        {
            _documentDataService = documentDataService;
            _dialogService = dialogService;

            AddProductCommand = new DelegateCommand(AddProduct);
            EditProductCommand = new DelegateCommand(EditProduct, () => SelectedProduct != null).ObservesProperty(() => SelectedProduct);
            DeleteProductCommand = new DelegateCommand(DeleteProduct, () => SelectedProduct != null).ObservesProperty(() => SelectedProduct);

            LoadProducts();
        }

        private async Task LoadProducts()
        {
            var products = await _documentDataService.GetProductsAsync();
            Products = new ObservableCollection<Product>(products);
        }

        private void AddProduct()
        {
            _dialogService.ShowDialog("ProductForm", null, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await LoadProducts();
                }
            });
        }

        private void EditProduct()
        {
            var parameters = new DialogParameters { { "product", SelectedProduct } };
            _dialogService.ShowDialog("ProductForm", parameters, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await LoadProducts();
                }
            });
        }

        private async void DeleteProduct()
        {
            // Confirmation dialog can be added here
            await _documentDataService.DeleteProductAsync(SelectedProduct.Id);
            await LoadProducts();
        }

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get { return _selectedProduct; }
            set { SetProperty(ref _selectedProduct, value); }
        }
    }
}
