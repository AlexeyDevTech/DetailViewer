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
        Task<List<DocumentRecord>> ReadRecordsAsync(string source, string sheetName = null);
        Task WriteRecordsAsync(string source, List<DocumentRecord> records, string sheetName = null);
    }
}
