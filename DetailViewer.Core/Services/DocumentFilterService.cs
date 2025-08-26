using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace DetailViewer.Core.Services
{
    public class DocumentFilterService : IDocumentFilterService
    {
        public List<DocumentDetailRecord> FilterRecords(List<DocumentDetailRecord> records, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return records;
            }

            var lowerSearchText = searchTerm.ToLower();

            return records.Where(r =>
                (r.Name?.ToLower().Contains(lowerSearchText) ?? false) ||
                (r.ESKDNumber?.FullCode.ToLower().Contains(lowerSearchText) ?? false) ||
                (r.YASTCode?.ToLower().Contains(lowerSearchText) ?? false)
            ).ToList();
        }
    }
}