using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace DetailViewer.Modules.Dialogs.ViewModels
{
    public class RegistrationViewModel : BindableBase, IDialogAware
    {
        private readonly IProfileService _profileService;
        private readonly IPasswordService _passwordService;

        public string Title => "Регистрация";

        public event Action<IDialogResult>? RequestClose;

        private string _lastName = string.Empty;
        public string LastName
        {
            get { return _lastName; }
            set { SetProperty(ref _lastName, value); }
        }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get { return _firstName; }
            set { SetProperty(ref _firstName, value); }
        }

        private string _middleName = string.Empty;
        public string MiddleName
        {
            get { return _middleName; }
            set { SetProperty(ref _middleName, value); }
        }

        private string _password = string.Empty;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        public DelegateCommand RegisterCommand { get; }

        public RegistrationViewModel(IProfileService profileService, IPasswordService passwordService)
        {
            _profileService = profileService;
            _passwordService = passwordService;
            RegisterCommand = new DelegateCommand(OnRegister);
        }

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

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) { }
    }
}
