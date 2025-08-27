namespace DetailViewer.Api.Models
{
    /// <summary>
    /// Определяет тип источника данных.
    /// </summary>
    public enum DataSourceType
    {
        /// <summary>
        /// Источник данных - файл Excel.
        /// </summary>
        Excel,
        /// <summary>
        /// Источник данных - Google Sheets.
        /// </summary>
        GoogleSheets
    }
}