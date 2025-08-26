
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IExcelExportService
    {
        Task ExportToExcelAsync(string filePath, System.Collections.Generic.List<Models.DocumentDetailRecord> records);
    }
}
