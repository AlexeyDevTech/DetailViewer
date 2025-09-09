using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    /// <summary>
    /// ViewModel для диалогового окна авторизации и регистрации.
    /// </summary>
    public class AuthorizationViewModel : BindableBase, IDialogAware
    {
        private readonly IProfileService _profileService;
        private readonly IPasswordService _passwordService;

        /// <summary>
        /// Заголовок диалогового окна.
        /// </summary>
        public string Title => "Вход и Регистрация";

        /// <summary>
        /// Событие, запрашивающее закрытие диалогового окна.
        /// </summary>
        public event Action<IDialogResult>? RequestClose;

        private int _selectedTabIndex;
        /// <summary>
        /// Индекс выбранной вкладки (0 - Вход, 1 - Регистрация).
        /// </summary>
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { SetProperty(ref _selectedTabIndex, value); }
        }

        private string? _statusMessage;
        /// <summary>
        /// Сообщение о статусе операции (успех/ошибка).
        /// </summary>
        public string? StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }

        private bool _isError;
        /// <summary>
        /// Флаг, указывающий, является ли текущее сообщение об ошибке.
        /// </summary>
        public bool IsError
        {
            get { return _isError; }
            set { SetProperty(ref _isError, value); }
        }

        // Login properties
        private List<ProfileDto> _profiles = new List<ProfileDto>();
        /// <summary>
        /// Список доступных профилей для входа.
        /// </summary>
        public List<ProfileDto> Profiles
        {
            get { return _profiles; }
            set { SetProperty(ref _profiles, value); }
        }

        private ProfileDto? _selectedProfile;
        /// <summary>
        /// Выбранный профиль для входа.
        /// </summary>
        public ProfileDto? SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                SetProperty(ref _selectedProfile, value);
                StatusMessage = null;
            }
        }

        private string _password = string.Empty;
        /// <summary>
        /// Пароль для входа.
        /// </summary>
        public string Password
        {
            get { return _password; }
            set
            {
                SetProperty(ref _password, value);
                StatusMessage = null;
            }
        }

        // Registration properties
        private string _newLastName = string.Empty;
        /// <summary>
        /// Фамилия для нового профиля.
        /// </summary>
        public string NewLastName
        {
            get { return _newLastName; }
            set { SetProperty(ref _newLastName, value); }
        }

        private string _newFirstName = string.Empty;
        /// <summary>
        /// Имя для нового профиля.
        /// </summary>
        public string NewFirstName
        {
            get { return _newFirstName; }
            set { SetProperty(ref _newFirstName, value); }
        }

        private string _newMiddleName = string.Empty;
        /// <summary>
        /// Отчество для нового профиля.
        /// </summary>
        public string NewMiddleName
        {
            get { return _newMiddleName; }
            set { SetProperty(ref _newMiddleName, value); }
        }

        private string _newPassword = string.Empty;
        /// <summary>
        /// Пароль для нового профиля.
        /// </summary>
        public string NewPassword
        {
            get { return _newPassword; }
            set { SetProperty(ref _newPassword, value); }
        }

        /// <summary>
        /// Команда для авторизации пользователя.
        /// </summary>
        public DelegateCommand AuthorizeCommand { get; }

        /// <summary>
        /// Команда для регистрации нового пользователя.
        /// </summary>
        public DelegateCommand RegisterCommand { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AuthorizationViewModel"/>.
        /// </summary>
        /// <param name="profileService">Сервис для работы с профилями пользователей.</param>
        /// <param name="passwordService">Сервис для хеширования и проверки паролей.</param>
        public AuthorizationViewModel(IProfileService profileService, IPasswordService passwordService)
        {
            _profileService = profileService;
            _passwordService = passwordService;
            AuthorizeCommand = new DelegateCommand(async () => await OnAuthorize());
            RegisterCommand = new DelegateCommand(OnRegister);
            LoadProfiles();
        }

        /// <summary>
        /// Асинхронно загружает список профилей пользователей.
        /// </summary>
        private async void LoadProfiles()
        {
            Profiles = await _profileService.GetAllProfilesAsync();
        }

        /// <summary>
        /// Выполняет авторизацию пользователя.
        /// </summary>
        private async Task OnAuthorize()
        {
            if (SelectedProfile != null)
            {
                var userProfile = await _profileService.GetProfileByIdAsync(SelectedProfile.Id);
                if (userProfile != null && _passwordService.VerifyPassword(Password, userProfile.PasswordHash))
                {
                    var parameters = new DialogParameters { { "user", userProfile } };
                    RequestClose?.Invoke(new DialogResult(ButtonResult.OK, parameters));
                }
                else
                {
                    StatusMessage = "Неверный логин или пароль";
                    IsError = true;
                }
            }
        }

        /// <summary>
        /// Выполняет регистрацию нового пользователя.
        /// </summary>
        private async void OnRegister()
        {
            try
            {
                var newProfile = new Profile
                {
                    LastName = NewLastName,
                    FirstName = NewFirstName,
                    MiddleName = NewMiddleName,
                    PasswordHash = _passwordService.HashPassword(NewPassword),
                    Role = Role.Operator
                };

                await _profileService.AddProfileAsync(newProfile);
                LoadProfiles();
                StatusMessage = "Регистрация прошла успешно! Теперь вы можете войти.";
                IsError = false;
                SelectedTabIndex = 0; // Switch to login tab
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка регистрации: {ex.Message}";
                IsError = true;
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
        public void OnDialogClosed() { }

        /// <summary>
        /// Вызывается при открытии диалогового окна.
        /// </summary>
        /// <param name="parameters">Параметры диалогового окна.</param>
        public void OnDialogOpened(IDialogParameters parameters) { }
    }
}
