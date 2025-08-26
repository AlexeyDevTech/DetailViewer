using DetailViewer.Core.Models;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для сервиса, управляющего настройками приложения.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Загружает настройки приложения из источника данных.
        /// </summary>
        /// <returns>Объект с настройками приложения.</returns>
        AppSettings LoadSettings();

        /// <summary>
        /// Асинхронно сохраняет настройки приложения.
        /// </summary>
        /// <param name="settings">Объект с настройками для сохранения.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task SaveSettingsAsync(AppSettings settings);
    }
}