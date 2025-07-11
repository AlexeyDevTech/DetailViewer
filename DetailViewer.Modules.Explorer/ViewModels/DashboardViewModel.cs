using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DetailViewer.Modules.Explorer.ViewModels
{
    public class DashboardViewModel : BindableBase
    {
        private readonly IDocumentDataService _documentDataService;
        private readonly IDialogService _dialogService;
        private readonly ILogger _logger;
        private readonly IProfileService _profileService;
        private readonly ISettingsService _settingsService;

        private string _statusText;
        public string StatusText
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

        private ObservableCollection<DocumentRecord> _documentRecords;
        public ObservableCollection<DocumentRecord> DocumentRecords
        {
            get { return _documentRecords; }
            set { SetProperty(ref _documentRecords, value); }
        }

        private List<DocumentRecord> _allRecords;

        private string _eskdNumberFilter;
        public string EskdNumberFilter
        {
            get { return _eskdNumberFilter; }
            set { SetProperty(ref _eskdNumberFilter, value, ApplyFilters); }
        }

        private string _nameFilter;
        public string NameFilter
        {
            get { return _nameFilter; }
            set { SetProperty(ref _nameFilter, value, ApplyFilters); }
        }

        private string _assemblyNameFilter;
        public string AssemblyNameFilter
        {
            get { return _assemblyNameFilter; }
            set { SetProperty(ref _assemblyNameFilter, value, ApplyFilters); }
        }

        private string _productNameFilter;
        public string ProductNameFilter
        {
            get { return _productNameFilter; }
            set { SetProperty(ref _productNameFilter, value, ApplyFilters); }
        }

        private string _fullNameFilter;
        public string FullNameFilter
        {
            get { return _fullNameFilter; }
            set { SetProperty(ref _fullNameFilter, value, ApplyFilters); }
        }

        private string _yastCodeFilter;
        public string YastCodeFilter
        {
            get { return _yastCodeFilter; }
            set { SetProperty(ref _yastCodeFilter, value, ApplyFilters); }
        }

        private string _assemblyNumberFilter;
        public string AssemblyNumberFilter
        {
            get { return _assemblyNumberFilter; }
            set { SetProperty(ref _assemblyNumberFilter, value, ApplyFilters); }
        }

        private string _productNumberFilter;
        public string ProductNumberFilter
        {
            get { return _productNumberFilter; }
            set { SetProperty(ref _productNumberFilter, value, ApplyFilters); }
        }

        private bool _onlyMyRecordsFilter;
        public bool OnlyMyRecordsFilter
        {
            get { return _onlyMyRecordsFilter; }
            set { SetProperty(ref _onlyMyRecordsFilter, value, ApplyFilters); }
        }

        public DelegateCommand FillFormCommand { get; private set; }
        public DelegateCommand FillBasedOnCommand { get; private set; }
        public DelegateCommand EditCommand { get; private set; }

        private DocumentRecord _selectedRecord;
        public DocumentRecord SelectedRecord
        {
            get { return _selectedRecord; }
            set { SetProperty(ref _selectedRecord, value, () => { 
                FillBasedOnCommand.RaiseCanExecuteChanged(); 
                EditCommand.RaiseCanExecuteChanged(); 
            }); }
        }

        public DashboardViewModel(IDocumentDataService documentDataService, IDialogService dialogService, ILogger logger, IProfileService profileService, ISettingsService settingsService)
        {
            _documentDataService = documentDataService;
            _dialogService = dialogService;
            _logger = logger;
            _profileService = profileService;
            _settingsService = settingsService;

            DocumentRecords = new ObservableCollection<DocumentRecord>();
            StatusText = "Готово";

            FillFormCommand = new DelegateCommand(FillForm);
            FillBasedOnCommand = new DelegateCommand(FillBasedOn, () => SelectedRecord != null);
            EditCommand = new DelegateCommand(Edit, () => SelectedRecord != null && SelectedRecord.FullName == GetActiveProfileFullName());
            LoadData();
        }

        private void FillForm()
        {
            _dialogService.ShowDialog("DocumentRecordForm", new DialogParameters(), async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var newRecord = r.Parameters.GetValue<DocumentRecord>("record");
                    await _documentDataService.AddRecordAsync(newRecord);
                    await LoadData();
                }
            });
        }

        private void FillBasedOn()
        {
            var parameters = new DialogParameters
            {
                { "record", SelectedRecord },
                { "isEditing", false }
            };

            _dialogService.ShowDialog("DocumentRecordForm", parameters, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var newRecord = r.Parameters.GetValue<DocumentRecord>("record");
                    await _documentDataService.AddRecordAsync(newRecord);
                    await LoadData();
                }
            });
        }

        private void Edit()
        {
            var parameters = new DialogParameters
            {
                { "record", SelectedRecord },
                { "isEditing", true }
            };

            _dialogService.ShowDialog("DocumentRecordForm", parameters, async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var updatedRecord = r.Parameters.GetValue<DocumentRecord>("record");
                    await _documentDataService.UpdateRecordAsync(updatedRecord);
                    await LoadData();
                }
            });
        }

        private string GetActiveProfileFullName()
        {
            var settings = _settingsService.LoadSettings();
            var activeProfile = _profileService.GetAllProfilesAsync().Result.FirstOrDefault(p => p.Id == settings.ActiveProfileId);
            return $"{activeProfile.LastName} {activeProfile.FirstName.FirstOrDefault()}.{activeProfile.MiddleName.FirstOrDefault()}.";
        }

        private async Task LoadData()
        {
            IsBusy = true;
            StatusText = "Загрузка данных...";
            try
            {
                _allRecords = await _documentDataService.GetAllRecordsAsync();
                ApplyFilters();
                StatusText = $"Данные успешно загружены. Записей: {DocumentRecords.Count}";
                _logger.LogInformation("Data loaded successfully.");
            }
            catch (Exception ex)
            {
                StatusText = "Ошибка при загрузке данных.";
                _logger.LogError($"Error loading data: {ex.Message}", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilters()
        {
            if (_allRecords == null) return;

            var filteredRecords = _allRecords.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(EskdNumberFilter))
            {
                filteredRecords = filteredRecords.Where(r => r.ESKDNumber != null && r.ESKDNumber.FullCode != null && r.ESKDNumber.FullCode.Contains(EskdNumberFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(YastCodeFilter))
            {
                filteredRecords = filteredRecords.Where(r => !string.IsNullOrEmpty(r.YASTCode) && r.YASTCode.Contains(YastCodeFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(NameFilter))
            {
                filteredRecords = filteredRecords.Where(r => !string.IsNullOrEmpty(r.Name) && r.Name.Contains(NameFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(AssemblyNumberFilter))
            {
                filteredRecords = filteredRecords.Where(r => r.AssemblyNumber != null && r.AssemblyNumber != null && r.AssemblyNumber.Contains(AssemblyNumberFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(AssemblyNameFilter))
            {
                filteredRecords = filteredRecords.Where(r => !string.IsNullOrEmpty(r.AssemblyName) && r.AssemblyName.Contains(AssemblyNameFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(ProductNumberFilter))
            {
                filteredRecords = filteredRecords.Where(r => r.ProductNumber != null && r.ProductNumber != null && r.ProductNumber.Contains(ProductNumberFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(ProductNameFilter))
            {
                filteredRecords = filteredRecords.Where(r => !string.IsNullOrEmpty(r.ProductName) && r.ProductName.Contains(ProductNameFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(FullNameFilter))
            {
                filteredRecords = filteredRecords.Where(r => !string.IsNullOrEmpty(r.FullName) && r.FullName.Contains(FullNameFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (OnlyMyRecordsFilter)
            {
                var settings = _settingsService.LoadSettings();
                var activeProfile = _profileService.GetAllProfilesAsync().Result.FirstOrDefault(p => p.Id == settings.ActiveProfileId);
                if (activeProfile != null)
                {
                    var activeProfileFullName = $"{activeProfile.LastName} {activeProfile.FirstName.FirstOrDefault()}.{activeProfile.MiddleName.FirstOrDefault()}.";
                    filteredRecords = filteredRecords.Where(r => !string.IsNullOrEmpty(r.FullName) && r.FullName == activeProfileFullName);
                }
            }

            DocumentRecords.Clear();
            foreach (var record in filteredRecords)
            {
                DocumentRecords.Add(record);
            }
        }
    }
}
