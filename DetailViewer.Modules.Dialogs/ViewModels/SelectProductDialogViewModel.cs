#nullable enable

using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class SelectProductDialogViewModel : BindableBase, IDialogAware
    {
        private readonly IProductService _productService;

        public string Title => "Выбор изделий";
        public event System.Action<IDialogResult>? RequestClose;

        private string? _searchText;
        public string? SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value, OnSearchTextChanged); }
        }

        private ObservableCollection<SelectableItem<Product>>? _allProducts;
        private ObservableCollection<SelectableItem<Product>>? _filteredProducts;
        public ObservableCollection<SelectableItem<Product>>? FilteredProducts
        {
            get { return _filteredProducts; }
            set { SetProperty(ref _filteredProducts, value); }
        }

        public DelegateCommand OkCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public SelectProductDialogViewModel(IProductService productService)
        {
            _productService = productService;
            OkCommand = new DelegateCommand(Ok);
            CancelCommand = new DelegateCommand(Cancel);
            LoadProducts();
        }

        private async void LoadProducts()
        {
            var products = await _productService.GetProductsAsync();
            _allProducts = new ObservableCollection<SelectableItem<Product>>(products.Select(p => new SelectableItem<Product>(p)));
            _filteredProducts = new ObservableCollection<SelectableItem<Product>>(_allProducts);
        }

        private void OnSearchTextChanged()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredProducts = new ObservableCollection<SelectableItem<Product>>(_allProducts);
            }
            else
            {
                var searchTextLower = SearchText.ToLower();
                FilteredProducts = new ObservableCollection<SelectableItem<Product>>(_allProducts.Where(p => p.Item.Name.ToLower().Contains(searchTextLower) || p.Item.EskdNumber.FullCode.ToLower().Contains(searchTextLower)));
            }
        }

        private void Ok()
        {
            var result = new DialogResult(ButtonResult.OK);
            result.Parameters.Add(DialogParameterKeys.SelectedProducts, FilteredProducts.Where(p => p.IsSelected).Select(p => p.Item).ToList());
            RequestClose?.Invoke(result);
        }

        private void Cancel()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) { }
    }
}