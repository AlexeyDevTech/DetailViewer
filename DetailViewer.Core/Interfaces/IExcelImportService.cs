
using System;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IExcelImportService
    {
        Task ImportFromExcelAsync(string filePath, string sheetName, IProgress<Tuple<double, string>> progress, bool createRelationships);
    }
}
