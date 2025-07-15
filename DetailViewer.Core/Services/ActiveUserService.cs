using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System;

namespace DetailViewer.Core.Services
{
    public class ActiveUserService : IActiveUserService
    {
        private Profile _currentUser;
        public Profile CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                CurrentUserChanged?.Invoke();
            }
        }

        public event Action CurrentUserChanged;
    }
}
