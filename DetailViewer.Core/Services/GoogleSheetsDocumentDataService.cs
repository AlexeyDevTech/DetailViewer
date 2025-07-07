using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace DetailViewer.Core.Services
{
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

        public async Task<List<DocumentRecord>> ReadRecordsAsync(string spreadsheetId, string sheetName)
        {
            var records = new List<DocumentRecord>();
            var range = $"{sheetName}!A1:I";

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

        public async Task WriteRecordsAsync(string spreadsheetId, List<DocumentRecord> records, string sheetName = null)
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

                var request = _sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, $"{sheetName}!A1");
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