using DetailViewer.Core.Models;
using System.Collections.Generic;

namespace DetailViewer.Core.Interfaces
{
    public interface IDocumentFilterService
    {
        List<DocumentRecord> FilterRecords(List<DocumentRecord> records, string searchTerm);
    }
}
