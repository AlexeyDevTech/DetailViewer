namespace DetailViewer.Core.Models
{
    public class AppSettings
    {
        public string DefaultCompanyCode { get; set; } = "ДТМЛ";
        public bool RunInTray { get; set; } = true;
        public string ApiUrl { get; set; } = "http://localhost:5013";
    }
}