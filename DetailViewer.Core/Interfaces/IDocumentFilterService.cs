using DetailViewer.Core.Models;
using System.Collections.Generic;

namespace DetailViewer.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для сервиса, выполняющего фильтрацию записей документов.
    /// </summary>
    public interface IDocumentFilterService
    {
        /// <summary>
        /// Фильтрует список записей документов на основе поискового запроса.
        /// </summary>
        /// <param name="records">Исходный список записей.</param>
        /// <param name="searchTerm">Строка для поиска.</param>
        /// <returns>Отфильтрованный список записей.</returns>
        List<DocumentDetailRecord> FilterRecords(List<DocumentDetailRecord> records, string searchTerm);
    }
}