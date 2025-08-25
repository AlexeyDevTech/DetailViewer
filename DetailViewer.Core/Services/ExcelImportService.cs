#nullable enable
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class ExcelImportService : IExcelImportService
    {
        private readonly ILogger _logger;
        private readonly IDocumentRecordService _documentRecordService;
        private readonly IAssemblyService _assemblyService;
        private readonly IProductService _productService;
        private readonly IClassifierService _classifierService;

        public ExcelImportService(IDocumentRecordService documentRecordService, IAssemblyService assemblyService, IProductService productService, IClassifierService classifierService, ILogger logger)
        {
            _documentRecordService = documentRecordService;
            _assemblyService = assemblyService;
            _productService = productService;
            _classifierService = classifierService;
            _logger = logger;
            ExcelPackage.License.SetNonCommercialPersonal("My personal project");
        }

        public async Task ImportFromExcelAsync(string filePath, string sheetName, IProgress<Tuple<double, string>> progress, bool createRelationships)
        {
            _logger.Log($"Importing from Excel: {filePath}, sheet: {sheetName}");
            try
            {
                var allRecords = await _documentRecordService.GetAllRecordsAsync();
                var existingEskdNumbers = allRecords.Select(r => r.ESKDNumber.FullCode).ToHashSet();

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    if (createRelationships)
                    {
                        await ImportAssembliesAsync(package, progress);
                    }

                    var worksheet = package.Workbook.Worksheets[sheetName];
                    if (worksheet == null)
                    {
                        throw new Exception($"Лист '{sheetName}' не найден в файле.");
                    }

                    var rowCount = worksheet.Dimension.Rows;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var eskdNumberString = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(eskdNumberString) || existingEskdNumbers.Contains(eskdNumberString))
                        {
                            progress.Report(new Tuple<double, string>((double)row / rowCount * 100, $"Пропущено: {eskdNumberString}"));
                            continue;
                        }

                        var record = await CreateRecordFromRow(worksheet, row, eskdNumberString);
                        var assemblyIds = new List<int>();
                        if (createRelationships)
                        {
                            var assembly = await ProcessAssemblyRelationship(worksheet, row);
                            if(assembly != null) assemblyIds.Add(assembly.Id);
                        }
                        
                        await _documentRecordService.AddRecordAsync(record, record.ESKDNumber, assemblyIds);
                        progress.Report(new Tuple<double, string>((double)row / rowCount * 100, $"Обработано: {eskdNumberString}"));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during Excel import: {ex.Message}", ex);
                throw new Exception("Ошибка во время импорта: " + ex.Message, ex);
            }
        }

        private async Task ImportAssembliesAsync(ExcelPackage package, IProgress<Tuple<double, string>> progress)
        {
            // This logic needs to be re-evaluated as it assumes direct DB access for checking existence and adding.
            // For now, it will be a simplified version.
            _logger.Log("Importing assemblies");
            progress.Report(new Tuple<double, string>(0, "Импорт сборок не полностью реализован в новой архитектуре."));
            await Task.CompletedTask;
        }

        private async Task<DocumentDetailRecord> CreateRecordFromRow(ExcelWorksheet worksheet, int row, string eskdNumberString)
        {
            var eskdNumber = new ESKDNumber().SetCode(eskdNumberString);
            // We assume classifier data is loaded and available through the service
            var classifier = _classifierService.GetClassifierByCode(eskdNumber.ClassNumber.Number.ToString("D6"));
            eskdNumber.ClassNumber = classifier != null ? new Classifier { Number = int.Parse(classifier.Code), Description = classifier.Description } : null;

            return new DocumentDetailRecord
            {
                Date = ParseDate(worksheet.Cells[row, 2].Value),
                ESKDNumber = eskdNumber,
                YASTCode = worksheet.Cells[row, 4].Value?.ToString()?.Trim(),
                Name = worksheet.Cells[row, 5].Value?.ToString()?.Trim(),
                FullName = worksheet.Cells[row, 10].Value?.ToString()?.Trim(),
            };
        }

        private async Task<Assembly?> ProcessAssemblyRelationship(ExcelWorksheet worksheet, int row)
        {
            var assemblyNumberString = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(assemblyNumberString)) return null;

            // This is a simplified logic. A real implementation would need to properly search
            // or create assemblies via the assembly service.
            var assemblies = await _assemblyService.GetAssembliesAsync();
            var assembly = assemblies.FirstOrDefault(a => a.EskdNumber.FullCode == assemblyNumberString);

            if (assembly == null)
            {
                // Placeholder for creating a new assembly if not found
                _logger.LogWarning($"Assembly with number {assemblyNumberString} not found. Skipping relationship.");
                return null;
            }
            return assembly;
        }

        private DateTime ParseDate(object? dateValue)
        {
            if (dateValue is double oaDate) return DateTime.FromOADate(oaDate);
            return DateTime.TryParse(dateValue?.ToString(), out var date) ? date : DateTime.MinValue;
        }
    }
}