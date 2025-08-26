using DetailViewer.Core.Events;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Events;
using System;

namespace DetailViewer.Infrastructure.Services
{
    /// <summary>
    /// Реализация сервиса для управления текущим активным пользователем.
    /// </summary>
    public class ActiveUserService : IActiveUserService
    {
        private readonly UserChangedEvent _userChangedEvent;
        private static Profile _currentUser;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ActiveUserService"/>.
        /// </summary>
        /// <param name="eventAggregator">Агрегатор событий для публикации изменений.</param>
        public ActiveUserService(IEventAggregator eventAggregator)
        {
           _userChangedEvent = eventAggregator.GetEvent<UserChangedEvent>();
        }

        /// <inheritdoc/>
        public Profile CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    _userChangedEvent.Publish(value);
                    CurrentUserChanged?.Invoke();
                }
            }
        }

        /// <inheritdoc/>
        public event Action CurrentUserChanged;
    }
}
