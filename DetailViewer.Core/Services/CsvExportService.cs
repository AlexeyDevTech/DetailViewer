using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
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

        public async Task ExportRecordsToCsvAsync(string filePath, List<DocumentRecord> records)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Write CSV header
                    await writer.WriteLineAsync("Date,ESKDNumber,YASTCode,Name,AssemblyNumber,AssemblyName,ProductNumber,ProductName,FullName");

                    // Write records
                    foreach (var record in records)
                    {
                        var line = $"{record.Date:yyyy-MM-dd},{record.ESKDNumber.GetCode()},{EscapeCsvField(record.YASTCode)},{EscapeCsvField(record.Name)},{EscapeCsvField(record.AssemblyNumber)},{EscapeCsvField(record.AssemblyName)},{EscapeCsvField(record.ProductNumber)},{EscapeCsvField(record.ProductName)},{EscapeCsvField(record.FullName)}";
                        await writer.WriteLineAsync(line);
                    }
                }
                _logger.LogInformation($"Successfully exported {records.Count} records to CSV: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error exporting records to CSV file {filePath}: {ex.Message}", ex);
                throw; // Re-throw to propagate the error
            }
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return string.Empty;

            // If the field contains a comma, double quote, or newline, enclose it in double quotes
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", """") + "\"";
            }
            return field;
        }
    }
}
