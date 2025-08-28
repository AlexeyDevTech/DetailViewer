using DetailViewer.Core.Events;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Printing;
using System.Threading.Tasks;

namespace DetailViewer.ViewModels
{
    /// <summary>
    /// ViewModel для главного окна приложения (MainWindow).
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IDialogService _dialogService;
        private readonly IActiveUserService _activeUserService;
        private readonly IClassifierService _classifierService;
        private readonly IEventAggregator _ea;
        private string _title = "Detail Viewer";

        /// <summary>
        /// Заголовок главного окна.
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _activeUserFullName;
        /// <summary>
        /// Полное имя текущего активного пользователя.
        /// </summary>
        public string ActiveUserFullName
        {
            get { return _activeUserFullName; }
            set { SetProperty(ref _activeUserFullName, value); }
        }

        /// <summary>
        /// Команда для навигации по регионам приложения.
        /// </summary>
        public DelegateCommand<string> NavigateCommand { get; private set; }

        /// <summary>
        /// Команда для отображения окна настроек.
        /// </summary>
        public DelegateCommand ShowSettingsCommand { get; private set; }

        /// <summary>
        /// Команда для отображения окна "О программе".
        /// </summary>
        public DelegateCommand ShowAboutCommand { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MainWindowViewModel"/>.
        /// </summary>
        public MainWindowViewModel(IRegionManager regionManager, IEventAggregator ea, IDialogService dialogService, IActiveUserService activeUserService, IClassifierService classifierService)
        {
            _regionManager = regionManager;
            _dialogService = dialogService;
            _activeUserService = activeUserService;
            _classifierService = classifierService;
            _ea = ea;

            _ea.GetEvent<UserChangedEvent>().Subscribe(OnCurrentUserChanged);
            NavigateCommand = new DelegateCommand<string>(Navigate);
            ShowSettingsCommand = new DelegateCommand(ShowSettings);
            ShowAboutCommand = new DelegateCommand(ShowAbout);

            InitializeApplication();
        }

        /// <summary>
        /// Инициализирует данные приложения, например, загружает классификаторы.
        /// </summary>
        private async void InitializeApplication()
        {
            await _classifierService.LoadClassifiersAsync();
        }

        /// <summary>
        /// Обработчик события смены текущего пользователя.
        /// </summary>
        /// <param name="profile">Профиль нового пользователя.</param>
        private void OnCurrentUserChanged(Profile profile)
        {
            ActiveUserFullName = profile?.FullName;
        }

        /// <summary>
        /// Выполняет навигацию к указанному пути.
        /// </summary>
        /// <param name="navigationPath">Путь для навигации.</param>
        private void Navigate(string navigationPath)
        {
            _regionManager.RequestNavigate("ContentRegion", navigationPath);
        }

        /// <summary>
        /// Открывает диалоговое окно настроек.
        /// </summary>
        private void ShowSettings()
        {
            _dialogService.ShowDialog("SettingsView", new DialogParameters(), r =>
            {
                // Handle dialog result if needed
            });
        }

        /// <summary>
        /// Открывает диалоговое окно "О программе".
        /// </summary>
        private void ShowAbout()
        {
            _dialogService.ShowDialog("AboutView", new DialogParameters(), r =>
            {
                // Handle dialog result if needed
            });
        }
    }
}
