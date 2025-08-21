namespace DetailViewer.Core.Models
{
    public class AppSettings
    {
        public string DatabasePath { get; set; } = "data.db";
        public string LocalDatabasePath { get; set; } = "temp.db";
        public string DefaultCompanyCode { get; set; } = "ДТМЛ";
        public bool RunInTray { get; set; } = true;
        public System.DateTime LastSyncTimestamp { get; set; }
        public string ApiUrl { get; set; } = "http://localhost:5013";
    }
}
 