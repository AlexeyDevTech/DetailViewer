using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class SqliteDocumentDataService : IDocumentDataService
    {
        private readonly ApplicationDbContext _dbContext;

        public SqliteDocumentDataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbContext.Database.EnsureCreated();
            ExcelPackage.License.SetNonCommercialPersonal("My personal project");
        }

        public async Task<List<DocumentRecord>> GetAllRecordsAsync()
        {
            return await _dbContext.DocumentRecords.ToListAsync();
        }

        public async Task AddRecordAsync(DocumentRecord record)
        {
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
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task ImportFromExcelAsync(string filePath)
        {
            var records = new List<DocumentRecord>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var record = new DocumentRecord
                    {
                        // Assuming the Excel columns are in a specific order.
                        // This needs to be adjusted based on the actual Excel file format.
                        Date = System.DateTime.Now, // Placeholder
                        ESKDNumber = new ESKDNumber().SetCode(worksheet.Cells[row, 2].Value.ToString()),
                        YASTCode = worksheet.Cells[row, 3].Value.ToString(),
                        Name = worksheet.Cells[row, 4].Value.ToString(),
                        AssemblyNumber = worksheet.Cells[row, 5].Value.ToString(),
                        AssemblyName = worksheet.Cells[row, 6].Value.ToString(),
                        ProductNumber = worksheet.Cells[row, 7].Value.ToString(),
                        ProductName = worksheet.Cells[row, 8].Value.ToString(),
                        FullName = worksheet.Cells[row, 9].Value.ToString(),
                    };
                    records.Add(record);
                }
            }

            foreach (var record in records)
            {
                // Map flattened properties
                record.CompanyCode = record.ESKDNumber.CompanyCode;
                record.ClassNumber = record.ESKDNumber.ClassNumber.Number;
                record.ClassifierName = record.ESKDNumber.ClassNumber.Name;
                record.DetailNumber = record.ESKDNumber.DetailNumber;
                record.Version = record.ESKDNumber.Version;

                _dbContext.DocumentRecords.Add(record);
            }

            await _dbContext.SaveChangesAsync();
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

                for (int i = 0; i < records.Count; i++)
                {
                    var record = records[i];
                    worksheet.Cells[i + 2, 1].Value = record.Date;
                    worksheet.Cells[i + 2, 2].Value = new ESKDNumber { CompanyCode = record.CompanyCode, ClassNumber = new Classifier { Number = record.ClassNumber, Name = record.ClassifierName }, DetailNumber = record.DetailNumber, Version = record.Version }.GetCode();
                    worksheet.Cells[i + 2, 3].Value = record.YASTCode;
                    worksheet.Cells[i + 2, 4].Value = record.Name;
                    worksheet.Cells[i + 2, 5].Value = record.AssemblyNumber;
                    worksheet.Cells[i + 2, 6].Value = record.AssemblyName;
                    worksheet.Cells[i + 2, 7].Value = record.ProductNumber;
                    worksheet.Cells[i + 2, 8].Value = record.ProductName;
                    worksheet.Cells[i + 2, 9].Value = record.FullName;
                }

                await package.SaveAsAsync(new FileInfo(filePath));
            }
        }
    }
}
