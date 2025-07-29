
using System;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IExcelImportService
    {
        Task ImportFromExcelAsync(string filePath, IProgress<double> progress, bool createRelationships);
    }
}
