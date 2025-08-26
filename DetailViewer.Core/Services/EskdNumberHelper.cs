using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace DetailViewer.Core.Services
{
    public static class EskdNumberHelper
    {
        public static int FindNextDetailNumber(IEnumerable<DocumentDetailRecord> records, string classCode)
        {
            if (records == null || !records.Any() || string.IsNullOrEmpty(classCode) || classCode.Length != 6)
            {
                return 1;
            }

            var relevantRecords = records
                .Where(r => r.ESKDNumber?.ClassNumber?.Number.ToString("D6") == classCode)
                .ToList();

            if (!relevantRecords.Any())
            {
                return 1;
            }

            return relevantRecords.Max(r => r.ESKDNumber.DetailNumber) + 1;
        }

        public static int FindNextVersionNumber(IEnumerable<DocumentDetailRecord> allRecords, DocumentDetailRecord? selectedRecord)
        {
            if (selectedRecord == null || allRecords == null)
            {
                return 1;
            }

            var versions = allRecords
                .Where(r => r.ESKDNumber.CompanyCode == selectedRecord.ESKDNumber.CompanyCode &&
                            r.ESKDNumber.ClassNumber?.Number == selectedRecord.ESKDNumber.ClassNumber?.Number &&
                            r.ESKDNumber.DetailNumber == selectedRecord.ESKDNumber.DetailNumber)
                .Select(r => r.ESKDNumber.Version)
                .OfType<int>()
                .ToList();

            return versions.Any() ? versions.Max() + 1 : 1;
        }
    }
}
