using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class ExcelDocumentDataService : IDocumentDataService
    {
        private readonly ILogger _logger;

        public ExcelDocumentDataService(ILogger logger)
        {
            _logger = logger;
            ExcelPackage.License.SetNonCommercialPersonal("Alexey");
        }

        public async Task<List<DocumentRecord>> ReadRecordsAsync(string filePath)
        {
            await Task.Yield();
            var records = new List<DocumentRecord>();

            if (!File.Exists(filePath))
            {
                _logger.LogError($"Excel file not found: {filePath}");
                throw new FileNotFoundException("Excel file not found", filePath);
            }

            try
            {
                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets[3]; // Assuming data is on the 4th worksheet

                if (worksheet == null)
                {
                    _logger.LogWarning($"Worksheet at index 3 not found in {filePath}.");
                    return records;
                }

                int rowCount = worksheet.Dimension?.Rows ?? 0;
                int colCount = worksheet.Dimension?.Columns ?? 0;

                if (rowCount < 2)
                {
                    _logger.LogInformation($"No data rows found in {filePath} (excluding header).");
                    return records; // Skip header
                }

                // Performance optimization: Read all data into an array
                var data = worksheet.Cells[1, 1, rowCount, colCount].LoadFromText();

                for (int row = 1; row < data.GetLength(0); row++) // Start from 1 to skip header row in data array
                {
                    try
                    {
                        // Ensure row has enough columns before accessing
                        if (data.GetLength(1) < 10)
                        {
                            _logger.LogWarning($"Row {row + 1} in Excel file has insufficient columns. Skipping.");
                            continue;
                        }

                        DateTime date = default;
                        string eskdNumber = data[row, 2]?.ToString(); // Column 3 in Excel (index 2 in 0-based array)
                        string yastCode = data[row, 3]?.ToString(); // Column 4 in Excel
                        string name = data[row, 4]?.ToString(); // Column 5 in Excel
                        string assemblyNumber = data[row, 5]?.ToString(); // Column 6 in Excel
                        string assemblyName = data[row, 6]?.ToString(); // Column 7 in Excel
                        string productNumber = data[row, 7]?.ToString(); // Column 8 in Excel
                        string productName = data[row, 8]?.ToString(); // Column 9 in Excel
                        string fullName = data[row, 9]?.ToString(); // Column 10 in Excel

                        // Attempt to parse Date
                        if (!DateTime.TryParse(data[row, 1]?.ToString(), out date)) // Column 2 in Excel (index 1 in 0-based array)
                        {
                            _logger.LogWarning($"Could not parse Date at row {row + 1}, column 2. Using default value.");
                        }

                        if (!string.IsNullOrEmpty(eskdNumber))
                        {
                            try
                            {
                                var record = new DocumentRecord
                                {
                                    Date = date,
                                    ESKDNumber = new ESKDNumber().SetCode(eskdNumber),
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
                                _logger.LogError($"Validation error for ESKDNumber at row {row + 1}: {eskdNumber}. {ex.Message}", ex);
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"ESKDNumber is empty at row {row + 1}. Skipping record.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An unexpected error occurred while processing record at row {row + 1}: {ex.Message}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading Excel file {filePath}: {ex.Message}", ex);
            }

            return records;
        }

        public async Task WriteRecordsAsync(string filePath, List<DocumentRecord> records)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            ExcelPackage package = null;

            try
            {
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

                // Write headers if file is new or empty
                if (!fileInfo.Exists || worksheet.Dimension == null || worksheet.Dimension.Rows == 0)
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
                if (worksheet.Dimension != null && worksheet.Dimension.Rows > 1) // Ensure there's data to sort
                {
                    var range = worksheet.Cells[2, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns];
                    range.Sort(1); // Sort by second column (ESKDNumber)
                }

                await package.SaveAsAsync(fileInfo);
                _logger.LogInformation($"Successfully wrote {records.Count} records to Excel file: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error writing to Excel file {filePath}: {ex.Message}", ex);
                throw; // Re-throw to propagate the error
            }
            finally
            {
                package?.Dispose();
            }
        }
    }

    public class GoogleSheetsDocumentDataService : IDocumentDataService
    {
        private readonly SheetsService _sheetsService;
        private readonly string _applicationName = "ExcelDataProcessing";
        private readonly ILogger _logger;

        public GoogleSheetsDocumentDataService(string credentialsPath, ILogger logger)
        {
            _logger = logger;
            try
            {
                using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);
                var credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);

                _sheetsService = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = _applicationName
                });
                _logger.LogInformation("GoogleSheetsDocumentDataService initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initializing GoogleSheetsDocumentDataService: {ex.Message}", ex);
                throw; // Re-throw to propagate the error
            }
        }

        public async Task<List<DocumentRecord>> ReadRecordsAsync(string spreadsheetId)
        {
            var records = new List<DocumentRecord>();
            var range = "Sheet1!A1:I";

            try
            {
                var request = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
                var response = await request.ExecuteAsync();

                var values = response.Values;
                if (values == null || values.Count < 2)
                {
                    _logger.LogInformation($"No data found in Google Sheet {spreadsheetId} or only header row.");
                    return records;
                }

                for (int i = 1; i < values.Count; i++)
                {
                    var row = values[i];
                    if (row.Count < 9)
                    {
                        _logger.LogWarning($"Row {i + 1} in Google Sheet has insufficient columns. Skipping.");
                        continue;
                    }

                    try
                    {
                        DateTime date = default;
                        if (!DateTime.TryParse(row[0]?.ToString(), out date))
                        {
                            _logger.LogWarning($"Could not parse Date at row {i + 1}, column 1. Using default value.");
                        }

                        string eskdNumber = row[1]?.ToString();
                        if (!string.IsNullOrEmpty(eskdNumber))
                        {
                            var record = new DocumentRecord
                            {
                                Date = date,
                                ESKDNumber = new ESKDNumber().SetCode(eskdNumber),
                                YASTCode = row[2]?.ToString(),
                                Name = row[3]?.ToString(),
                                AssemblyNumber = row[4]?.ToString(),
                                AssemblyName = row[5]?.ToString(),
                                ProductNumber = row[6]?.ToString(),
                                ProductName = row[7]?.ToString(),
                                FullName = row[8]?.ToString()
                            };
                            records.Add(record);
                        }
                        else
                        {
                            _logger.LogWarning($"ESKDNumber is empty at row {i + 1}. Skipping record.");
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogError($"Validation error for ESKDNumber at row {i + 1}: {row[1]?.ToString()}. {ex.Message}", ex);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An unexpected error occurred while processing record at row {i + 1} from Google Sheet: {ex.Message}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading Google Sheet {spreadsheetId}: {ex.Message}", ex);
                throw; // Re-throw to propagate the error
            }

            _logger.LogInformation($"Successfully read {records.Count} records from Google Sheet {spreadsheetId}.");
            return records;
        }

        public async Task WriteRecordsAsync(string spreadsheetId, List<DocumentRecord> records)
        {
            try
            {
                var valueRange = new ValueRange { Values = new List<IList<object>> { GetHeaders() } };

                foreach (var record in records)
                {
                    valueRange.Values.Add(new List<object>
                    {
                        record.Date.ToString("yyyy-MM-dd"),
                        record.ESKDNumber.GetCode(), // Use GetCode() for ESKDNumber
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
                _logger.LogInformation($"Successfully wrote {records.Count} records to Google Sheet {spreadsheetId}.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error writing to Google Sheet {spreadsheetId}: {ex.Message}", ex);
                throw; // Re-throw to propagate the error
            }
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