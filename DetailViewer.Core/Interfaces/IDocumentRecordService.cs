using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IDocumentRecordService
    {
        Task<List<DocumentDetailRecord>> GetAllRecordsAsync();
        Task AddRecordAsync(DocumentDetailRecord record, ESKDNumber eskdNumber, List<int> assemblyIds);
        Task UpdateRecordAsync(DocumentDetailRecord record, List<int> assemblyIds);
        Task DeleteRecordAsync(int recordId);
        Task<List<Assembly>> GetParentAssembliesForDetailAsync(int detailId);
    }
}