
using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
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
        private readonly ApplicationDbContext _dbContext;
        private readonly IClassifierProvider _classifierProvider;

        public ExcelImportService(ApplicationDbContext dbContext, IClassifierProvider classifierProvider)
        {
            _dbContext = dbContext;
            _classifierProvider = classifierProvider;
            ExcelPackage.License.SetNonCommercialPersonal("My personal project");
        }

        public async Task ImportFromExcelAsync(string filePath, IProgress<double> progress)
        {
            var existingEskdNumbers = new HashSet<string>(await _dbContext.ESKDNumbers.Select(e => e.FullCode).ToListAsync());
            var recordsToAdd = new List<DocumentDetailRecord>();
            var importedEskdNumbers = new HashSet<string>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets["Детали"];
                if (worksheet == null)
                {
                    return;
                }
                var rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var eskdNumberString = worksheet.Cells[row, 3].Value?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(eskdNumberString) || existingEskdNumbers.Contains(eskdNumberString) || importedEskdNumbers.Contains(eskdNumberString))
                    {
                        progress.Report((double)row / rowCount * 100);
                        continue;
                    }

                    var record = await CreateRecordFromExcelRow(worksheet, row, eskdNumberString);
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

        private async Task<DocumentDetailRecord> CreateRecordFromExcelRow(ExcelWorksheet worksheet, int row, string eskdNumberString)
        {
            var date = ParseDate(worksheet.Cells[row, 2].Value);
            var eskdNumber = await CreateEskdNumber(eskdNumberString);

            return new DocumentDetailRecord
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
        }

        private DateTime ParseDate(object dateValue)
        {
            if (dateValue is double oaDate)
            {
                return DateTime.FromOADate(oaDate);
            }
            if (DateTime.TryParse(dateValue?.ToString(), out var date))
            {
                return date;
            }
            return DateTime.MinValue;
        }

        private async Task<ESKDNumber> CreateEskdNumber(string eskdNumberString)
        {
            var eskdNumber = new ESKDNumber().SetCode(eskdNumberString);
            if (eskdNumber.ClassNumber != null)
            {
                var classifierNumber = eskdNumber.ClassNumber.Number.ToString("D6");
                var classifier = await _dbContext.Classifiers.FirstOrDefaultAsync(c => c.Number.ToString() == classifierNumber);

                if (classifier == null)
                {
                    var classifierInfo = _classifierProvider.GetClassifierByCode(classifierNumber);
                    if (classifierInfo != null)
                    {
                        classifier = new Classifier
                        {
                            Number = int.Parse(classifierInfo.Code),
                            Description = classifierInfo.Description
                        };
                        _dbContext.Classifiers.Add(classifier);
                        await _dbContext.SaveChangesAsync(); 
                    }
                }
                eskdNumber.ClassNumber = classifier;
            }
            return eskdNumber;
        }
    }
}
