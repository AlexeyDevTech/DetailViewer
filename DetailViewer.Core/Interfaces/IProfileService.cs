using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для сервиса, управляющего профилями пользователей.
    /// </summary>
    public interface IProfileService
    {
        /// <summary>
        /// Асинхронно получает все профили пользователей.
        /// </summary>
        /// <returns>Список всех профилей.</returns>
        Task<List<Profile>> GetAllProfilesAsync();

        /// <summary>
        /// Асинхронно получает профиль по его ID.
        /// </summary>
        /// <param name="id">Идентификатор профиля.</param>
        /// <returns>Профиль.</returns>
        Task<Profile> GetProfileByIdAsync(int id);

        /// <summary>
        /// Асинхронно добавляет новый профиль пользователя.
        /// </summary>
        /// <param name="profile">Новый профиль.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task AddProfileAsync(Profile profile);

        /// <summary>
        /// Асинхронно обновляет существующий профиль пользователя.
        /// </summary>
        /// <param name="profile">Профиль с обновленными данными.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task UpdateProfileAsync(Profile profile);

        /// <summary>
        /// Асинхронно удаляет профиль по его ID.
        /// </summary>
        /// <param name="profileId">Идентификатор профиля для удаления.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task DeleteProfileAsync(int profileId);
    }
}
