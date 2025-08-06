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

namespace DetailViewer.Modules.Explorer.ViewModels
{
    public class AssembliesDashboardViewModel : BindableBase
    {
        private readonly IAssemblyService _assemblyService;
        private readonly IDialogService _dialogService;
        private readonly ILogger _logger;

        private string? _statusText;
        public string? StatusText
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

        private ObservableCollection<Assembly> _assemblies;
        public ObservableCollection<Assembly> Assemblies
        {
            get { return _assemblies; }
            set { SetProperty(ref _assemblies, value); }
        }

        private Assembly? _selectedAssembly;
        public Assembly? SelectedAssembly
        {
            get => _selectedAssembly;
            set => SetProperty(ref _selectedAssembly, value);
        }

        private List<Assembly>? _allAssemblies;

        private string? _eskdNumberFilter;
        public string? EskdNumberFilter
        {
            get { return _eskdNumberFilter; }
            set { SetProperty(ref _eskdNumberFilter, value, ApplyFilters); }
        }

        private string? _nameFilter;
        public string? NameFilter
        {
            get { return _nameFilter; }
            set { SetProperty(ref _nameFilter, value, ApplyFilters); }
        }

        public DelegateCommand AddAssemblyCommand { get; private set; }
        public DelegateCommand EditAssemblyCommand { get; private set; }
        public DelegateCommand DeleteAssemblyCommand { get; private set; }

        public AssembliesDashboardViewModel(IAssemblyService assemblyService, IDialogService dialogService, ILogger logger)
        {
            _assemblyService = assemblyService;
            _dialogService = dialogService;
            _logger = logger;

            _assemblies = new ObservableCollection<Assembly>();
            StatusText = "Готово";

            AddAssemblyCommand = new DelegateCommand(AddAssembly);
            EditAssemblyCommand = new DelegateCommand(EditAssembly, () => SelectedAssembly != null).ObservesProperty(() => SelectedAssembly);
            DeleteAssemblyCommand = new DelegateCommand(DeleteAssembly, () => SelectedAssembly != null).ObservesProperty(() => SelectedAssembly);

            Task.Run(LoadData);
        }

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

        private void DeleteAssembly()
        {
            _logger.Log("Deleting assembly");
            _dialogService.ShowDialog("ConfirmationDialog", new DialogParameters { { "message", $"Вы уверены, что хотите удалить запись: {SelectedAssembly.EskdNumber?.FullCode ?? "<unf>"}?" } }, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    await _assemblyService.DeleteAssemblyAsync(SelectedAssembly.Id);
                    await LoadData();
                }
            });
        }

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

        private async Task LoadData()
        {
            _logger.Log("Loading data for AssembliesDashboard");
            IsBusy = true;
            StatusText = "Загрузка данных...";
            try
            {
                _allAssemblies = await _assemblyService.GetAssembliesAsync();
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