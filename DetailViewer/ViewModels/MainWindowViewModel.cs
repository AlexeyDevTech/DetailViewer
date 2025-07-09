using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs; // Added

namespace DetailViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IDialogService _dialogService; // Added
        private string _title = "Detail Viewer";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public DelegateCommand<string> NavigateCommand { get; private set; }
        public DelegateCommand ShowSettingsCommand { get; private set; } // Added

        public MainWindowViewModel(IRegionManager regionManager, IDialogService dialogService) // Modified constructor
        {
            _regionManager = regionManager;
            _dialogService = dialogService; // Added
            NavigateCommand = new DelegateCommand<string>(Navigate);
            ShowSettingsCommand = new DelegateCommand(ShowSettings); // Added
        }

        private void Navigate(string navigationPath)
        {
            _regionManager.RequestNavigate("ContentRegion", navigationPath);
        }

        private void ShowSettings() // Added
        {
            _dialogService.ShowDialog("SettingsView", new DialogParameters(), r =>
            {
                // Handle dialog result if needed
            });
        }
    }
}
