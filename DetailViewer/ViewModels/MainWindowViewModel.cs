using DetailViewer.Core.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Threading.Tasks;

namespace DetailViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IDialogService _dialogService;
        private readonly IActiveUserService _activeUserService;
        private readonly IClassifierService _classifierService;

        private string _title = "Detail Viewer";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _activeUserFullName;
        public string ActiveUserFullName
        {
            get { return _activeUserFullName; }
            set { SetProperty(ref _activeUserFullName, value); }
        }

        public DelegateCommand<string> NavigateCommand { get; private set; }
        public DelegateCommand ShowSettingsCommand { get; private set; }
        public DelegateCommand ShowAboutCommand { get; private set; }

        public MainWindowViewModel(IRegionManager regionManager, IDialogService dialogService, IActiveUserService activeUserService, IClassifierService classifierService)
        {
            _regionManager = regionManager;
            _dialogService = dialogService;
            _activeUserService = activeUserService;
            _classifierService = classifierService;

            _activeUserService.CurrentUserChanged += OnCurrentUserChanged;

            NavigateCommand = new DelegateCommand<string>(Navigate);
            ShowSettingsCommand = new DelegateCommand(ShowSettings);
            ShowAboutCommand = new DelegateCommand(ShowAbout);

            InitializeApplication();
        }

        private async void InitializeApplication()
        {
            await _classifierService.LoadClassifiersAsync();
        }

        private void OnCurrentUserChanged()
        {
            ActiveUserFullName = _activeUserService.CurrentUser?.FullName;
        }

        private void Navigate(string navigationPath)
        {
            _regionManager.RequestNavigate("ContentRegion", navigationPath);
        }

        private void ShowSettings()
        {
            _dialogService.ShowDialog("SettingsView", new DialogParameters(), r =>
            {
                // Handle dialog result if needed
            });
        }

        private void ShowAbout()
        {
            _dialogService.ShowDialog("AboutView", new DialogParameters(), r =>
            {
                // Handle dialog result if needed
            });
        }
    }
}