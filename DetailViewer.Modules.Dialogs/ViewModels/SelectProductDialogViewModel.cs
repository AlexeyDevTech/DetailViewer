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
    /// <summary>
    /// ViewModel для диалогового окна выбора продуктов.
    /// </summary>
    public class SelectProductDialogViewModel : BindableBase, IDialogAware
    {
        private readonly IProductService _productService;

        /// <summary>
        /// Заголовок диалогового окна.
        /// </summary>
        public string Title => "Выбор изделий";

        /// <summary>
        /// Событие, запрашивающее закрытие диалогового окна.
        /// </summary>
        public event System.Action<IDialogResult>? RequestClose;

        private string? _searchText;
        /// <summary>
        /// Текст для поиска/фильтрации продуктов.
        /// </summary>
        public string? SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value, OnSearchTextChanged); }
        }

        private ObservableCollection<SelectableItem<Product>>? _allProducts;
        private ObservableCollection<SelectableItem<Product>>? _filteredProducts;
        /// <summary>
        /// Отфильтрованный список продуктов для отображения.
        /// </summary>
        public ObservableCollection<SelectableItem<Product>>? FilteredProducts
        {
            get { return _filteredProducts; }
            set { SetProperty(ref _filteredProducts, value); }
        }

        /// <summary>
        /// Команда для подтверждения выбора.
        /// </summary>
        public DelegateCommand OkCommand { get; private set; }

        /// <summary>
        /// Команда для отмены выбора.
        /// </summary>
        public DelegateCommand CancelCommand { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SelectProductDialogViewModel"/>.
        /// </summary>
        /// <param name="productService">Сервис для работы с продуктами.</param>
        public SelectProductDialogViewModel(IProductService productService)
        {
            _productService = productService;
            OkCommand = new DelegateCommand(Ok);
            CancelCommand = new DelegateCommand(Cancel);
            LoadProducts();
        }

        /// <summary>
        /// Асинхронно загружает все продукты и инициализирует список для выбора.
        /// </summary>
        private async void LoadProducts()
        {
            var products = await _productService.GetProductsAsync();
            _allProducts = new ObservableCollection<SelectableItem<Product>>(products.Select(p => new SelectableItem<Product>(p)));
            _filteredProducts = new ObservableCollection<SelectableItem<Product>>(_allProducts);
        }

        /// <summary>
        /// Вызывается при изменении текста поиска для фильтрации продуктов.
        /// </summary>
        private void OnSearchTextChanged()
        {
            if (_allProducts == null)
            {
                return;
            }

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

        /// <summary>
        /// Обрабатывает команду OK, возвращая выбранные продукты.
        /// </summary>
        private void Ok()
        {
            var result = new DialogResult(ButtonResult.OK);
            if (FilteredProducts != null)
            {
                result.Parameters.Add(DialogParameterKeys.SelectedProducts, FilteredProducts.Where(p => p.IsSelected).Select(p => p.Item).ToList());
            }
            RequestClose?.Invoke(result);
        }

        /// <summary>
        /// Обрабатывает команду Cancel.
        /// </summary>
        private void Cancel()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        /// <summary>
        /// Определяет, можно ли закрыть диалоговое окно.
        /// </summary>
        /// <returns>Всегда true.</returns>
        public bool CanCloseDialog() => true;

        /// <summary>
        /// Вызывается после закрытия диалогового окна.
        /// </summary>
        public void OnDialogClosed() { }

        /// <summary>
        /// Вызывается при открытии диалогового окна.
        /// </summary>
        /// <param name="parameters">Параметры диалогового окна.</param>
        public void OnDialogOpened(IDialogParameters parameters) { }
    }
}
