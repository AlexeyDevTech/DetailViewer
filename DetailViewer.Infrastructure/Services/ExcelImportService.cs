#nullable enable
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Infrastructure.Services
{
    /// <summary>
    /// Реализация сервиса для импорта данных из Excel-файлов.
    /// </summary>
    public class ExcelImportService : IExcelImportService
    {
        private readonly ILogger _logger;
        private readonly IDocumentRecordService _documentRecordService;
        private readonly IAssemblyService _assemblyService;
        private readonly IProductService _productService;
        private readonly IClassifierService _classifierService;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExcelImportService"/>.
        /// </summary>
        public ExcelImportService(IDocumentRecordService documentRecordService, IAssemblyService assemblyService, IProductService productService, IClassifierService classifierService, ILogger logger)
        {
            _documentRecordService = documentRecordService;
            _assemblyService = assemblyService;
            _productService = productService;
            _classifierService = classifierService;
            _logger = logger;
            ExcelPackage.License.SetNonCommercialPersonal("My personal project");
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Импортирует сборки из листа "СБ" Excel-файла.
        /// </summary>
        private async Task ImportAssembliesAsync(ExcelPackage package, IProgress<Tuple<double, string>> progress)
        {
            _logger.Log("Starting assembly import from 'СБ' sheet.");
            var worksheet = package.Workbook.Worksheets["СБ"];
            if (worksheet == null)
            {
                _logger.LogWarning("'СБ' sheet not found. Skipping assembly import.");
                progress.Report(new Tuple<double, string>(0, "Лист 'СБ' не найден, импорт сборок пропущен."));
                return;
            }

            var existingAssemblies = (await _assemblyService.GetAssembliesAsync()).ToDictionary(a => a.EskdNumber.FullCode, a => a);
            var rowCount = worksheet.Dimension.Rows;
            int newAssemblies = 0;
            int serviceNumberCounter = 1;

            for (int row = 2; row <= rowCount; row++)
            {
                var name = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                var eskdNumberString = worksheet.Cells[row, 3].Value?.ToString()?.Trim();

                // Если нет ни имени, ни номера - пропускаем строку
                if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(eskdNumberString))
                {
                    continue;
                }

                ESKDNumber eskdNumber;
                // Если номер пуст, но есть имя - генерируем служебный номер
                if (string.IsNullOrWhiteSpace(eskdNumberString))
                {
                    eskdNumberString = $"СЛУЖ.000000.{serviceNumberCounter++:D3}";
                    eskdNumber = new ESKDNumber().SetCode(eskdNumberString);
                }
                else
                {
                    eskdNumber = new ESKDNumber().SetCode(eskdNumberString);
                }

                var fullCode = eskdNumber.GetCode();
                if (existingAssemblies.ContainsKey(fullCode))
                {
                    continue;
                }

                var assembly = new Assembly
                {
                    EskdNumber = eskdNumber,
                    Name = name,
                    Date = ParseDate(worksheet.Cells[row, 2].Value), // Добавлено чтение даты
                    Author = worksheet.Cells[row, 8].Value?.ToString()?.Trim() // Добавлено чтение автора
                };

                await _assemblyService.AddAssemblyAsync(assembly, new List<int>(), new List<int>());
                existingAssemblies.Add(fullCode, assembly);
                newAssemblies++;
                progress.Report(new Tuple<double, string>((double)row / rowCount * 100, $"Импортирована сборка: {name} ({eskdNumberString})"));
            }

            _logger.Log($"Assembly import completed. Added {newAssemblies} new assemblies.");
            progress.Report(new Tuple<double, string>(100, $"Импорт сборок завершен. Добавлено: {newAssemblies}"));
        }

        /// <summary>
        /// Создает объект DocumentDetailRecord из строки Excel.
        /// </summary>
        private async Task<DocumentDetailRecord> CreateRecordFromRow(ExcelWorksheet worksheet, int row, string eskdNumberString)
        {
            var eskdNumber = new ESKDNumber().SetCode(eskdNumberString);

            if (eskdNumber.ClassNumber != null)
            {
                var classifier = _classifierService.GetClassifierByNumber(eskdNumber.ClassNumber.Number);
                eskdNumber.ClassNumber = classifier != null
                    ? new Classifier { Number = classifier.Number, Description = classifier.Description }
                    : new Classifier() { Description = "<неопознанный код>" };
            }

            return new DocumentDetailRecord
            {
                Date = ParseDate(worksheet.Cells[row, 2].Value),
                ESKDNumber = eskdNumber,
                YASTCode = worksheet.Cells[row, 4].Value?.ToString()?.Trim(),
                Name = worksheet.Cells[row, 5].Value?.ToString()?.Trim(),
                FullName = worksheet.Cells[row, 10].Value?.ToString()?.Trim(),
            };
        }

        /// <summary>
        /// Обрабатывает связь детали со сборкой из строки Excel.
        /// </summary>
        private async Task<Assembly?> ProcessAssemblyRelationship(ExcelWorksheet worksheet, int row)
        {
            var assemblyNumberString = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(assemblyNumberString)) return null;

            var assemblies = await _assemblyService.GetAssembliesAsync();
            var assembly = assemblies.FirstOrDefault(a => a.EskdNumber.FullCode == assemblyNumberString);

            if (assembly == null)
            {
                _logger.LogWarning($"Assembly with number {assemblyNumberString} not found. Skipping relationship.");
                return null;
            }
            return assembly;
        }

        /// <summary>
        /// Парсит дату из ячейки Excel, поддерживая числовой и строковый форматы.
        /// </summary>
        private DateTime ParseDate(object? dateValue)
        {
            if (dateValue is double oaDate) return DateTime.FromOADate(oaDate);
            return DateTime.TryParse(dateValue?.ToString(), out var date) ? date : DateTime.MinValue;
        }
    }
}
