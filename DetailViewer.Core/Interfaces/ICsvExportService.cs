using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для сервиса, экспортирующего данные в формат CSV.
    /// </summary>
    public interface ICsvExportService
    {
        /// <summary>
        /// Асинхронно экспортирует список записей документов в CSV-файл.
        /// </summary>
        /// <param name="filePath">Путь к файлу для сохранения.</param>
        /// <param name="records">Список записей для экспорта.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task ExportRecordsToCsvAsync(string filePath, List<DocumentDetailRecord> records);
    }
}