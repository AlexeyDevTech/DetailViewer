using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class CsvExportService : ICsvExportService
    {
        private readonly ILogger _logger;

        public CsvExportService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task ExportRecordsToCsvAsync(string filePath, List<DocumentDetailRecord> records)
        {
            _logger.Log($"Exporting {records.Count()} records to CSV: {filePath}");
            var csv = new StringBuilder();
            csv.AppendLine("Date;ESKDNumber;YASTCode;Name;Assemblies");

            foreach (var record in records)
            {
                var assemblies = record.AssemblyDetails != null && record.AssemblyDetails.Any()
                    ? string.Join(", ", record.AssemblyDetails.Select(ad => ad.Assembly?.EskdNumber?.FullCode ?? ""))
                    : "";

                csv.AppendLine($"{record.Date:yyyy-MM-dd};{record.ESKDNumber?.FullCode ?? ""};{record.YASTCode};{record.Name};\"{assemblies}\"");
            }

            await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
        }
    }
}