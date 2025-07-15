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
    public class AuthorizationViewModel : BindableBase, IDialogAware
    {
        private readonly IProfileService _profileService;
        private readonly IPasswordService _passwordService;

        public string Title => "Вход и Регистрация";

        public event Action<IDialogResult> RequestClose;

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { SetProperty(ref _selectedTabIndex, value); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }

        // Login properties
        private List<Profile> _profiles;
        public List<Profile> Profiles
        {
            get { return _profiles; }
            set { SetProperty(ref _profiles, value); }
        }

        private Profile _selectedProfile;
        public Profile SelectedProfile
        {
            get { return _selectedProfile; }
            set { SetProperty(ref _selectedProfile, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        // Registration properties
        private string _newLastName;
        public string NewLastName
        {
            get { return _newLastName; }
            set { SetProperty(ref _newLastName, value); }
        }

        private string _newFirstName;
        public string NewFirstName
        {
            get { return _newFirstName; }
            set { SetProperty(ref _newFirstName, value); }
        }

        private string _newMiddleName;
        public string NewMiddleName
        {
            get { return _newMiddleName; }
            set { SetProperty(ref _newMiddleName, value); }
        }

        private string _newPassword;
        public string NewPassword
        {
            get { return _newPassword; }
            set { SetProperty(ref _newPassword, value); }
        }

        public DelegateCommand AuthorizeCommand { get; }
        public DelegateCommand RegisterCommand { get; }

        public AuthorizationViewModel(IProfileService profileService, IPasswordService passwordService)
        {
            _profileService = profileService;
            _passwordService = passwordService;
            AuthorizeCommand = new DelegateCommand(OnAuthorize);
            RegisterCommand = new DelegateCommand(OnRegister);
            LoadProfiles();
        }

        private async void LoadProfiles()
        {
            Profiles = await _profileService.GetAllProfilesAsync();
        }

        private void OnAuthorize()
        {
            if (SelectedProfile != null && _passwordService.VerifyPassword(Password, SelectedProfile.PasswordHash))
            {
                var parameters = new DialogParameters { { "user", SelectedProfile } };
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK, parameters));
            }
            else
            {
                StatusMessage = "Неверный логин или пароль";
            }
        }

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
                SelectedTabIndex = 0; // Switch to login tab
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка регистрации: {ex.Message}";
            }
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) { }
    }
}