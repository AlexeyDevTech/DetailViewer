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

        public async Task ImportFromExcelAsync(string filePath, IProgress<double> progress, bool createRelationships)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var existingEskdNumbers = new HashSet<string>(await dbContext.ESKDNumbers.Select(e => e.FullCode).ToListAsync());
                var existingAssemblies = (await dbContext.Assemblies.Include(a => a.EskdNumber).ToListAsync()).ToDictionary(a => a.EskdNumber.FullCode);
                var existingProducts = (await dbContext.Products.Include(p => p.EskdNumber).ToListAsync()).ToDictionary(p => p.EskdNumber.FullCode);

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals("Детали", StringComparison.OrdinalIgnoreCase) || ws.Name.Equals("Details", StringComparison.OrdinalIgnoreCase));
                    if (worksheet == null) return;

                    var rowCount = worksheet.Dimension.Rows;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var eskdNumberString = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(eskdNumberString) || existingEskdNumbers.Contains(eskdNumberString))
                        {
                            progress.Report((double)row / rowCount * 100);
                            continue;
                        }

                        var record = await CreateRecordFromRow(worksheet, row, eskdNumberString, dbContext);
                        dbContext.DocumentRecords.Add(record);
                        existingEskdNumbers.Add(eskdNumberString);

                        if (createRelationships)
                        {
                            await ProcessRelationships(worksheet, row, record, existingAssemblies, existingProducts, dbContext);
                        }

                        progress.Report((double)row / rowCount * 100);
                    }
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
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

        private async Task ProcessRelationships(ExcelWorksheet worksheet, int row, DocumentDetailRecord record, Dictionary<string, Assembly> assemblies, Dictionary<string, Product> products, ApplicationDbContext dbContext)
        {
            var assemblyNumber = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(assemblyNumber)) return;

            if (!assemblies.TryGetValue(assemblyNumber, out var assembly))
            {
                assembly = new Assembly
                {
                    EskdNumber = await GetOrCreateEskdNumber(assemblyNumber, dbContext),
                    Name = worksheet.Cells[row, 7].Value?.ToString()?.Trim(),
                };
                dbContext.Assemblies.Add(assembly);
                assemblies.Add(assemblyNumber, assembly);
            }

            var assemblyDetail = new AssemblyDetail { Assembly = assembly, Detail = record };
            dbContext.AssemblyDetails.Add(assemblyDetail);

            var productNumber = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(productNumber)) return;

            if (!products.TryGetValue(productNumber, out var product))
            {
                product = new Product
                {
                    EskdNumber = await GetOrCreateEskdNumber(productNumber, dbContext),
                    Name = worksheet.Cells[row, 9].Value?.ToString()?.Trim(),
                };
                dbContext.Products.Add(product);
                products.Add(productNumber, product);
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