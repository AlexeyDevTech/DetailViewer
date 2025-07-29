using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using OfficeOpenXml;
using System.IO;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class SettingsViewModel : BindableBase, IDialogAware
    {
        private readonly IProfileService _profileService;
        private readonly ISettingsService _settingsService;
        private readonly IActiveUserService _activeUserService;
        private readonly IPasswordService _passwordService;
        private readonly IExcelExportService _exportService;
        private string _databasePath;
        private string _defaultCompanyCode;
        public string DefaultCompanyCode
        {
            get { return _defaultCompanyCode; }
            set { SetProperty(ref _defaultCompanyCode, value); }
        }
        private ObservableCollection<Profile> _profiles;
        private Profile _selectedProfile;
        private string _newProfileLastName;
        private string _newProfileFirstName;
        private string _newProfileMiddleName;
        private string _newProfilePassword;
        private double _importProgress;
        private bool _isImporting;
        private string _importStatus;
        private bool _runInTray;

        public bool RunInTray
        {
            get { return _runInTray; }
            set { SetProperty(ref _runInTray, value); }
        }

        public string DatabasePath
        {
            get { return _databasePath; }
            set { SetProperty(ref _databasePath, value); }
        }

        public ObservableCollection<Profile> Profiles
        {
            get { return _profiles; }
            set { SetProperty(ref _profiles, value); }
        }

        public Profile SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                if (SetProperty(ref _selectedProfile, value) && value != null)
                {
                    // Do nothing on selection
                }
            }
        }

        public string NewProfileLastName
        {
            get { return _newProfileLastName; }
            set { SetProperty(ref _newProfileLastName, value); }
        }

        public string NewProfileFirstName
        {
            get { return _newProfileFirstName; }
            set { SetProperty(ref _newProfileFirstName, value); }
        }

        public string NewProfileMiddleName
        {
            get { return _newProfileMiddleName; }
            set { SetProperty(ref _newProfileMiddleName, value); }
        }

        public string NewProfilePassword
        {
            get { return _newProfilePassword; }
            set { SetProperty(ref _newProfilePassword, value); }
        }

        public double ImportProgress
        {
            get { return _importProgress; }
            set { SetProperty(ref _importProgress, value); }
        }

        public bool IsImporting
        {
            get { return _isImporting; }
            set { SetProperty(ref _isImporting, value); }
        }

        public string ImportStatus
        {
            get { return _importStatus; }
            set { SetProperty(ref _importStatus, value); }
        }

        public bool IsAdminOrModerator { get; private set; }
        public IEnumerable<Role> Roles { get; private set; }
        private Role _newProfileRole;
        private IExcelImportService _importService;

        public Role NewProfileRole
        {
            get => _newProfileRole;
            set => SetProperty(ref _newProfileRole, value);
        }

        public DelegateCommand ImportCommand { get; private set; }
        public DelegateCommand ExportCommand { get; private set; }
        public DelegateCommand AddProfileCommand { get; private set; }
        public DelegateCommand SaveProfileCommand { get; private set; }
        public DelegateCommand DeleteProfileCommand { get; private set; }
        public DelegateCommand ChangeDatabasePathCommand { get; private set; }
        public DelegateCommand<string> CloseDialogCommand { get; private set; }

        public string Title => "Настройки";

        public event Action<IDialogResult> RequestClose;

                private readonly IDialogService _dialogService;

        public SettingsViewModel(IProfileService profileService, 
                                 ISettingsService settingsService, 
                                 IActiveUserService activeUserService, 
                                 IPasswordService passwordService,
                                 IExcelExportService exportService,
                                 IExcelImportService importService,
                                 IDialogService dialogService)
        {
            
            _profileService = profileService;
            _settingsService = settingsService;
            _activeUserService = activeUserService;
            _passwordService = passwordService;
            _exportService = exportService;
            _importService = importService;
            _dialogService = dialogService;

            //DatabasePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "detailviewer.db");
            var settings = _settingsService.LoadSettings();
            DatabasePath = settings.DatabasePath;
            DefaultCompanyCode = settings.DefaultCompanyCode;
            RunInTray = settings.RunInTray;

            ImportCommand = new DelegateCommand(Import);
            ExportCommand = new DelegateCommand(Export);
            AddProfileCommand = new DelegateCommand(AddProfile);
            SaveProfileCommand = new DelegateCommand(SaveProfile, () => SelectedProfile != null).ObservesProperty(() => SelectedProfile);
            DeleteProfileCommand = new DelegateCommand(DeleteProfile, () => SelectedProfile != null).ObservesProperty(() => SelectedProfile);
            ChangeDatabasePathCommand = new DelegateCommand(ChangeDatabasePath);
            CloseDialogCommand = new DelegateCommand<string>(CloseDialog);

            IsAdminOrModerator = _activeUserService.CurrentUser?.Role == Role.Admin || _activeUserService.CurrentUser?.Role == Role.Moderator;
            RaisePropertyChanged(nameof(IsAdminOrModerator));

            Roles = Enum.GetValues(typeof(Role)).Cast<Role>();
            NewProfileRole = Role.Operator;

            LoadProfiles();
        }

        private async void LoadProfiles()
        {
            var profiles = await _profileService.GetAllProfilesAsync();
            Profiles = new ObservableCollection<Profile>(profiles);
            SelectedProfile = Profiles.FirstOrDefault(p => p.Id == _activeUserService.CurrentUser?.Id) ?? Profiles.FirstOrDefault();
        }

        private async void AddProfile()
        {
            if (!string.IsNullOrWhiteSpace(NewProfileLastName) && !string.IsNullOrWhiteSpace(NewProfilePassword))
            {
                var newProfile = new Profile
                {
                    LastName = NewProfileLastName,
                    FirstName = NewProfileFirstName,
                    MiddleName = NewProfileMiddleName,
                    Role = NewProfileRole,
                    PasswordHash = _passwordService.HashPassword(NewProfilePassword)
                };
                await _profileService.AddProfileAsync(newProfile);
                NewProfileLastName = string.Empty;
                NewProfileFirstName = string.Empty;
                NewProfileMiddleName = string.Empty;
                NewProfilePassword = string.Empty;
                LoadProfiles();
            }
        }

        private async void SaveProfile()
        {
            if (SelectedProfile != null)
            {
                await _profileService.UpdateProfileAsync(SelectedProfile);
            }
        }

        private async void DeleteProfile()
        {
            if (SelectedProfile != null)
            {
                await _profileService.DeleteProfileAsync(SelectedProfile.Id);
                LoadProfiles();
            }
        }

                private async void Import()
        {
            var openFileDialog = new OpenFileDialog { Filter = "Excel Files|*.xlsx" };
            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var sheetNames = GetSheetNames(filePath);
                var dialogParameters = new DialogParameters { { "sheetNames", sheetNames } };

                _dialogService.ShowDialog("SelectSheetDialog", dialogParameters, async (result) =>
                {
                    if (result.Result == ButtonResult.OK)
                    {
                        var selectedSheet = result.Parameters.GetValue<string>("selectedSheet");
                        var createRelationships = result.Parameters.GetValue<bool>("createRelationships");

                        IsImporting = true;
                        ImportStatus = "Импорт...";
                        var progress = new Progress<Tuple<double, string>>(p =>
                        {
                            ImportProgress = p.Item1;
                            ImportStatus = p.Item2;
                        });
                        await _importService.ImportFromExcelAsync(filePath, selectedSheet, progress, createRelationships);
                        IsImporting = false;
                        ImportStatus = "Импорт завершен.";
                    }
                });
            }
        }

        private List<string> GetSheetNames(string filePath)
        {
            var sheetNames = new List<string>();
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    sheetNames.Add(worksheet.Name);
                }
            }
            return sheetNames;
        }

        private async void Export()
        {
            var saveFileDialog = new SaveFileDialog { Filter = "Excel Files|*.xlsx" };
            if (saveFileDialog.ShowDialog() == true)
            {
                await _exportService.ExportToExcelAsync(saveFileDialog.FileName);
            }
        }

        private void ChangeDatabasePath()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "SQLite Database (*.db)|*.db",
                FileName = System.IO.Path.GetFileName(DatabasePath)
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                DatabasePath = saveFileDialog.FileName;
                var settings = _settingsService.LoadSettings();
                settings.DatabasePath = DatabasePath;
                settings.DefaultCompanyCode = DefaultCompanyCode;
                settings.RunInTray = RunInTray;
                _settingsService.SaveSettingsAsync(settings);
            }
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
            var settings = _settingsService.LoadSettings();
            settings.DatabasePath = DatabasePath;
            settings.DefaultCompanyCode = DefaultCompanyCode;
            settings.RunInTray = RunInTray;
            _settingsService.SaveSettingsAsync(settings);
        }

        public void OnDialogOpened(IDialogParameters parameters) { }

        private void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "ok")
            {
                var settings = _settingsService.LoadSettings();
                settings.DatabasePath = DatabasePath;
                settings.DefaultCompanyCode = DefaultCompanyCode;
                settings.RunInTray = RunInTray;
                _settingsService.SaveSettingsAsync(settings);
                result = ButtonResult.OK;
            }
            else if (parameter?.ToLower() == "cancel")
            {
                result = ButtonResult.Cancel;
            }

            RequestClose?.Invoke(new DialogResult(result));
        }
    }
}