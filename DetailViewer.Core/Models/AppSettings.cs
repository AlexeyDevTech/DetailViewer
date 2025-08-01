namespace DetailViewer.Core.Models
{
    public class AppSettings
    {
        public string DatabasePath { get; set; }
        public string LocalDatabasePath { get; set; }
        public string DefaultCompanyCode { get; set; } = "ДТМЛ";
        public bool RunInTray { get; set; } = true;
        public System.DateTime LastSyncTimestamp { get; set; }
    }
}
