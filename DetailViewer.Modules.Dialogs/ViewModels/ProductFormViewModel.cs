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
    /// <summary>
    /// ViewModel для формы создания/редактирования продукта.
    /// </summary>
    public class ProductFormViewModel : BindableBase, IDialogAware
    {
        private readonly IProductService _productService;
        private readonly IAssemblyService _assemblyService;
        private readonly IClassifierService _classifierService;
        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private readonly IActiveUserService _activeUserService;
        private readonly IDialogService _dialogService;

        /// <summary>
        /// Заголовок диалогового окна.
        /// </summary>
        public string Title => "Форма изделия";

        /// <summary>
        /// Событие, запрашивающее закрытие диалогового окна.
        /// </summary>
        public event Action<IDialogResult>? RequestClose;

        private Product _product;
        /// <summary>
        /// Редактируемый или создаваемый продукт.
        /// </summary>
        public Product Product { get => _product; set => SetProperty(ref _product, value); }

        private ObservableCollection<Assembly> _parentAssemblies;
        /// <summary>
        /// Коллекция родительских сборок для текущего продукта.
        /// </summary>
        public ObservableCollection<Assembly> ParentAssemblies { get => _parentAssemblies; set => SetProperty(ref _parentAssemblies, value); }

        private ObservableCollection<Product> _parentProducts;
        /// <summary>
        /// Коллекция родительских продуктов для текущего продукта.
        /// </summary>
        public ObservableCollection<Product> ParentProducts { get => _parentProducts; set => SetProperty(ref _parentProducts, value); }

        private string? _companyCode, _classNumberString, _productName, _productMaterial;
        private int _detailNumber;
        private int? _version;

        /// <summary>
        /// Код компании для децимального номера продукта.
        /// </summary>
        public string? CompanyCode { get => _companyCode; set => SetProperty(ref _companyCode, value, OnESKDNumberPartChanged); }

        /// <summary>
        /// Строковое представление номера класса для децимального номера продукта.
        /// </summary>
        public string? ClassNumberString { get => _classNumberString; set => SetProperty(ref _classNumberString, value, OnClassNumberStringChanged); }

        /// <summary>
        /// Порядковый номер детали для децимального номера продукта.
        /// </summary>
        public int DetailNumber { get => _detailNumber; set => SetProperty(ref _detailNumber, value, OnESKDNumberPartChanged); }

        /// <summary>
        /// Номер версии для децимального номера продукта.
        /// </summary>
        public int? Version { get => _version; set => SetProperty(ref _version, value, OnESKDNumberPartChanged); }

        /// <summary>
        /// Наименование продукта.
        /// </summary>
        public string? ProductName { get => _productName; set => SetProperty(ref _productName, value); }

        /// <summary>
        /// Материал продукта.
        /// </summary>
        public string? ProductMaterial { get => _productMaterial; set => SetProperty(ref _productMaterial, value); }

        /// <summary>
        /// Полное строковое представление децимального номера продукта.
        /// </summary>
        public string ESKDNumberString
        {
            get
            {
                if (string.IsNullOrEmpty(CompanyCode) || string.IsNullOrEmpty(ClassNumberString) || DetailNumber == 0) return string.Empty;
                string baseCode = $"{CompanyCode}.{ClassNumberString}.{DetailNumber:D3}";
                return Version.HasValue ? $"{baseCode}-{Version.Value:D2}" : baseCode;
            }
        }

        private ObservableCollection<ClassifierData>? _allClassifiers;
        /// <summary>
        /// Все доступные классификаторы.
        /// </summary>
        public ObservableCollection<ClassifierData>? AllClassifiers { get => _allClassifiers; set => SetProperty(ref _allClassifiers, value); }

        private ObservableCollection<ClassifierData>? _filteredClassifiers;
        /// <summary>
        /// Отфильтрованные классификаторы.
        /// </summary>
        public ObservableCollection<ClassifierData>? FilteredClassifiers { get => _filteredClassifiers; set => SetProperty(ref _filteredClassifiers, value); }

        private bool _isUpdatingFromSelection = false;
        private ClassifierData? _selectedClassifier;
        /// <summary>
        /// Выбранный классификатор.
        /// </summary>
        public ClassifierData? SelectedClassifier
        {
            get => _selectedClassifier;
            set
            {
                SetProperty(ref _selectedClassifier, value);
                if (value != null) { _isUpdatingFromSelection = true; ClassNumberString = value.Code; _isUpdatingFromSelection = false; }
            }
        }

        private List<Product>? _allProducts;
        private ObservableCollection<Product>? _filteredProducts;
        /// <summary>
        /// Отфильтрованные продукты.
        /// </summary>
        public ObservableCollection<Product>? FilteredProducts { get => _filteredProducts; set => SetProperty(ref _filteredProducts, value); }

        /// <summary>
        /// Команда для сохранения продукта.
        /// </summary>
        public DelegateCommand SaveCommand { get; private set; }

        /// <summary>
        /// Команда для отмены изменений.
        /// </summary>
        public DelegateCommand CancelCommand { get; private set; }

        /// <summary>
        /// Команда для добавления родительской сборки.
        /// </summary>
        public DelegateCommand AddParentAssemblyCommand { get; private set; }

        /// <summary>
        /// Команда для удаления родительской сборки.
        /// </summary>
        public DelegateCommand<Assembly> RemoveParentAssemblyCommand { get; private set; }

        /// <summary>
        /// Команда для добавления родительского продукта.
        /// </summary>
        public DelegateCommand AddParentProductCommand { get; private set; }

        /// <summary>
        /// Команда для удаления родительского продукта.
        /// </summary>
        public DelegateCommand<Product> RemoveParentProductCommand { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProductFormViewModel"/>.
        /// </summary>
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

        /// <summary>
        /// Загружает все классификаторы.
        /// </summary>
        private void LoadClassifiers() => AllClassifiers = new ObservableCollection<ClassifierData>(_classifierService.GetAllClassifiers());

        /// <summary>
        /// Асинхронно загружает все продукты.
        /// </summary>
        private async void LoadProducts() => _allProducts = await _productService.GetProductsAsync();

        /// <summary>
        /// Фильтрует классификаторы на основе введенной строки номера класса.
        /// </summary>
        private void FilterClassifiers()
        {
            if (AllClassifiers == null) { FilteredClassifiers = new ObservableCollection<ClassifierData>(); return; }
            if (string.IsNullOrWhiteSpace(ClassNumberString)) { FilteredClassifiers = new ObservableCollection<ClassifierData>(AllClassifiers); return; }
            FilteredClassifiers = new ObservableCollection<ClassifierData>(AllClassifiers.Where(c => c.Code.StartsWith(ClassNumberString, StringComparison.OrdinalIgnoreCase)).OrderBy(c => c.Code).ToList());
        }

        /// <summary>
        /// Фильтрует продукты на основе введенной строки номера класса.
        /// </summary>
        private void FilterProducts()
        {
            if (_allProducts == null || ClassNumberString?.Length != 6) { FilteredProducts = new ObservableCollection<Product>(); return; }
            var records = _allProducts.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(ClassNumberString)) records = records.Where(r => r.EskdNumber?.ClassNumber?.Number.ToString("D6").StartsWith(ClassNumberString) == true);
            FilteredProducts = new ObservableCollection<Product>(records.OrderBy(r => r.EskdNumber.FullCode).ToList());
        }

        /// <summary>
        /// Вызывается при изменении части децимального номера для обновления свойства ESKDNumberString.
        /// </summary>
        private void OnESKDNumberPartChanged() => RaisePropertyChanged(nameof(ESKDNumberString));

        /// <summary>
        /// Вызывается при изменении строки номера класса.
        /// </summary>
        private void OnClassNumberStringChanged()
        {
            if (_isUpdatingFromSelection) return;
            FilterClassifiers();
            FilterProducts();
            OnESKDNumberPartChanged();
        }

        /// <summary>
        /// Добавляет родительскую сборку к текущему продукту.
        /// </summary>
        private void AddParentAssembly()
        {
            _dialogService.ShowDialog("SelectAssemblyDialog", new DialogParameters(), r =>
            {
                if (r.Result == ButtonResult.OK) { var selectedAssemblies = r.Parameters.GetValue<List<Assembly>>(DialogParameterKeys.SelectedAssemblies); if (selectedAssemblies != null) foreach (var assembly in selectedAssemblies) if (!ParentAssemblies.Any(p => p.Id == assembly.Id)) ParentAssemblies.Add(assembly); }
            });
        }

        /// <summary>
        /// Удаляет родительскую сборку из текущего продукта.
        /// </summary>
        /// <param name="assembly">Удаляемая родительская сборка.</param>
        private void RemoveParentAssembly(Assembly assembly) { if (assembly != null) ParentAssemblies.Remove(assembly); }

        /// <summary>
        /// Добавляет родительский продукт к текущему продукту.
        /// </summary>
        private void AddParentProduct()
        {
            _dialogService.ShowDialog("SelectProductDialog", new DialogParameters(), r =>
            {
                if (r.Result == ButtonResult.OK) { var selectedProducts = r.Parameters.GetValue<List<Product>>(DialogParameterKeys.SelectedProducts); if (selectedProducts != null) foreach (var product in selectedProducts) if (!ParentProducts.Any(p => p.Id == product.Id)) ParentProducts.Add(product); }
            });
        }

        /// <summary>
        /// Удаляет родительский продукт из текущего продукта.
        /// </summary>
        /// <param name="product">Удаляемый родительский продукт.</param>
        private void RemoveParentProduct(Product product) { if (product != null) ParentProducts.Remove(product); }

        /// <summary>
        /// Сохраняет текущий продукт (добавляет или обновляет).
        /// </summary>
        private async void Save()
        {
            Product.EskdNumber.CompanyCode = CompanyCode;
            Product.EskdNumber.DetailNumber = DetailNumber;
            Product.EskdNumber.Version = Version;

            if (Product.EskdNumber.ClassNumber == null) Product.EskdNumber.ClassNumber = new Classifier();

            if (SelectedClassifier != null)
            {
                Product.EskdNumber.ClassNumber.Number = int.Parse(SelectedClassifier.Code);
                Product.EskdNumber.ClassNumber.Name = SelectedClassifier.Description;
            }
            else if (int.TryParse(ClassNumberString, out int classNumberValue))
            {
                Product.EskdNumber.ClassNumber.Number = classNumberValue;
                var classifierData = _classifierService.GetClassifierByCode(ClassNumberString);
                Product.EskdNumber.ClassNumber.Name = classifierData?.Description ?? "<неопознанный код>";
            }

            try
            {
                if (ParentProducts.Any())
                {
                    if (Product.Id == 0) await _productService.AddProductAsync(Product, ParentAssemblies.Select(a => a.Id).ToList());
                    await _assemblyService.ConvertProductToAssemblyAsync(Product.Id, ParentProducts.ToList());
                }
                else
                {
                    if (Product.Id == 0) 
                        await _productService.AddProductAsync(Product, ParentAssemblies.Select(a => a.Id).ToList());
                    else 
                    {
                        await _productService.UpdateProductAsync(Product);
                        await _productService.UpdateProductParentAssembliesAsync(Product.Id, ParentAssemblies.ToList());
                    }
                }
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при сохранении продукта: {ex.Message}", ex);
                _dialogService.ShowDialog("ErrorDialog", new DialogParameters($"Message=Произошла ошибка: {ex.Message}"), null);
            }
        }

        /// <summary>
        /// Отменяет изменения и закрывает диалоговое окно.
        /// </summary>
        private void Cancel() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));

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
