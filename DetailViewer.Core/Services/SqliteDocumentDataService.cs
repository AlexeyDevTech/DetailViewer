using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class SqliteDocumentDataService : IDocumentDataService
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly Dictionary<string, ClassifierData> _classifierData;

        public SqliteDocumentDataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbContext.Database.EnsureCreated();
            ExcelPackage.License.SetNonCommercialPersonal("My personal project");

            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eskd_classifiers.json");
            var jsonData = File.ReadAllText(jsonPath);
            var rootClassifiers = JsonSerializer.Deserialize<List<ClassifierData>>(jsonData);
            _classifierData = FlattenClassifiers(rootClassifiers).ToDictionary(c => c.Code);
        }

        private List<ClassifierData> FlattenClassifiers(List<ClassifierData> classifiers)
        {
            var flattenedList = new List<ClassifierData>();
            foreach (var classifier in classifiers)
            {
                flattenedList.Add(classifier);
                if (classifier.Children != null && classifier.Children.Any())
                {
                    flattenedList.AddRange(FlattenClassifiers(classifier.Children));
                }
            }
            return flattenedList;
        }

        public async Task<List<DocumentRecord>> GetAllRecordsAsync()
        {
            return await _dbContext.DocumentRecords.Include(r => r.ESKDNumber).ThenInclude(e => e.ClassNumber).ToListAsync();
        }

        public async Task AddRecordAsync(DocumentRecord record)
        {
            _dbContext.Classifiers.Add(record.ESKDNumber.ClassNumber);
            _dbContext.ESKDNumbers.Add(record.ESKDNumber);
            _dbContext.DocumentRecords.Add(record);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateRecordAsync(DocumentRecord record)
        {
            _dbContext.DocumentRecords.Update(record);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteRecordAsync(int recordId)
        {
            var record = await _dbContext.DocumentRecords.FindAsync(recordId);
            if (record != null)
            {
                _dbContext.DocumentRecords.Remove(record);
                var eskdNumber = await _dbContext.ESKDNumbers.FindAsync(record.ESKDNumberId);
                if (eskdNumber != null)
                {
                    _dbContext.ESKDNumbers.Remove(eskdNumber);
                    var classifier = await _dbContext.Classifiers.FindAsync(eskdNumber.ClassifierId);
                    if (classifier != null)
                    {
                        _dbContext.Classifiers.Remove(classifier);
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task ImportFromExcelAsync(string filePath, IProgress<double> progress)
        {
            var existingEskdNumbers = new HashSet<string>(await _dbContext.ESKDNumbers.Select(e => e.FullCode).ToListAsync());
            var recordsToAdd = new List<DocumentRecord>();
            var importedEskdNumbers = new HashSet<string>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets["Детали"];
                if (worksheet == null)
                {
                    // Handle case where sheet is not found, maybe log an error
                    return;
                }
                var rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var eskdNumberString = worksheet.Cells[row, 3].Value?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(eskdNumberString) || existingEskdNumbers.Contains(eskdNumberString) || importedEskdNumbers.Contains(eskdNumberString))
                    {
                        // Skip duplicate or empty
                        progress.Report((double)row / rowCount * 100);
                        continue;
                    }

                    DateTime date;
                    var dateValue = worksheet.Cells[row, 2].Value;

                    if (dateValue is double oaDate)
                    {
                        date = DateTime.FromOADate(oaDate);
                    }
                    else if (!DateTime.TryParse(dateValue?.ToString(), out date))
                    {
                        date = DateTime.MinValue;
                        Console.WriteLine($"Warning: Could not parse date on row {row}. Value: '{dateValue}'. Using default value.");
                    }

                    var eskdNumber = new ESKDNumber().SetCode(eskdNumberString);
                    if (eskdNumber.ClassNumber != null)
                    {
                        var classifierNumber = eskdNumber.ClassNumber.Number.ToString("D6");
                        var classifier = await _dbContext.Classifiers.FirstOrDefaultAsync(c => c.Number.ToString() == classifierNumber);

                        if (classifier == null)
                        {
                            if (_classifierData.TryGetValue(classifierNumber, out var classifierInfo))
                            {
                                classifier = new Classifier
                                {
                                    Number = int.Parse(classifierInfo.Code),
                                    Description = classifierInfo.Description
                                };
                                _dbContext.Classifiers.Add(classifier);
                                // Save immediately to get an ID for the new classifier
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                        eskdNumber.ClassNumber = classifier;
                    }

                    var record = new DocumentRecord
                    {
                        Date = date,
                        ESKDNumber = eskdNumber,
                        YASTCode = worksheet.Cells[row, 4].Value?.ToString() ?? string.Empty,
                        Name = worksheet.Cells[row, 5].Value?.ToString() ?? string.Empty,
                        AssemblyNumber = worksheet.Cells[row, 6].Value?.ToString() ?? string.Empty,
                        AssemblyName = worksheet.Cells[row, 7].Value?.ToString() ?? string.Empty,
                        ProductNumber = worksheet.Cells[row, 8].Value?.ToString() ?? string.Empty,
                        ProductName = worksheet.Cells[row, 9].Value?.ToString() ?? string.Empty,
                        FullName = worksheet.Cells[row, 10].Value?.ToString() ?? string.Empty,
                    };
                    recordsToAdd.Add(record);
                    importedEskdNumbers.Add(eskdNumberString);
                    progress.Report((double)row / rowCount * 100);
                }
            }

            if (recordsToAdd.Any())
            {
                _dbContext.DocumentRecords.AddRange(recordsToAdd);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task ExportToExcelAsync(string filePath)
        {
            var records = await GetAllRecordsAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Export");

                // Add header
                worksheet.Cells[1, 1].Value = "Date";
                worksheet.Cells[1, 2].Value = "ESKDNumber";
                worksheet.Cells[1, 3].Value = "YASTCode";
                worksheet.Cells[1, 4].Value = "Name";
                worksheet.Cells[1, 5].Value = "AssemblyNumber";
                worksheet.Cells[1, 6].Value = "AssemblyName";
                worksheet.Cells[1, 7].Value = "ProductNumber";
                worksheet.Cells[1, 8].Value = "ProductName";
                worksheet.Cells[1, 9].Value = "FullName";
                worksheet.Cells[1, 10].Value = "ClassifierName";
                worksheet.Cells[1, 11].Value = "ClassifierNumber";
                worksheet.Cells[1, 12].Value = "ClassifierDescription";


                for (int i = 0; i < records.Count; i++)
                {
                    var record = records[i];
                    worksheet.Cells[i + 2, 1].Value = record.Date;
                    worksheet.Cells[i + 2, 2].Value = record.ESKDNumber?.GetCode();
                    worksheet.Cells[i + 2, 3].Value = record.YASTCode;
                    worksheet.Cells[i + 2, 4].Value = record.Name;
                    worksheet.Cells[i + 2, 5].Value = record.AssemblyNumber;
                    worksheet.Cells[i + 2, 6].Value = record.AssemblyName;
                    worksheet.Cells[i + 2, 7].Value = record.ProductNumber;
                    worksheet.Cells[i + 2, 8].Value = record.ProductName;
                    worksheet.Cells[i + 2, 9].Value = record.FullName;
                    worksheet.Cells[i + 2, 10].Value = record.ESKDNumber?.ClassNumber?.Name;
                    worksheet.Cells[i + 2, 11].Value = record.ESKDNumber?.ClassNumber?.Number;
                    worksheet.Cells[i + 2, 12].Value = record.ESKDNumber?.ClassNumber?.Description;
                }

                await package.SaveAsAsync(new FileInfo(filePath));
            }
        }
    }
}
