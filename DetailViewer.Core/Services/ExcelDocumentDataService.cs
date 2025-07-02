using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DetailViewer.Core.Services
{
    public class ExcelDocumentDataService : IDocumentDataService
    {
        public ExcelDocumentDataService()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Alexey");
        }

        public async Task<List<DocumentRecord>> ReadRecordsAsync(string filePath)
        {
            await Task.Yield();
            var records = new List<DocumentRecord>();

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Excel file not found", filePath);

            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];

            int rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount < 2) return records; // Пропускаем заголовок

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    DateTime date = default;
                    string eskdNumber = null;
                    string yastCode = null;
                    string name = null;
                    string assemblyNumber = null;
                    string assemblyName = null;
                    string productNumber = null;
                    string productName = null;
                    string fullName = null;

                    try { date = worksheet.Cells[row, 1].GetValue<DateTime>(); }
                    catch (Exception ex) { Console.WriteLine($"Error reading Date at row {row}, column 1: {ex.Message}"); continue; }

                    try { eskdNumber = worksheet.Cells[row, 2].GetValue<string>(); }
                    catch (Exception ex) { Console.WriteLine($"Error reading ESKDNumber at row {row}, column 2: {ex.Message}"); continue; }

                    try { yastCode = worksheet.Cells[row, 3].GetValue<string>(); }
                    catch (Exception ex) { Console.WriteLine($"Error reading YASTCode at row {row}, column 3: {ex.Message}"); continue; }

                    try { name = worksheet.Cells[row, 4].GetValue<string>(); }
                    catch (Exception ex) { Console.WriteLine($"Error reading Name at row {row}, column 4: {ex.Message}"); continue; }

                    try { assemblyNumber = worksheet.Cells[row, 5].GetValue<string>(); }
                    catch (Exception ex) { Console.WriteLine($"Error reading AssemblyNumber at row {row}, column 5: {ex.Message}"); continue; }

                    try { assemblyName = worksheet.Cells[row, 6].GetValue<string>(); }
                    catch (Exception ex) { Console.WriteLine($"Error reading AssemblyName at row {row}, column 6: {ex.Message}"); continue; }

                    try { productNumber = worksheet.Cells[row, 7].GetValue<string>(); }
                    catch (Exception ex) { Console.WriteLine($"Error reading ProductNumber at row {row}, column 7: {ex.Message}"); continue; }

                    try { productName = worksheet.Cells[row, 8].GetValue<string>(); }
                    catch (Exception ex) { Console.WriteLine($"Error reading ProductName at row {row}, column 8: {ex.Message}"); continue; }

                    try { fullName = worksheet.Cells[row, 9].GetValue<string>(); }
                    catch (Exception ex) { Console.WriteLine($"Error reading FullName at row {row}, column 9: {ex.Message}"); continue; }

                    var record = new DocumentRecord
                    {
                        Date = date,
                        ESKDNumber = new ESKDNumber().SetCode(worksheet.Cells[row, 2].GetValue<string>()),
                        YASTCode = yastCode,
                        Name = name,
                        AssemblyNumber = assemblyNumber,
                        AssemblyName = assemblyName,
                        ProductNumber = productNumber,
                        ProductName = productName,
                        FullName = fullName
                    };
                    records.Add(record);
                }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine($"Error processing record at row {row}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"An unexpected error occurred at row {row}: {ex.Message}");
                }
            }

            return records;
        }

        public async Task WriteRecordsAsync(string filePath, List<DocumentRecord> records)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            ExcelPackage package;

            if (fileInfo.Exists)
            {
                // Load existing file
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
                package = new ExcelPackage(stream);
            }
            else
            {
                // Create new file
                package = new ExcelPackage();
            }

            var worksheet = package.Workbook.Worksheets.FirstOrDefault() ?? package.Workbook.Worksheets.Add("Sheet1");

            // Write headers if file is new
            if (!fileInfo.Exists)
            {
                worksheet.Cells[1, 1].Value = "Дата";
                worksheet.Cells[1, 2].Value = "Децимальный номер ЕСКД";
                worksheet.Cells[1, 3].Value = "Код классификатора ЯСТ";
                worksheet.Cells[1, 4].Value = "Наименование";
                worksheet.Cells[1, 5].Value = "Применяемость (номер СБ)";
                worksheet.Cells[1, 6].Value = "Наименование (СБОРКА)";
                worksheet.Cells[1, 7].Value = "Применяемость (номер изделия)";
                worksheet.Cells[1, 8].Value = "Наименование (ИЗДЕЛИЕ)";
                worksheet.Cells[1, 9].Value = "ФИО";
            }

            // Find the last used row
            int startRow = worksheet.Dimension?.Rows + 1 ?? 2;

            // Write new records
            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                int row = startRow + i;
                worksheet.Cells[row, 1].Value = record.Date;
                worksheet.Cells[row, 2].Value = record.ESKDNumber.GetCode();
                worksheet.Cells[row, 3].Value = record.YASTCode;
                worksheet.Cells[row, 4].Value = record.Name;
                worksheet.Cells[row, 5].Value = record.AssemblyNumber;
                worksheet.Cells[row, 6].Value = record.AssemblyName;
                worksheet.Cells[row, 7].Value = record.ProductNumber;
                worksheet.Cells[row, 8].Value = record.ProductName;
                worksheet.Cells[row, 9].Value = record.FullName;
            }

            // Sort by ESKDNumber (column 2)
            if (worksheet.Dimension != null)
            {
                var range = worksheet.Cells[2, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns];
                range.Sort(1); // Sort by second column (ESKDNumber)
            }

            await package.SaveAsAsync(fileInfo);
        }
    }

    public class GoogleSheetsDocumentDataService : IDocumentDataService
    {
        private readonly SheetsService _sheetsService;
        private readonly string _applicationName = "ExcelDataProcessing";

        public GoogleSheetsDocumentDataService(string credentialsPath)
        {
            using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);
            var credential = GoogleCredential.FromStream(stream)
                .CreateScoped(SheetsService.Scope.Spreadsheets);

            _sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName
            });
        }

        public async Task<List<DocumentRecord>> ReadRecordsAsync(string spreadsheetId)
        {
            var records = new List<DocumentRecord>();
            var range = "Sheet1!A1:I";

            var request = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = await request.ExecuteAsync();

            var values = response.Values;
            if (values == null || values.Count < 2) return records;

            for (int i = 1; i < values.Count; i++)
            {
                var row = values[i];
                if (row.Count < 9) continue;

                var record = new DocumentRecord
                {
                    Date = DateTime.Parse(row[0].ToString()),
                    ESKDNumber = new ESKDNumber().SetCode(row[1].ToString()),
                    YASTCode = row[2].ToString(),
                    Name = row[3].ToString(),
                    AssemblyNumber = row[4].ToString(),
                    AssemblyName = row[5].ToString(),
                    ProductNumber = row[6].ToString(),
                    ProductName = row[7].ToString(),
                    FullName = row[8].ToString()
                };
                records.Add(record);
            }

            return records;
        }

        public async Task WriteRecordsAsync(string spreadsheetId, List<DocumentRecord> records)
        {
            var valueRange = new ValueRange { Values = new List<IList<object>> { GetHeaders() } };

            foreach (var record in records)
            {
                valueRange.Values.Add(new List<object>
                {
                    record.Date.ToString("yyyy-MM-dd"),
                    record.ESKDNumber,
                    record.YASTCode,
                    record.Name,
                    record.AssemblyNumber,
                    record.AssemblyName,
                    record.ProductNumber,
                    record.ProductName,
                    record.FullName
                });
            }

            var request = _sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, "Sheet1!A1");
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            await request.ExecuteAsync();
        }

        private IList<object> GetHeaders()
        {
            return new List<object>
            {
                "Дата",
                "Децимальный номер ЕСКД",
                "Код классификатора ЯСТ",
                "Наименование",
                "Применяемость (номер СБ)",
                "Наименование (СБОРКА)",
                "Применяемость (номер изделия)",
                "Наименование (ИЗДЕЛИЕ)",
                "ФИО"
            };
        }
    }
}