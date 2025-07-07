namespace DetailViewer.Core.Models
{
    public class AppSettings
    {
        public DataSourceType CurrentDataSourceType { get; set; } = DataSourceType.Excel;
        public string DefaultExcelFilePath { get; set; }
        public string GoogleSheetsCredentialsPath { get; set; }
        public string LastUsedExcelFilePath { get; set; }
        public string LastUsedGoogleSheetId { get; set; }
        public string LastUsedGoogleSheetName { get; set; }
        public string FirmwareCode { get; set; }
        // Add other settings as needed
    }
}
