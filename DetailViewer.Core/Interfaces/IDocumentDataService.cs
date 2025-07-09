using DetailViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IDocumentDataService
    {
        Task<List<DocumentRecord>> GetAllRecordsAsync();
        Task AddRecordAsync(DocumentRecord record);
        Task UpdateRecordAsync(DocumentRecord record);
        Task DeleteRecordAsync(int recordId);
        Task ImportFromExcelAsync(string filePath);
        Task ExportToExcelAsync(string filePath);
    }
}
