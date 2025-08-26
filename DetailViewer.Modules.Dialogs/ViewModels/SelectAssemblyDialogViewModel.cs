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
    /// Вспомогательный класс для представления элемента, который может быть выбран в списке.
    /// </summary>
    /// <typeparam name="T">Тип элемента.</typeparam>
    public class SelectableItem<T> : BindableBase
    {
        private bool _isSelected;
        /// <summary>
        /// Получает или задает значение, указывающее, выбран ли элемент.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        /// <summary>
        /// Получает сам элемент.
        /// </summary>
        public T Item { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SelectableItem{T}"/>.
        /// </summary>
        /// <param name="item">Элемент для обертывания.</param>
        public SelectableItem(T item)
        {
            Item = item;
        }
    }

    /// <summary>
    /// ViewModel для диалогового окна выбора сборок.
    /// </summary>
    public class SelectAssemblyDialogViewModel : BindableBase, IDialogAware
    {
        private readonly IAssemblyService _assemblyService;
        private readonly IProductService _productService;

        /// <summary>
        /// Заголовок диалогового окна.
        /// </summary>
        public string Title => "Выбор сборок";

        /// <summary>
        /// Событие, запрашивающее закрытие диалогового окна.
        /// </summary>
        public event System.Action<IDialogResult>? RequestClose;

        private string? _searchText;
        /// <summary>
        /// Текст для поиска/фильтрации сборок.
        /// </summary>
        public string? SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value, OnSearchTextChanged); }
        }

        private ObservableCollection<SelectableItem<Assembly>>? _allAssemblies;
        private ObservableCollection<SelectableItem<Assembly>>? _filteredAssemblies;
        /// <summary>
        /// Отфильтрованный список сборок для отображения.
        /// </summary>
        public ObservableCollection<SelectableItem<Assembly>>? FilteredAssemblies
        {
            get { return _filteredAssemblies; }
            set { SetProperty(ref _filteredAssemblies, value); }
        }

        private SelectableItem<Assembly>? _selectedAssembly;
        /// <summary>
        /// Выбранная сборка в списке.
        /// </summary>
        public SelectableItem<Assembly>? SelectedAssembly
        {
            get { return _selectedAssembly; }
            set { SetProperty(ref _selectedAssembly, value, OnSelectedAssemblyChanged); }
        }

        private ObservableCollection<Product> _relatedProducts;
        /// <summary>
        /// Продукты, связанные с выбранной сборкой.
        /// </summary>
        public ObservableCollection<Product> RelatedProducts
        {
            get { return _relatedProducts; }
            set { SetProperty(ref _relatedProducts, value); }
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
        /// Инициализирует новый экземпляр класса <see cref="SelectAssemblyDialogViewModel"/>.
        /// </summary>
        /// <param name="assemblyService">Сервис для работы со сборками.</param>
        /// <param name="productService">Сервис для работы с продуктами.</param>
        public SelectAssemblyDialogViewModel(IAssemblyService assemblyService, IProductService productService)
        {
            _assemblyService = assemblyService;
            _productService = productService;
            OkCommand = new DelegateCommand(Ok);
            CancelCommand = new DelegateCommand(Cancel);
            _relatedProducts = new ObservableCollection<Product>();
            LoadAssemblies();
        }

        /// <summary>
        /// Асинхронно загружает все сборки и инициализирует список для выбора.
        /// </summary>
        private async void LoadAssemblies()
        {
            var assemblies = await _assemblyService.GetAssembliesAsync();
            _allAssemblies = new ObservableCollection<SelectableItem<Assembly>>(assemblies.Select(a => new SelectableItem<Assembly>(a)));
            FilteredAssemblies = new ObservableCollection<SelectableItem<Assembly>>(_allAssemblies);
        }

        /// <summary>
        /// Вызывается при изменении текста поиска для фильтрации сборок.
        /// </summary>
        private void OnSearchTextChanged()
        {
            if (_allAssemblies == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredAssemblies = new ObservableCollection<SelectableItem<Assembly>>(_allAssemblies);
            }
            else
            {
                var searchTextLower = SearchText.ToLower();
                FilteredAssemblies = new ObservableCollection<SelectableItem<Assembly>>(_allAssemblies.Where(a => a.Item.Name.ToLower().Contains(searchTextLower) || a.Item.EskdNumber.FullCode.ToLower().Contains(searchTextLower)));
            }
        }

        /// <summary>
        /// Вызывается при изменении выбранной сборки для загрузки связанных продуктов.
        /// </summary>
        private async void OnSelectedAssemblyChanged()
        {
            RelatedProducts.Clear();
            if (SelectedAssembly != null)
            {
                var products = await _productService.GetProductsByAssemblyId(SelectedAssembly.Item.Id);
                foreach (var product in products)
                {
                    RelatedProducts.Add(product);
                }
            }
        }

        /// <summary>
        /// Обрабатывает команду OK, возвращая выбранные сборки.
        /// </summary>
        private void Ok()
        {
            var result = new DialogResult(ButtonResult.OK);
            if (FilteredAssemblies != null)
            {
                result.Parameters.Add(DialogParameterKeys.SelectedAssemblies, FilteredAssemblies.Where(a => a.IsSelected).Select(a => a.Item).ToList());
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
