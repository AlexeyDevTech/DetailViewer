using Prism.Events;
using Prism.Mvvm;
using DetailViewer.Core.Events;

namespace DetailViewer.ViewModels
{
    public class SplashScreenViewModel : BindableBase
    {
        private string _statusText = "Инициализация...";
        public string StatusText
        {
            get { return _statusText; }
            set { SetProperty(ref _statusText, value); }
        }

        public SplashScreenViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<StatusUpdateEvent>().Subscribe(UpdateStatus);
        }

        private void UpdateStatus(string status)
        {
            StatusText = status;
        }
    }
}
