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
                return records; // Return all records if search term is empty
            }

            string lowerSearchTerm = searchTerm.ToLower();

            return records.Where(r =>
                r.ESKDNumber?.GetCode()?.ToLower().Contains(lowerSearchTerm) == true ||
                r.YASTCode?.ToLower().Contains(lowerSearchTerm) == true ||
                r.Name?.ToLower().Contains(lowerSearchTerm) == true ||
                r.FullName?.ToLower().Contains(lowerSearchTerm) == true
            ).ToList();
        }
    }
}
