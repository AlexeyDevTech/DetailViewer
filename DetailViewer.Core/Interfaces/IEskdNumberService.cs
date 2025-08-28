
using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IEskdNumberService
    {
        /// <summary>
        /// Находит следующий доступный номер детали на основе кода класса.
        /// </summary>
        /// <param name="classCode">6-значный код класса.</param>
        /// <returns>Следующий номер детали.</returns>
        Task<int> GetNextDetailNumberAsync(string classCode);

        /// <summary>
        /// Находит следующий доступный номер версии для существующей записи.
        /// </summary>
        /// <param name="selectedRecord">Запись, для которой ищется новая версия.</param>
        /// <returns>Следующий номер версии.</returns>
        Task<int?> GetNextVersionNumberAsync(DocumentDetailRecord selectedRecord);
    }
}
