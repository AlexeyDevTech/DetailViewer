using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    /// <summary>
    /// ViewModel для формы регистрации нового пользователя.
    /// </summary>
    public class RegistrationViewModel : BindableBase, IDialogAware
    {
        private readonly IProfileService _profileService;
        private readonly IPasswordService _passwordService;

        /// <summary>
        /// Заголовок диалогового окна.
        /// </summary>
        public string Title => "Регистрация";

        /// <summary>
        /// Событие, запрашивающее закрытие диалогового окна.
        /// </summary>
        public event Action<IDialogResult>? RequestClose;

        private string _lastName = string.Empty;
        /// <summary>
        /// Фамилия нового пользователя.
        /// </summary>
        public string LastName
        {
            get { return _lastName; }
            set { SetProperty(ref _lastName, value); }
        }

        private string _firstName = string.Empty;
        /// <summary>
        /// Имя нового пользователя.
        /// </summary>
        public string FirstName
        {
            get { return _firstName; }
            set { SetProperty(ref _firstName, value); }
        }

        private string _middleName = string.Empty;
        /// <summary>
        /// Отчество нового пользователя.
        /// </summary>
        public string MiddleName
        {
            get { return _middleName; }
            set { SetProperty(ref _middleName, value); }
        }

        private string _password = string.Empty;
        /// <summary>
        /// Пароль нового пользователя.
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        /// <summary>
        /// Команда для регистрации нового пользователя.
        /// </summary>
        public DelegateCommand RegisterCommand { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RegistrationViewModel"/>.
        /// </summary>
        /// <param name="profileService">Сервис для работы с профилями пользователей.</param>
        /// <param name="passwordService">Сервис для хеширования и проверки паролей.</param>
        public RegistrationViewModel(IProfileService profileService, IPasswordService passwordService)
        {
            _profileService = profileService;
            _passwordService = passwordService;
            RegisterCommand = new DelegateCommand(OnRegister);
        }

        /// <summary>
        /// Выполняет регистрацию нового пользователя.
        /// </summary>
        private async void OnRegister()
        {
            var newProfile = new Profile
            {
                LastName = LastName,
                FirstName = FirstName,
                MiddleName = MiddleName,
                PasswordHash = _passwordService.HashPassword(Password),
                Role = Role.Operator
            };

            await _profileService.AddProfileAsync(newProfile);

            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
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