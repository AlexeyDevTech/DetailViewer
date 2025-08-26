using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для сервиса, управляющего записями документов (деталей).
    /// </summary>
    public interface IDocumentRecordService
    {
        /// <summary>
        /// Асинхронно получает все записи документов.
        /// </summary>
        /// <returns>Список всех записей.</returns>
        Task<List<DocumentDetailRecord>> GetAllRecordsAsync();

        /// <summary>
        /// Асинхронно добавляет новую запись документа.
        /// </summary>
        /// <param name="record">Новая запись.</param>
        /// <param name="eskdNumber">Децимальный номер для записи.</param>
        /// <param name="assemblyIds">Список ID сборок, в которые входит деталь.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task AddRecordAsync(DocumentDetailRecord record, ESKDNumber eskdNumber, List<int> assemblyIds);

        /// <summary>
        /// Асинхронно обновляет существующую запись документа.
        /// </summary>
        /// <param name="record">Запись с обновленными данными.</param>
        /// <param name="assemblyIds">Новый список ID сборок, в которые входит деталь.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task UpdateRecordAsync(DocumentDetailRecord record, List<int> assemblyIds);

        /// <summary>
        /// Асинхронно удаляет запись по ее ID.
        /// </summary>
        /// <param name="recordId">Идентификатор записи для удаления.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task DeleteRecordAsync(int recordId);

        /// <summary>
        /// Асинхронно получает список родительских сборок для указанной детали.
        /// </summary>
        /// <param name="detailId">Идентификатор детали.</param>
        /// <returns>Список родительских сборок.</returns>
        Task<List<Assembly>> GetParentAssembliesForDetailAsync(int detailId);
    }
}
