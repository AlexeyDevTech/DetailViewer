namespace DetailViewer.Api.Models
{
    public class AppSettings
    {
        /// <summary>
        /// Получает или устанавливает путь к основной базе данных.
        /// </summary>
        public string DatabasePath { get; set; } = "data.db";
        /// <summary>
        /// Получает или устанавливает путь к локальной базе данных (кэшу).
        /// </summary>
        public string LocalDatabasePath { get; set; } = "temp.db";
        /// <summary>
        /// Получает или устанавливает код компании по умолчанию.
        /// </summary>
        public string DefaultCompanyCode { get; set; } = "ДТМЛ";
        /// <summary>
        /// Получает или устанавливает значение, указывающее, должно ли приложение запускаться в системном трее.
        /// </summary>
        public bool RunInTray { get; set; } = true;
        /// <summary>
        /// Получает или устанавливает временную метку последней синхронизации.
        /// </summary>
        public System.DateTime LastSyncTimestamp { get; set; }
    }
}
