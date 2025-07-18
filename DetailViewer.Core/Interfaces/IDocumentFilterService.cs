using DetailViewer.Core.Models;
using System.Collections.Generic;

namespace DetailViewer.Core.Interfaces
{
    public interface IDocumentFilterService
    {
        List<DocumentDetailRecord> FilterRecords(List<DocumentDetailRecord> records, string searchTerm);
    }
}
