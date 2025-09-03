#nullable enable

using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using DetailViewer.Infrastructure.Services;
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
    /// ViewModel для формы создания/редактирования сборки.
    /// </summary>
    public class AssemblyFormViewModel : BindableBase, IDialogAware
    {
        private readonly IAssemblyService _assemblyService;
        private readonly IProductService _productService;
        private readonly IClassifierService _classifierService;
        private readonly IEskdNumberService _eskdNumberService;
        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private readonly IActiveUserService _activeUserService;
        private readonly IDialogService _dialogService;

        /// <summary>
        /// Заголовок диалогового окна.
        /// </summary>
        public string Title => "Форма сборки";

        /// <summary>
        /// Событие, запрашивающее закрытие диалогового окна.
        /// </summary>
        public event Action<IDialogResult>? RequestClose;

        private Assembly _assembly;
        /// <summary>
        /// Редактируемая или создаваемая сборка.
        /// </summary>
        public Assembly Assembly { get => _assembly; set => SetProperty(ref _assembly, value); }

        private ObservableCollection<Assembly> _parentAssemblies;
        /// <summary>
        /// Коллекция родительских сборок для текущей сборки.
        /// </summary>
        public ObservableCollection<Assembly> ParentAssemblies { get => _parentAssemblies; set => SetProperty(ref _parentAssemblies, value); }

        private ObservableCollection<Product> _relatedProducts;
        /// <summary>
        /// Коллекция продуктов, связанных с текущей сборкой.
        /// </summary>
        public ObservableCollection<Product> RelatedProducts { get => _relatedProducts; set => SetProperty(ref _relatedProducts, value); }

        private string? _companyCode, _classNumberString;
        private int _detailNumber;
        private int? _version;

        /// <summary>
        /// Код компании для децимального номера сборки.
        /// </summary>
        public string? CompanyCode { get => _companyCode; set => SetProperty(ref _companyCode, value, OnESKDNumberPartChanged); }

        /// <summary>
        /// Строковое представление номера класса для децимального номера сборки.
        /// </summary>
        public string? ClassNumberString { get => _classNumberString; set => SetProperty(ref _classNumberString, value, OnClassNumberStringChanged); }

        /// <summary>
        /// Порядковый номер детали для децимального номера сборки.
        /// </summary>
        public int DetailNumber { get => _detailNumber; set => SetProperty(ref _detailNumber, value, OnESKDNumberPartChanged); }

        /// <summary>
        /// Номер версии для децимального номера сборки.
        /// </summary>
        public int? Version { get => _version; set => SetProperty(ref _version, value, OnESKDNumberPartChanged); }

        /// <summary>
        /// Полное строковое представление децимального номера сборки.
        /// </summary>
        public string ESKDNumberString
        {
            get
            {
                if (string.IsNullOrEmpty(CompanyCode) || string.IsNullOrEmpty(ClassNumberString) || DetailNumber == 0) return string.Empty;
                try { string baseCode = $"{CompanyCode}.{int.Parse(ClassNumberString):D6}.{DetailNumber:D3}"; return Version.HasValue ? $"{baseCode}-{Version.Value:D2}" : baseCode; }
                catch (FormatException) { return "Invalid ClassNumber format"; }
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

        private List<Assembly>? _allAssemblies;
        private ObservableCollection<Assembly>? _filteredAssemblies;
        /// <summary>
        /// Отфильтрованные сборки.
        /// </summary>
        public ObservableCollection<Assembly>? FilteredAssemblies { get => _filteredAssemblies; set => SetProperty(ref _filteredAssemblies, value); }

        /// <summary>
        /// Команда для сохранения сборки.
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
        /// Команда для добавления связанного продукта.
        /// </summary>
        public DelegateCommand AddRelatedProductCommand { get; private set; }

        /// <summary>
        /// Команда для удаления связанного продукта.
        /// </summary>
        public DelegateCommand<Product> RemoveRelatedProductCommand { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AssemblyFormViewModel"/>.
        /// </summary>
        public AssemblyFormViewModel(IAssemblyService assemblyService, IEskdNumberService eskdNumberService, IProductService productService, IClassifierService classifierService, ILogger logger, ISettingsService settingsService, IActiveUserService activeUserService, IDialogService dialogService)
        {
            _assemblyService = assemblyService;
            _productService = productService;
            _classifierService = classifierService;
            _eskdNumberService = eskdNumberService;
            _logger = logger;
            _settingsService = settingsService;
            _activeUserService = activeUserService;
            _dialogService = dialogService;

            _assembly = new Assembly { EskdNumber = new ESKDNumber(), Author = _activeUserService.CurrentUser?.ShortName };
            _parentAssemblies = new ObservableCollection<Assembly>();
            _relatedProducts = new ObservableCollection<Product>();

            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
            AddParentAssemblyCommand = new DelegateCommand(AddParentAssembly);
            RemoveParentAssemblyCommand = new DelegateCommand<Assembly>(RemoveParentAssembly);
            AddRelatedProductCommand = new DelegateCommand(AddRelatedProduct);
            RemoveRelatedProductCommand = new DelegateCommand<Product>(RemoveRelatedProduct);

            LoadAssemblies();
        }

        /// <summary>
        /// Загружает все классификаторы.
        /// </summary>
        private void LoadClassifiers() => AllClassifiers = new ObservableCollection<ClassifierData>(_classifierService.GetAllClassifiers());

        /// <summary>
        /// Асинхронно загружает все сборки.
        /// </summary>
        private async void LoadAssemblies() => _allAssemblies = await _assemblyService.GetAssembliesAsync();

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
        /// Фильтрует сборки на основе введенной строки номера класса.
        /// </summary>
        private void FilterAssemblies()
        {
            if (_allAssemblies == null || ClassNumberString?.Length != 6) { FilteredAssemblies = new ObservableCollection<Assembly>(); return; }
            var records = _allAssemblies.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(ClassNumberString)) records = records.Where(r => (r.EskdNumber?.ClassNumber?.Number.ToString("D6").StartsWith(ClassNumberString) ?? false));
            FilteredAssemblies = new ObservableCollection<Assembly>(records.OrderBy(r => r.EskdNumber.FullCode).ToList());
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
            FilterAssemblies();
            OnESKDNumberPartChanged();
        }

        /// <summary>
        /// Добавляет родительскую сборку к текущей сборке.
        /// </summary>
        private void AddParentAssembly()
        {
            _dialogService.ShowDialog("SelectAssemblyDialog", new DialogParameters(), r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var selectedAssemblies = r.Parameters.GetValue<List<Assembly>>(DialogParameterKeys.SelectedAssemblies);
                    if (selectedAssemblies != null) foreach (var assembly in selectedAssemblies) if (!ParentAssemblies.Any(p => p.Id == assembly.Id)) ParentAssemblies.Add(assembly);
                }
            });
        }
        /// <summary>
        /// Асинхронно находит следующий доступный номер детали.
        /// </summary>
        private async Task FindNextDetailNumber()
        {
            if (string.IsNullOrWhiteSpace(ClassNumberString))
            {
                DetailNumber = 0;
                return;
            }
            DetailNumber = await _eskdNumberService.GetNextDetailNumberAsync(ClassNumberString);
        }

        /// <summary>
        /// Удаляет родительскую сборку из текущей сборки.
        /// </summary>
        /// <param name="assembly">Удаляемая родительская сборка.</param>
        private void RemoveParentAssembly(Assembly assembly) { if (assembly != null) ParentAssemblies.Remove(assembly); }

        /// <summary>
        /// Добавляет связанный продукт к текущей сборке.
        /// </summary>
        private void AddRelatedProduct()
        {
            _dialogService.ShowDialog("SelectProductDialog", new DialogParameters() { { "SelectProducts", RelatedProducts.ToList() } }, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var selectedProducts = r.Parameters.GetValue<List<Product>>(DialogParameterKeys.SelectedProducts);
                    if (selectedProducts != null) foreach (var product in selectedProducts) if (!RelatedProducts.Any(p => p.Id == product.Id)) RelatedProducts.Add(product);
                }
            });
        }

        /// <summary>
        /// Удаляет связанный продукт из текущей сборки.
        /// </summary>
        /// <param name="product">Удаляемый связанный продукт.</param>
        private void RemoveRelatedProduct(Product product) { if (product != null) RelatedProducts.Remove(product); }

        /// <summary>
        /// Сохраняет текущую сборку (добавляет или обновляет).
        /// </summary>
        private async void Save()
        {
            Assembly.EskdNumber.CompanyCode = CompanyCode;
            Assembly.EskdNumber.DetailNumber = DetailNumber;
            Assembly.EskdNumber.Version = Version;

            if (Assembly.EskdNumber.ClassNumber == null) Assembly.EskdNumber.ClassNumber = new Classifier();

            if (SelectedClassifier != null)
            {
                Assembly.EskdNumber.ClassNumber.Number = int.Parse(SelectedClassifier.Code);
                Assembly.EskdNumber.ClassNumber.Name = SelectedClassifier.Description;
            }
            else if (int.TryParse(ClassNumberString, out int classNumberValue))
            {
                Assembly.EskdNumber.ClassNumber.Number = classNumberValue;
                var classifierData = _classifierService.GetClassifierByCode(ClassNumberString);
                Assembly.EskdNumber.ClassNumber.Name = classifierData?.Description ?? "<неопознанный код>";
            }

            if (Assembly.Id == 0) await _assemblyService.AddAssemblyAsync(Assembly, ParentAssemblies.Select(a => a.Id).ToList(), RelatedProducts.Select(p => p.Id).ToList());
            else { await _assemblyService.UpdateAssemblyAsync(Assembly); await _assemblyService.UpdateAssemblyParentAssembliesAsync(Assembly.Id, ParentAssemblies.ToList()); await _assemblyService.UpdateAssemblyRelatedProductsAsync(Assembly.Id, RelatedProducts.ToList()); }
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
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
            if (parameters.ContainsKey("assembly"))
            {
                Assembly = parameters.GetValue<Assembly>("assembly");
                CompanyCode = Assembly.EskdNumber.CompanyCode;
                ClassNumberString = Assembly.EskdNumber.ClassNumber?.Number.ToString("D6");
                DetailNumber = Assembly.EskdNumber.DetailNumber;
                Version = Assembly.EskdNumber.Version;
                var parentAssemblies = await _assemblyService.GetParentAssembliesAsync(Assembly.Id);
                foreach (var item in parentAssemblies) ParentAssemblies.Add(item);
                var relatedProducts = await _assemblyService.GetRelatedProductsAsync(Assembly.Id);
                foreach (var item in relatedProducts) RelatedProducts.Add(item);
            }
            else { CompanyCode = _settingsService.LoadSettings().DefaultCompanyCode; Assembly.EskdNumber.CompanyCode = CompanyCode; }
        }
    }
}
