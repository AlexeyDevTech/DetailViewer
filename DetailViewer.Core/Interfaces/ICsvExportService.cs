using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface ICsvExportService
    {
        Task ExportRecordsToCsvAsync(string filePath, List<DocumentDetailRecord> records);
    }
}
