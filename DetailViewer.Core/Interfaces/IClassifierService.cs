using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для сервиса, управляющего классификаторами ЕСКД.
    /// </summary>
    public interface IClassifierService
    {
        /// <summary>
        /// Асинхронно загружает все классификаторы из источника данных.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task LoadClassifiersAsync();

        /// <summary>
        /// Возвращает все загруженные классификаторы.
        /// </summary>
        /// <returns>Коллекция всех классификаторов.</returns>
        IEnumerable<Classifier> GetAllClassifiers();

        /// <summary>
        /// Получает классификатор по его числовому коду.
        /// </summary>
        /// <param name="number">6-значный код классификатора.</param>
        /// <returns>Найденный классификатор или null, если не найден.</returns>
        Classifier? GetClassifierByNumber(int number);
    }
}
