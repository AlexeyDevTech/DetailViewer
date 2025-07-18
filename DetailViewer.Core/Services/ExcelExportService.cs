
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class ExcelExportService : IExcelExportService
    {
        private readonly IDocumentDataService _documentDataService;

        public ExcelExportService(IDocumentDataService documentDataService)
        {
            _documentDataService = documentDataService;
            ExcelPackage.License.SetNonCommercialPersonal("My personal project");
        }

        public async Task ExportToExcelAsync(string filePath)
        {
            var records = await _documentDataService.GetAllRecordsAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Детали");

                AddHeaders(worksheet);
                AddData(worksheet, records);
                ApplyStyling(worksheet, records.Count);

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                await package.SaveAsAsync(new FileInfo(filePath));
            }
        }

        private void AddHeaders(ExcelWorksheet worksheet)
        {
            string[] headers = {
                "Дата", "ЕСКД номер", "ЯСТ код", "Имя", "Номер сборки",
                "Имя сборки", "Номер продукта", "Имя продукта", "ФИО",
                "Наименование классификатора", "Номер классификатора", "Описание классификатора"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }
        }

        private void AddData(ExcelWorksheet worksheet, List<DocumentDetailRecord> records)
        {
            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                var row = i + 2;

                worksheet.Cells[row, 1].Value = record.Date.ToShortDateString();
                worksheet.Cells[row, 2].Value = record.ESKDNumber?.FullCode;
                worksheet.Cells[row, 3].Value = record.YASTCode;
                worksheet.Cells[row, 4].Value = record.Name;
                worksheet.Cells[row, 5].Value = record.AssemblyNumber;
                worksheet.Cells[row, 6].Value = record.AssemblyName;
                worksheet.Cells[row, 7].Value = record.ProductNumber;
                worksheet.Cells[row, 8].Value = record.ProductName;
                worksheet.Cells[row, 9].Value = record.FullName;
                worksheet.Cells[row, 10].Value = record.ESKDNumber?.ClassNumber?.Name;
                worksheet.Cells[row, 11].Value = record.ESKDNumber?.ClassNumber?.Number;
                worksheet.Cells[row, 12].Value = record.ESKDNumber?.ClassNumber?.Description;
            }
        }

        private void ApplyStyling(ExcelWorksheet worksheet, int recordCount)
        {
            using (var range = worksheet.Cells[1, 1, 1, 12])
            {
                range.Style.Font.Bold = true;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            using (var range = worksheet.Cells[1, 1, recordCount + 1, 12])
            {
                range.Style.Font.Name = "Calibri";
                range.Style.Font.Size = 11;
                range.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                range.Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                range.Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);
            }
        }
    }
}
