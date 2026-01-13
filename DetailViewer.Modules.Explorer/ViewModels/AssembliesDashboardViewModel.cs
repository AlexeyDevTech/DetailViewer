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
    /// <summary>
    /// ViewModel для панели управления сборками.
    /// </summary>
    public class AssembliesDashboardViewModel : BindableBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IAssemblyService _assemblyService;
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

        private ObservableCollection<Assembly> _assemblies;
        /// <summary>
        /// Коллекция сборок, отображаемых на панели.
        /// </summary>
        public ObservableCollection<Assembly> Assemblies
        {
            get { return _assemblies; }
            set { SetProperty(ref _assemblies, value); }
        }

        private Assembly? _selectedAssembly;
        /// <summary>
        /// Выбранная сборка.
        /// </summary>
        public Assembly? SelectedAssembly
        {
            get => _selectedAssembly;
            set => SetProperty(ref _selectedAssembly, value);
        }

        private List<Assembly> _allAssemblies = new List<Assembly>();

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
        /// Команда для добавления новой сборки.
        /// </summary>
        public DelegateCommand AddAssemblyCommand { get; private set; }

        /// <summary>
        /// Команда для редактирования выбранной сборки.
        /// </summary>
        public DelegateCommand EditAssemblyCommand { get; private set; }

        /// <summary>
        /// Команда для удаления выбранной сборки.
        /// </summary>
        public DelegateCommand DeleteAssemblyCommand { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AssembliesDashboardViewModel"/>.
        /// </summary>
        public AssembliesDashboardViewModel(IAssemblyService assemblyService, ISettingsService settingsService, IDialogService dialogService, ILogger logger, IEventAggregator eventAggregator)
        {
            _settingsService = settingsService;
            _assemblyService = assemblyService;
            _dialogService = dialogService;
            _logger = logger;
            _eventAggregator = eventAggregator;

            _assemblies = new ObservableCollection<Assembly>();
            StatusText = "Готово";

            AddAssemblyCommand = new DelegateCommand(AddAssembly);
            EditAssemblyCommand = new DelegateCommand(EditAssembly, () => SelectedAssembly != null).ObservesProperty(() => SelectedAssembly);
            DeleteAssemblyCommand = new DelegateCommand(DeleteAssembly, () => SelectedAssembly != null).ObservesProperty(() => SelectedAssembly);

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
        /// Открывает диалог редактирования сборки.
        /// </summary>
        private void EditAssembly()
        {
            _logger.Log("Editing assembly");
            var parameters = new DialogParameters { { "assembly", SelectedAssembly } };
            _dialogService.ShowDialog("AssemblyForm", parameters, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await LoadData();
                }
            });
        }

        /// <summary>
        /// Удаляет выбранную сборку.
        /// </summary>
        private void DeleteAssembly()
        {
            if (SelectedAssembly == null)
            {
                return;
            }

            _logger.Log("Deleting assembly");
            _dialogService.ShowDialog("ConfirmationDialog", new DialogParameters { { "message", $"Вы уверены, что хотите удалить запись: {SelectedAssembly.EskdNumber?.FullCode ?? "<unf>"}?" } }, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (SelectedAssembly != null)
                    {
                        await _assemblyService.DeleteAssemblyAsync(SelectedAssembly.Id);
                        await LoadData();
                    }
                }
            });
        }

        /// <summary>
        /// Открывает диалог добавления новой сборки.
        /// </summary>
        private void AddAssembly()
        {
            _logger.Log("Adding assembly");
            _dialogService.ShowDialog("AssemblyForm", null, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await LoadData();
                }
            });
        }

        /// <summary>
        /// Асинхронно загружает данные сборок.
        /// </summary>
        private async Task LoadData()
        {
            _logger.Log("Loading data for AssembliesDashboard");
            IsBusy = true;
            StatusText = "Загрузка данных...";
            try
            {
                var records = await _assemblyService.GetAssembliesAsync();
                _allAssemblies = records.Where(r => r.EskdNumber.CompanyCode == _settingsService.LoadSettings().DefaultCompanyCode).ToList();
                ApplyFilters();
                StatusText = $"Данные успешно загружены.";
                _logger.LogInfo("Assemblies loaded successfully.");
            }
            catch (Exception ex)
            {
                StatusText = "Ошибка при загрузке данных.";
                _logger.LogError($"Error loading assemblies: {ex.Message}", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Применяет фильтры к списку сборок.
        /// </summary>
        private void ApplyFilters()
        {
            _logger.Log("Applying filters to assemblies");
            if (_allAssemblies == null) return;

            var filteredAssemblies = _allAssemblies.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(EskdNumberFilter))
            {
                filteredAssemblies = filteredAssemblies.Where(a => a.EskdNumber != null && a.EskdNumber.FullCode != null && a.EskdNumber.FullCode.Contains(EskdNumberFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(NameFilter))
            {
                filteredAssemblies = filteredAssemblies.Where(a => !string.IsNullOrEmpty(a.Name) && a.Name.Contains(NameFilter, StringComparison.OrdinalIgnoreCase));
            }

            Assemblies.Clear();
            foreach (var assembly in filteredAssemblies)
            {
                Assemblies.Add(assembly);
            }

            StatusText = $"Отобрано записей: {Assemblies.Count}";
        }
    }
}