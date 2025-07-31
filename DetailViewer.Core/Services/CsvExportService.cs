using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System;
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
            _logger.Log($"Exporting {records.Count} records to CSV: {filePath}");
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Write CSV header
                    await writer.WriteLineAsync("Date,ESKDNumber,YASTCode,Name,AssemblyNumber,AssemblyName,ProductNumber,ProductName,FullName");

                    // Write records
                    foreach (var record in records)
                    {
                        if (record.AssemblyDetails.Any())
                        {
                            foreach (var ad in record.AssemblyDetails)
                            {
                                if (ad.Assembly.ProductAssemblies.Any())
                                {
                                    foreach (var pa in ad.Assembly.ProductAssemblies)
                                    {
                                        var line = $"{record.Date:yyyy-MM-dd},{record.ESKDNumber.GetCode()},{EscapeCsvField(record.YASTCode)},{EscapeCsvField(record.Name)},{EscapeCsvField(ad.Assembly.EskdNumber.FullCode)},{EscapeCsvField(ad.Assembly.Name)},{EscapeCsvField(pa.Product.EskdNumber.FullCode)},{EscapeCsvField(pa.Product.Name)},{EscapeCsvField(record.FullName)}";
                                        await writer.WriteLineAsync(line);
                                    }
                                }
                                else
                                {
                                    var line = $"{record.Date:yyyy-MM-dd},{record.ESKDNumber.GetCode()},{EscapeCsvField(record.YASTCode)},{EscapeCsvField(record.Name)},{EscapeCsvField(ad.Assembly.EskdNumber.FullCode)},{EscapeCsvField(ad.Assembly.Name)},,,{EscapeCsvField(record.FullName)}";
                                    await writer.WriteLineAsync(line);
                                }
                            }
                        }
                        else
                        {
                            var line = $"{record.Date:yyyy-MM-dd},{record.ESKDNumber.GetCode()},{EscapeCsvField(record.YASTCode)},{EscapeCsvField(record.Name)},,,,,{EscapeCsvField(record.FullName)}";
                            await writer.WriteLineAsync(line);
                        }
                    }
                }
                _logger.LogInfo($"Successfully exported {records.Count} records to CSV: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error exporting records to CSV file {filePath}: {ex.Message}", ex);
                throw; // Re-throw to propagate the error
            }
        }

        private string EscapeCsvField(string field)
        {
            _logger.Log($"Escaping CSV field: {field}");
            if (string.IsNullOrEmpty(field)) return string.Empty;

            // If the field contains a comma, double quote, or newline, enclose it in double quotes
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}
