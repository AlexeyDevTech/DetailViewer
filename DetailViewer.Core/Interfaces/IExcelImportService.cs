using System;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для сервиса, импортирующего данные из формата Excel.
    /// </summary>
    public interface IExcelImportService
    {
        /// <summary>
        /// Асинхронно импортирует записи документов из Excel-файла.
        /// </summary>
        /// <param name="filePath">Путь к Excel-файлу.</param>
        /// <param name="sheetName">Название листа в файле для импорта.</param>
        /// <param name="progress">Объект для отслеживания прогресса операции.</param>
        /// <param name="createRelationships">Флаг, указывающий, нужно ли создавать связи между сущностями.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task ImportFromExcelAsync(string filePath, string sheetName, IProgress<Tuple<double, string>> progress, bool createRelationships);
    }
}