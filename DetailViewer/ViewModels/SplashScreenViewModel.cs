using Prism.Events;
using Prism.Mvvm;
using DetailViewer.Core.Events;

namespace DetailViewer.ViewModels
{
    /// <summary>
    /// ViewModel для экрана-заставки (SplashScreen).
    /// </summary>
    public class SplashScreenViewModel : BindableBase
    {
        private string _statusText = "Инициализация...";
        /// <summary>
        /// Текст статуса, отображаемый на экране-заставке.
        /// </summary>
        public string StatusText
        {
            get { return _statusText; }
            set { SetProperty(ref _statusText, value); }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SplashScreenViewModel"/>.
        /// </summary>
        /// <param name="eventAggregator">Агрегатор событий для подписки на обновления статуса.</param>
        public SplashScreenViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<StatusUpdateEvent>().Subscribe(UpdateStatus);
        }

        /// <summary>
        /// Обновляет текст статуса на экране-заставке.
        /// </summary>
        /// <param name="status">Новый текст статуса.</param>
        private void UpdateStatus(string status)
        {
            StatusText = status;
        }
    }
}