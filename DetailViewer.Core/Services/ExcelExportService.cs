using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class ExcelExportService : IExcelExportService
    {
        private readonly IDocumentRecordService _documentRecordService;

        public ExcelExportService(IDocumentRecordService documentRecordService)
        {
            _documentRecordService = documentRecordService;
        }

        public async Task ExportToExcelAsync(string filePath, List<DocumentDetailRecord> records)
        {
            if (records == null)
            {
                records = await _documentRecordService.GetAllRecordsAsync();
            }

            ExcelPackage.License.SetNonCommercialPersonal("My personal project");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Детали");

                // Заголовки
                worksheet.Cells[1, 1].Value = "Наименование";
                worksheet.Cells[1, 2].Value = "Обозначение";
                worksheet.Cells[1, 3].Value = "Код ЯАСП";
                worksheet.Cells[1, 4].Value = "Дата";
                worksheet.Cells[1, 5].Value = "Автор";

                // Данные
                for (int i = 0; i < records.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = records[i].Name;
                    worksheet.Cells[i + 2, 2].Value = records[i].ESKDNumber.FullCode;
                    worksheet.Cells[i + 2, 3].Value = records[i].YASTCode;
                    worksheet.Cells[i + 2, 4].Value = records[i].Date.ToShortDateString();
                    worksheet.Cells[i + 2, 5].Value = records[i].FullName;
                }

                await package.SaveAsAsync(new FileInfo(filePath));
            }
        }
    }
}