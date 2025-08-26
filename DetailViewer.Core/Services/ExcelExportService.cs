using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class ExcelExportService : IExcelExportService
    {
        private readonly ILogger _logger;

        public ExcelExportService(ILogger logger)
        {
            _logger = logger;
            ExcelPackage.License.SetNonCommercialPersonal("My personal project");
        }

        public async Task ExportToExcelAsync(string filePath, List<DocumentDetailRecord> records)
        {
            _logger.Log($"Exporting {records.Count()} records to Excel: {filePath}");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Export");

                worksheet.Cells[1, 1].Value = "Date";
                worksheet.Cells[1, 2].Value = "ESKDNumber";
                worksheet.Cells[1, 3].Value = "YASTCode";
                worksheet.Cells[1, 4].Value = "Name";

                int row = 2;
                foreach (var record in records)
                {
                    worksheet.Cells[row, 1].Value = record.Date.ToShortDateString();
                    worksheet.Cells[row, 2].Value = record.ESKDNumber?.FullCode ?? "";
                    worksheet.Cells[row, 3].Value = record.YASTCode;
                    worksheet.Cells[row, 4].Value = record.Name;
                    row++;
                }

                await package.SaveAsAsync(new FileInfo(filePath));
            }
        }
    }
}
