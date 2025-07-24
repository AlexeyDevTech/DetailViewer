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
    public class SelectableItem<T> : BindableBase
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public T Item { get; private set; }

        public SelectableItem(T item)
        {
            Item = item;
        }
    }

    public class SelectAssemblyDialogViewModel : BindableBase, IDialogAware
    {
        private readonly IDocumentDataService _documentDataService;

        public string Title => "Выбор сборок";
        public event System.Action<IDialogResult> RequestClose;

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value, OnSearchTextChanged); }
        }

        private ObservableCollection<SelectableItem<Assembly>> _allAssemblies;
        private ObservableCollection<SelectableItem<Assembly>> _filteredAssemblies;
        public ObservableCollection<SelectableItem<Assembly>> FilteredAssemblies
        {
            get { return _filteredAssemblies; }
            set { SetProperty(ref _filteredAssemblies, value); }
        }

        private SelectableItem<Assembly> _selectedAssembly;
        public SelectableItem<Assembly> SelectedAssembly
        {
            get { return _selectedAssembly; }
            set { SetProperty(ref _selectedAssembly, value, OnSelectedAssemblyChanged); }
        }

        private ObservableCollection<Product> _relatedProducts;
        public ObservableCollection<Product> RelatedProducts
        {
            get { return _relatedProducts; }
            set { SetProperty(ref _relatedProducts, value); }
        }

        public DelegateCommand OkCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public SelectAssemblyDialogViewModel(IDocumentDataService documentDataService)
        {
            _documentDataService = documentDataService;
            OkCommand = new DelegateCommand(Ok);
            CancelCommand = new DelegateCommand(Cancel);
            RelatedProducts = new ObservableCollection<Product>();
            LoadAssemblies();
        }

        private async void LoadAssemblies()
        {
            var assemblies = await _documentDataService.GetAssembliesAsync();
            _allAssemblies = new ObservableCollection<SelectableItem<Assembly>>(assemblies.Select(a => new SelectableItem<Assembly>(a)));
            FilteredAssemblies = new ObservableCollection<SelectableItem<Assembly>>(_allAssemblies);
        }

        private void OnSearchTextChanged()
        {
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

        private async void OnSelectedAssemblyChanged()
        {
            RelatedProducts.Clear();
            if (SelectedAssembly != null)
            {
                var products = await _documentDataService.GetProductsByAssemblyId(SelectedAssembly.Item.Id);
                foreach (var product in products)
                {
                    RelatedProducts.Add(product);
                }
            }
        }

        private void Ok()
        {
            var result = new DialogResult(ButtonResult.OK);
            result.Parameters.Add(DialogParameterKeys.SelectedAssemblies, FilteredAssemblies.Where(a => a.IsSelected).Select(a => a.Item).ToList());
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
