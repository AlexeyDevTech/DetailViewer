using DetailViewer.Core.Events;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Events;
using System;

namespace DetailViewer.Core.Services
{
    public class ActiveUserService : IActiveUserService
    {
        UserChangedEvent uce;
        public ActiveUserService(IEventAggregator ea)
        {
           uce = ea.GetEvent<UserChangedEvent>();
        }
        private static Profile _currentUser;
        public Profile CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                uce.Publish(value);
                CurrentUserChanged?.Invoke();
            }
        }

        public event Action CurrentUserChanged;
    }
}