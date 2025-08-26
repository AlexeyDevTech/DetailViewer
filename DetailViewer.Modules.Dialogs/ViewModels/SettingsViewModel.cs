#nullable enable

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
    /// <summary>
    /// ViewModel для окна настроек приложения.
    /// </summary>
    public class SettingsViewModel : BindableBase, IDialogAware
    {
        private readonly IProfileService _profileService;
        private readonly ISettingsService _settingsService;
        private readonly IActiveUserService _activeUserService;
        private readonly IPasswordService _passwordService;
        private readonly IExcelExportService _exportService;
        private string _defaultCompanyCode;

        /// <summary>
        /// Код компании по умолчанию.
        /// </summary>
        public string DefaultCompanyCode
        {
            get { return _defaultCompanyCode; }
            set { SetProperty(ref _defaultCompanyCode, value); }
        }
        private ObservableCollection<Profile> _profiles;
        private Profile? _selectedProfile;
        private string _newProfileLastName;
        private string _newProfileFirstName;
        private string _newProfileMiddleName;
        private string _newProfilePassword;
        private double _importProgress;
        private bool _isImporting;
        private string _importStatus;
        private bool _runInTray;

        /// <summary>
        /// Флаг, указывающий, запускать ли приложение в трее.
        /// </summary>
        public bool RunInTray
        {
            get { return _runInTray; }
            set { SetProperty(ref _runInTray, value); }
        }

        /// <summary>
        /// Коллекция профилей пользователей.
        /// </summary>
        public ObservableCollection<Profile> Profiles
        {
            get { return _profiles; }
            set { SetProperty(ref _profiles, value); }
        }

        /// <summary>
        /// Выбранный профиль пользователя.
        /// </summary>
        public Profile? SelectedProfile
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

        /// <summary>
        /// Фамилия для нового профиля.
        /// </summary>
        public string NewProfileLastName
        {
            get { return _newProfileLastName; }
            set { SetProperty(ref _newProfileLastName, value); }
        }

        /// <summary>
        /// Имя для нового профиля.
        /// </summary>
        public string NewProfileFirstName
        {
            get { return _newProfileFirstName; }
            set { SetProperty(ref _newProfileFirstName, value); }
        }

        /// <summary>
        /// Отчество для нового профиля.
        /// </summary>
        public string NewProfileMiddleName
        {
            get { return _newProfileMiddleName; }
            set { SetProperty(ref _newProfileMiddleName, value); }
        }

        /// <summary>
        /// Пароль для нового профиля.
        /// </summary>
        public string NewProfilePassword
        {
            get { return _newProfilePassword; }
            set { SetProperty(ref _newProfilePassword, value); }
        }

        /// <summary>
        /// Прогресс импорта данных (от 0 до 100).
        /// </summary>
        public double ImportProgress
        {
            get { return _importProgress; }
            set { SetProperty(ref _importProgress, value); }
        }

        /// <summary>
        /// Флаг, указывающий, идет ли процесс импорта.
        /// </summary>
        public bool IsImporting
        {
            get { return _isImporting; }
            set { SetProperty(ref _isImporting, value); }
        }

        /// <summary>
        /// Статус импорта данных.
        /// </summary>
        public string ImportStatus
        {
            get { return _importStatus; }
            set { SetProperty(ref _importStatus, value); }
        }

        /// <summary>
        /// Флаг, указывающий, является ли текущий пользователь администратором или модератором.
        /// </summary>
        public bool IsAdminOrModerator { get; private set; }

        /// <summary>
        /// Коллекция доступных ролей пользователей.
        /// </summary>
        public IEnumerable<Role> Roles { get; private set; }
        private Role _newProfileRole;

        /// <summary>
        /// Выбранная роль для нового профиля.
        /// </summary>
        public Role NewProfileRole
        {
            get => _newProfileRole;
            set => SetProperty(ref _newProfileRole, value);
        }

        /// <summary>
        /// Команда для запуска импорта данных.
        /// </summary>
        public DelegateCommand ImportCommand { get; private set; }

        /// <summary>
        /// Команда для запуска экспорта данных.
        /// </summary>
        public DelegateCommand ExportCommand { get; private set; }

        /// <summary>
        /// Команда для добавления нового профиля пользователя.
        /// </summary>
        public DelegateCommand AddProfileCommand { get; private set; }

        /// <summary>
        /// Команда для сохранения изменений в выбранном профиле.
        /// </summary>
        public DelegateCommand SaveProfileCommand { get; private set; }

        /// <summary>
        /// Команда для удаления выбранного профиля.
        /// </summary>
        public DelegateCommand DeleteProfileCommand { get; private set; }

        /// <summary>
        /// Команда для закрытия диалогового окна настроек.
        /// </summary>
        public DelegateCommand<string> CloseDialogCommand { get; private set; }

        /// <summary>
        /// Заголовок диалогового окна.
        /// </summary>
        public string Title => "Настройки";

        /// <summary>
        /// Событие, запрашивающее закрытие диалогового окна.
        /// </summary>
        public event Action<IDialogResult> RequestClose;

        private readonly IDialogService _dialogService;
        private readonly IExcelImportService _importService;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SettingsViewModel"/>.
        /// </summary>
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

            var settings = _settingsService.LoadSettings();
            DefaultCompanyCode = settings.DefaultCompanyCode;
            RunInTray = settings.RunInTray;

            ImportCommand = new DelegateCommand(Import);
            ExportCommand = new DelegateCommand(Export);
            AddProfileCommand = new DelegateCommand(AddProfile);
            SaveProfileCommand = new DelegateCommand(SaveProfile, () => SelectedProfile != null).ObservesProperty(() => SelectedProfile);
            DeleteProfileCommand = new DelegateCommand(DeleteProfile, () => SelectedProfile != null).ObservesProperty(() => SelectedProfile);
            CloseDialogCommand = new DelegateCommand<string>(CloseDialog);

            IsAdminOrModerator = _activeUserService.CurrentUser?.Role == Role.Admin || _activeUserService.CurrentUser?.Role == Role.Moderator;
            RaisePropertyChanged(nameof(IsAdminOrModerator));

            Roles = Enum.GetValues(typeof(Role)).Cast<Role>();
            NewProfileRole = Role.Operator;

            LoadProfiles();
        }

        /// <summary>
        /// Асинхронно загружает список профилей пользователей.
        /// </summary>
        private async void LoadProfiles()
        {
            var profiles = await _profileService.GetAllProfilesAsync();
            Profiles = new ObservableCollection<Profile>(profiles);
            SelectedProfile = Profiles.FirstOrDefault(p => p.Id == _activeUserService.CurrentUser?.Id) ?? Profiles.FirstOrDefault();
        }

        /// <summary>
        /// Добавляет новый профиль пользователя.
        /// </summary>
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

        /// <summary>
        /// Сохраняет изменения в выбранном профиле пользователя.
        /// </summary>
        private async void SaveProfile()
        {
            if (SelectedProfile != null)
            {
                await _profileService.UpdateProfileAsync(SelectedProfile);
            }
        }

        /// <summary>
        /// Удаляет выбранный профиль пользователя.
        /// </summary>
        private async void DeleteProfile()
        {
            if (SelectedProfile != null)
            {
                await _profileService.DeleteProfileAsync(SelectedProfile.Id);
                LoadProfiles();
            }
        }

        /// <summary>
        /// Запускает процесс импорта данных из Excel-файла.
        /// </summary>
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
                        //await _importService.ImportFromExcelAsync(filePath, selectedSheet, progress, createRelationships);
                        IsImporting = false;
                        ImportStatus = "Импорт завершен.";
                    }
                });
            }
        }

        /// <summary>
        /// Получает названия листов из указанного Excel-файла.
        /// </summary>
        /// <param name="filePath">Путь к Excel-файлу.</param>
        /// <returns>Список названий листов.</returns>
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

        /// <summary>
        /// Запускает процесс экспорта данных.
        /// </summary>
        private async void Export()
        {
            var saveFileDialog = new SaveFileDialog { Filter = "Excel Files|*.xlsx" };
            if (saveFileDialog.ShowDialog() == true)
            {
                //await _exportService.ExportToExcelAsync(saveFileDialog.FileName, null);
            }
        }

        /// <summary>
        /// Определяет, можно ли закрыть диалоговое окно.
        /// </summary>
        /// <returns>Всегда true.</returns>
        public bool CanCloseDialog() => true;

        /// <summary>
        /// Вызывается после закрытия диалогового окна.
        /// </summary>
        public void OnDialogClosed()
        {
            var settings = _settingsService.LoadSettings();
            settings.DefaultCompanyCode = DefaultCompanyCode;
            settings.RunInTray = RunInTray;
            _settingsService.SaveSettingsAsync(settings);
        }

        /// <summary>
        /// Вызывается при открытии диалогового окна.
        /// </summary>
        /// <param name="parameters">Параметры диалогового окна.</param>
        public void OnDialogOpened(IDialogParameters parameters) { }

        /// <summary>
        /// Закрывает диалоговое окно настроек.
        /// </summary>
        /// <param name="parameter">Параметр, определяющий результат закрытия (например, "ok" или "cancel").</param>
        private void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "ok")
            {
                var settings = _settingsService.LoadSettings();
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
