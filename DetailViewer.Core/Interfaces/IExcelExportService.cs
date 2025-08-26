using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для сервиса, экспортирующего данные в формат Excel.
    /// </summary>
    public interface IExcelExportService
    {
        /// <summary>
        /// Асинхронно экспортирует список записей документов в Excel-файл.
        /// </summary>
        /// <param name="filePath">Путь к файлу для сохранения.</param>
        /// <param name="records">Список записей для экспорта.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task ExportToExcelAsync(string filePath, List<DocumentDetailRecord> records);
    }
}