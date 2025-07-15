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

        public string Title => "Авторизация";

        public event Action<IDialogResult> RequestClose;

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

        public DelegateCommand AuthorizeCommand { get; }

        public AuthorizationViewModel(IProfileService profileService)
        {
            _profileService = profileService;
            AuthorizeCommand = new DelegateCommand(OnAuthorize);
            LoadProfiles();
        }

        private async void LoadProfiles()
        {
            Profiles = await _profileService.GetAllProfilesAsync();
        }

        private void OnAuthorize()
        {
            // TODO: Implement actual password validation here
            if (SelectedProfile != null)
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            }
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) { }
    }
}