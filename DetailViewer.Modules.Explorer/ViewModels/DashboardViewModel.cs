using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Linq;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace DetailViewer.Modules.Explorer.ViewModels
{
    public class DashboardViewModel : BindableBase
    {
        private readonly IDocumentDataServiceFactory _documentDataServiceFactory;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private AppSettings _appSettings;
        private ILogger _logger;

        private DataSourceType _selectedDataSourceType;
        public DataSourceType SelectedDataSourceType
        {
            get { return _selectedDataSourceType; }
            set
            {
                SetProperty(ref _selectedDataSourceType, value);
                _appSettings.CurrentDataSourceType = value;
                _settingsService.SaveSettingsAsync(_appSettings);
                RaisePropertyChanged(nameof(IsExcelSelected));
                RaisePropertyChanged(nameof(IsGoogleSheetsSelected));
            }
        }

        public bool IsExcelSelected => SelectedDataSourceType == DataSourceType.Excel;
        public bool IsGoogleSheetsSelected => SelectedDataSourceType == DataSourceType.GoogleSheets;

        private string _excelFilePath;
        public string ExcelFilePath
        {
            get { return _excelFilePath; }
            set
            {
                SetProperty(ref _excelFilePath, value);
                _appSettings.LastUsedExcelFilePath = value;
                _settingsService.SaveSettingsAsync(_appSettings);
            }
        }

        private string _googleSheetId;
        public string GoogleSheetId
        {
            get { return _googleSheetId; }
            set
            {
                SetProperty(ref _googleSheetId, value);
                _appSettings.LastUsedGoogleSheetId = value;
                _settingsService.SaveSettingsAsync(_appSettings);
            }
        }

        private string _googleSheetName;
        public string GoogleSheetName
        {
            get { return _googleSheetName; }
            set
            {
                SetProperty(ref _googleSheetName, value);
                _appSettings.LastUsedGoogleSheetName = value;
                _settingsService.SaveSettingsAsync(_appSettings);
            }
        }

        private ObservableCollection<DocumentRecord> _documentRecords;
        public ObservableCollection<DocumentRecord> DocumentRecords
        {
            get { return _documentRecords; }
            set { SetProperty(ref _documentRecords, value); }
        }

        public DelegateCommand FillFormCommand { get; private set; }
        public DelegateCommand OpenTableCommand { get; private set; }
        public DelegateCommand LoadDataCommand { get; private set; }
        public DelegateCommand SaveDataCommand { get; private set; }
        public DelegateCommand BrowseExcelFileCommand { get; private set; }

        public DashboardViewModel(IDocumentDataServiceFactory documentDataServiceFactory, IDialogService dialogService, ISettingsService settingsService, AppSettings appSettings, ILogger logger)
        {
            _documentDataServiceFactory = documentDataServiceFactory;
            _dialogService = dialogService;
            _settingsService = settingsService;
            _appSettings = appSettings;
            _logger = logger;

            DocumentRecords = new ObservableCollection<DocumentRecord>();

            // Load settings
            SelectedDataSourceType = _appSettings.CurrentDataSourceType;
            ExcelFilePath = _appSettings.LastUsedExcelFilePath;
            GoogleSheetId = _appSettings.LastUsedGoogleSheetId;
            GoogleSheetName = _appSettings.LastUsedGoogleSheetName;

            FillFormCommand = new DelegateCommand(FillForm);
            OpenTableCommand = new DelegateCommand(async () => await OpenTable());
            LoadDataCommand = new DelegateCommand(async () => await LoadData());
            SaveDataCommand = new DelegateCommand(async () => await SaveData());
            BrowseExcelFileCommand = new DelegateCommand(BrowseExcelFile);
        }

        private async Task SaveData()
        {
            try
            {
                var documentDataService = _documentDataServiceFactory.CreateService(SelectedDataSourceType);
                if (SelectedDataSourceType == DataSourceType.Excel)
                {
                    if (string.IsNullOrEmpty(ExcelFilePath))
                    {
                        var saveFileDialog = new SaveFileDialog
                        {
                            Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                            Title = "Select Excel File to Save"
                        };

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            ExcelFilePath = saveFileDialog.FileName;
                        }
                        else
                        {
                            return; // User cancelled
                        }
                    }
                    await documentDataService.WriteRecordsAsync(ExcelFilePath, DocumentRecords.ToList());
                }
                else if (SelectedDataSourceType == DataSourceType.GoogleSheets)
                {
                    if (string.IsNullOrEmpty(GoogleSheetId) || string.IsNullOrEmpty(GoogleSheetName))
                    {
                        _logger.LogWarning("Google Sheet ID and Sheet Name must be provided to save data.");
                        return;
                    }
                    await documentDataService.WriteRecordsAsync(GoogleSheetId, DocumentRecords.ToList(), GoogleSheetName);
                }
                _logger.LogInformation("Data saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving data: {ex.Message}", ex);
            }
        }

        private void FillForm()
        {
            _dialogService.ShowDialog("DocumentRecordForm", new DialogParameters(), async r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var newRecord = r.Parameters.GetValue<DocumentRecord>("record");
                    DocumentRecords.Add(newRecord);
                    await SaveData(); // Save data after adding a new record
                }
            });
        }

        private async Task OpenTable()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                var documentDataService = _documentDataServiceFactory.CreateService(SelectedDataSourceType);
                List<DocumentRecord> records = new List<DocumentRecord>();

                if (SelectedDataSourceType == DataSourceType.Excel)
                {
                    if (string.IsNullOrEmpty(ExcelFilePath))
                    {
                        BrowseExcelFile();
                        if (string.IsNullOrEmpty(ExcelFilePath)) return; // User cancelled
                    }
                    records = await documentDataService.ReadRecordsAsync(ExcelFilePath);
                }
                else if (SelectedDataSourceType == DataSourceType.GoogleSheets)
                {
                    if (string.IsNullOrEmpty(GoogleSheetId) || string.IsNullOrEmpty(GoogleSheetName))
                    {
                        _logger.LogWarning("Google Sheet ID and Sheet Name must be provided to load data.");
                        return;
                    }
                    records = await documentDataService.ReadRecordsAsync(GoogleSheetId, GoogleSheetName);
                }

                DocumentRecords.Clear();
                foreach (var record in records)
                {
                    DocumentRecords.Add(record);
                }
                _logger.LogInformation("Data loaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading data: {ex.Message}", ex);
            }
        }

        private void BrowseExcelFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Select Excel File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ExcelFilePath = openFileDialog.FileName;
            }
        }
    }
}