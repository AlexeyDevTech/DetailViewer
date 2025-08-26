using DetailViewer.Core.Models;
using System;

namespace DetailViewer.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для сервиса, управляющего текущим активным пользователем в системе.
    /// </summary>
    public interface IActiveUserService
    {
        /// <summary>
        /// Получает или задает текущего пользователя системы.
        /// </summary>
        Profile CurrentUser { get; set; }

        /// <summary>
        /// Событие, возникающее при смене текущего пользователя.
        /// </summary>
        event Action CurrentUserChanged;
    }
}