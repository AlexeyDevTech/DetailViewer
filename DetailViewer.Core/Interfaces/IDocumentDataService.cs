using DetailViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IDocumentDataService
    {
        Task<List<DocumentDetailRecord>> GetAllRecordsAsync();
        Task AddRecordAsync(DocumentDetailRecord record, int? assemblyId);
        Task UpdateRecordAsync(DocumentDetailRecord record, int? assemblyId);
        Task DeleteRecordAsync(int recordId);
        Task<List<Assembly>> GetAssembliesAsync();
        Task<List<Product>> GetProductsAsync();
    }
}
