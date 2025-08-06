using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace DetailViewer.Core.Services
{
    public static class EskdNumberHelper
    {
        public static int FindNextDetailNumber(IEnumerable<DocumentDetailRecord> records, string classNumberString)
        {
            if (records == null || string.IsNullOrEmpty(classNumberString) || classNumberString.Length != 6)
            {
                return 1;
            }

            var existingDetailNumbers = records
                .Where(r => r.ESKDNumber?.ClassNumber?.Number.ToString("D6") == classNumberString)
                .Select(r => r.ESKDNumber.DetailNumber)
                .ToHashSet();

            for (int i = 1; i <= 999; i++)
            {
                if (!existingDetailNumbers.Contains(i))
                {
                    return i;
                }
            }

            return 1000; // Or throw an exception, depending on requirements
        }

        public static int? FindNextVersionNumber(IEnumerable<DocumentDetailRecord> records, DocumentDetailRecord sourceRecord)
        {
            if (records == null || sourceRecord?.ESKDNumber?.ClassNumber == null)
            {
                return null;
            }

            var existingVersions = records
                .Where(r => r.ESKDNumber != null &&
                            r.ESKDNumber.ClassNumber != null &&
                            r.ESKDNumber.CompanyCode == sourceRecord.ESKDNumber.CompanyCode &&
                            r.ESKDNumber.ClassNumber.Number == sourceRecord.ESKDNumber.ClassNumber.Number &&
                            r.ESKDNumber.DetailNumber == sourceRecord.ESKDNumber.DetailNumber &&
                            r.ESKDNumber.Version.HasValue)
                .Select(r => r.ESKDNumber.Version.Value)
                .ToList();

            if (existingVersions.Any())
            {
                return existingVersions.Max() + 1;
            }
            else
            {
                return 1; // First version for this ESKD number
            }
        }
    }
}
