using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class SettingsViewModel : BindableBase, IDialogAware
    {
        private readonly IDocumentDataService _documentDataService;
        private readonly IProfileService _profileService;
        private readonly ISettingsService _settingsService;
        private string _databasePath;
        private ObservableCollection<Profile> _profiles;
        private Profile _selectedProfile;
        private string _newProfileLastName;
        private string _newProfileFirstName;
        private string _newProfileMiddleName;
        private double _importProgress;
        private bool _isImporting;
        private string _importStatus;

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
                    var settings = _settingsService.LoadSettings();
                    settings.ActiveProfileId = value.Id;
                    _settingsService.SaveSettingsAsync(settings);
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

        public DelegateCommand ImportCommand { get; private set; }
        public DelegateCommand ExportCommand { get; private set; }
        public DelegateCommand AddProfileCommand { get; private set; }
        public DelegateCommand SaveProfileCommand { get; private set; }
        public DelegateCommand DeleteProfileCommand { get; private set; }

        public string Title => "Настройки";

        public event Action<IDialogResult> RequestClose;

        public SettingsViewModel(IDocumentDataService documentDataService, IProfileService profileService, ISettingsService settingsService)
        {
            _documentDataService = documentDataService;
            _profileService = profileService;
            _settingsService = settingsService;
            DatabasePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "detailviewer.db");

            ImportCommand = new DelegateCommand(Import);
            ExportCommand = new DelegateCommand(Export);
            AddProfileCommand = new DelegateCommand(AddProfile);
            SaveProfileCommand = new DelegateCommand(SaveProfile, () => SelectedProfile != null).ObservesProperty(() => SelectedProfile);
            DeleteProfileCommand = new DelegateCommand(DeleteProfile, () => SelectedProfile != null).ObservesProperty(() => SelectedProfile);

            LoadProfiles();
        }

        private async void LoadProfiles()
        {
            var settings = _settingsService.LoadSettings();
            var profiles = await _profileService.GetAllProfilesAsync();
            Profiles = new ObservableCollection<Profile>(profiles);
            SelectedProfile = Profiles.FirstOrDefault(p => p.Id == settings.ActiveProfileId) ?? Profiles.FirstOrDefault();
        }

        private async void AddProfile()
        {
            if (!string.IsNullOrWhiteSpace(NewProfileLastName) || !string.IsNullOrWhiteSpace(NewProfileFirstName) || !string.IsNullOrWhiteSpace(NewProfileMiddleName))
            {
                var newProfile = new Profile
                {
                    LastName = NewProfileLastName,
                    FirstName = NewProfileFirstName,
                    MiddleName = NewProfileMiddleName
                };
                await _profileService.AddProfileAsync(newProfile);
                NewProfileLastName = string.Empty;
                NewProfileFirstName = string.Empty;
                NewProfileMiddleName = string.Empty;
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
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                IsImporting = true;
                ImportStatus = "Импорт...";
                var progress = new Progress<double>(p => ImportProgress = p);
                await _documentDataService.ImportFromExcelAsync(openFileDialog.FileName, progress);
                IsImporting = false;
                ImportStatus = "Импорт завершен.";
            }
        }

        private async void Export()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await _documentDataService.ExportToExcelAsync(saveFileDialog.FileName);
            }
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            // Clean up resources if necessary
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            // Handle parameters passed to the dialog if necessary
        }
    }
}