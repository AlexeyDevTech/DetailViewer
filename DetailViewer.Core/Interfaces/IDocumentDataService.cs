using DetailViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IDocumentDataService
    {
        Task<List<DocumentDetailRecord>> GetAllRecordsAsync();
        Task AddRecordAsync(DocumentDetailRecord record);
        Task UpdateRecordAsync(DocumentDetailRecord record);
        Task DeleteRecordAsync(int recordId);
    }
}
