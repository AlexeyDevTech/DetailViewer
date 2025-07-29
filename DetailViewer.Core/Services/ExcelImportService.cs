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
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IClassifierProvider _classifierProvider;

        public ExcelImportService(IDbContextFactory<ApplicationDbContext> dbContextFactory, IClassifierProvider classifierProvider)
        {
            _dbContextFactory = dbContextFactory;
            _classifierProvider = classifierProvider;
            ExcelPackage.License.SetNonCommercialPersonal("My personal project");
        }

        public async Task ImportFromExcelAsync(string filePath, string sheetName, IProgress<Tuple<double, string>> progress, bool createRelationships)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[sheetName];
                    if (worksheet == null)
                    {
                        throw new Exception($"Лист '{sheetName}' не найден в файле.");
                    }

                    var rowCount = worksheet.Dimension.Rows;
                    var processedCount = 0;
                    var skippedCount = 0;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var eskdNumberString = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(eskdNumberString))
                        {
                            skippedCount++;
                            progress.Report(new Tuple<double, string>((double)row / rowCount * 100, $"Пропущено: {skippedCount}"));
                            continue;
                        }

                        var parsedEskd = new ESKDNumber().SetCode(eskdNumberString);
                        if (parsedEskd.ClassNumber == null || string.IsNullOrEmpty(parsedEskd.CompanyCode))
                        {
                            skippedCount++;
                            progress.Report(new Tuple<double, string>((double)row / rowCount * 100, $"Пропущено (ошибка парсинга): {skippedCount}"));
                            continue;
                        }

                        var classifierNumber = parsedEskd.ClassNumber.Number;
                        var detailNumber = parsedEskd.DetailNumber;
                        var version = parsedEskd.Version;
                        var companyCode = parsedEskd.CompanyCode;

                        var exists = await dbContext.ESKDNumbers
                            .AnyAsync(e => e.CompanyCode == companyCode &&
                                            e.ClassNumber.Number == classifierNumber &&
                                            e.DetailNumber == detailNumber &&
                                            e.Version == version);

                        if (exists)
                        {
                            skippedCount++;
                            progress.Report(new Tuple<double, string>((double)row / rowCount * 100, $"Пропущено (дубликат): {skippedCount}"));
                            continue;
                        }

                        var record = await CreateRecordFromRow(worksheet, row, eskdNumberString, dbContext);
                        dbContext.DocumentRecords.Add(record);

                        if (createRelationships)
                        {
                            await ProcessRelationships(worksheet, row, record, dbContext);
                        }

                        processedCount++;
                        progress.Report(new Tuple<double, string>((double)row / rowCount * 100, $"Обработано: {processedCount}"));
                    }
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Здесь можно добавить логирование
                throw new Exception("Ошибка во время импорта: " + ex.Message, ex);
            }
        }

        private async Task<DocumentDetailRecord> CreateRecordFromRow(ExcelWorksheet worksheet, int row, string eskdNumberString, ApplicationDbContext dbContext)
        {
            var eskdNumber = await GetOrCreateEskdNumber(eskdNumberString, dbContext);
            return new DocumentDetailRecord
            {
                Date = ParseDate(worksheet.Cells[row, 2].Value),
                ESKDNumber = eskdNumber,
                YASTCode = worksheet.Cells[row, 4].Value?.ToString()?.Trim(),
                Name = worksheet.Cells[row, 5].Value?.ToString()?.Trim(),
                FullName = worksheet.Cells[row, 10].Value?.ToString()?.Trim(),
            };
        }

        private async Task ProcessRelationships(ExcelWorksheet worksheet, int row, DocumentDetailRecord record, ApplicationDbContext dbContext)
        {
            var assemblyNumberString = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(assemblyNumberString)) return;

            var parsedAssemblyEskd = new ESKDNumber().SetCode(assemblyNumberString);
            if (parsedAssemblyEskd.ClassNumber == null || string.IsNullOrEmpty(parsedAssemblyEskd.CompanyCode)) return;

            var assemblyClassifier = parsedAssemblyEskd.ClassNumber.Number;
            var assemblyDetail = parsedAssemblyEskd.DetailNumber;
            var assemblyVersion = parsedAssemblyEskd.Version;
            var assemblyCompany = parsedAssemblyEskd.CompanyCode;

            var assembly = await dbContext.Assemblies.Include(a => a.EskdNumber)
                .FirstOrDefaultAsync(a => a.EskdNumber.CompanyCode == assemblyCompany &&
                                        a.EskdNumber.ClassNumber.Number == assemblyClassifier &&
                                        a.EskdNumber.DetailNumber == assemblyDetail &&
                                        a.EskdNumber.Version == assemblyVersion);

            if (assembly == null)
            {
                assembly = new Assembly
                {
                    EskdNumber = await GetOrCreateEskdNumber(assemblyNumberString, dbContext),
                    Name = worksheet.Cells[row, 8].Value?.ToString()?.Trim(),
                };
                dbContext.Assemblies.Add(assembly);
            }

            var assemblyDetailRecord = new AssemblyDetail { Assembly = assembly, Detail = record };
            dbContext.AssemblyDetails.Add(assemblyDetailRecord);

            var productNumberString = worksheet.Cells[row, 9].Value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(productNumberString)) return;

            var parsedProductEskd = new ESKDNumber().SetCode(productNumberString);
            if (parsedProductEskd.ClassNumber == null || string.IsNullOrEmpty(parsedProductEskd.CompanyCode)) return;

            var productClassifier = parsedProductEskd.ClassNumber.Number;
            var productDetail = parsedProductEskd.DetailNumber;
            var productVersion = parsedProductEskd.Version;
            var productCompany = parsedProductEskd.CompanyCode;

            var product = await dbContext.Products.Include(p => p.EskdNumber)
                .FirstOrDefaultAsync(p => p.EskdNumber.CompanyCode == productCompany &&
                                        p.EskdNumber.ClassNumber.Number == productClassifier &&
                                        p.EskdNumber.DetailNumber == productDetail &&
                                        p.EskdNumber.Version == productVersion);

            if (product == null)
            {
                product = new Product
                {
                    EskdNumber = await GetOrCreateEskdNumber(productNumberString, dbContext),
                    Name = worksheet.Cells[row, 10].Value?.ToString()?.Trim(),
                };
                dbContext.Products.Add(product);
            }

            var productAssembly = new ProductAssembly { Product = product, Assembly = assembly };
            dbContext.ProductAssemblies.Add(productAssembly);
        }

        private async Task<ESKDNumber> GetOrCreateEskdNumber(string eskdNumberString, ApplicationDbContext dbContext)
        {
            var eskdNumber = new ESKDNumber().SetCode(eskdNumberString);
            if (eskdNumber.ClassNumber != null)
            {
                var classifierCode = eskdNumber.ClassNumber.Number.ToString("D6");
                var classifier = await dbContext.Classifiers.FirstOrDefaultAsync(c => c.Number.ToString() == classifierCode);
                if (classifier == null)
                {
                    var classifierInfo = _classifierProvider.GetClassifierByCode(classifierCode);
                    if (classifierInfo != null)
                    {
                        classifier = new Classifier
                        {
                            Number = int.Parse(classifierInfo.Code),
                            Description = classifierInfo.Description
                        };
                        dbContext.Classifiers.Add(classifier);
                    }
                }
                eskdNumber.ClassNumber = classifier;
            }
            return eskdNumber;
        }

        private DateTime ParseDate(object dateValue)
        {
            if (dateValue is double oaDate) return DateTime.FromOADate(oaDate);
            return DateTime.TryParse(dateValue?.ToString(), out var date) ? date : DateTime.MinValue;
        }
    }
}