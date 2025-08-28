namespace DetailViewer.Core.Models
{
    /// <summary>
    /// Определяет типы источников данных для импорта.
    /// </summary>
    public enum DataSourceType
    {
        /// <summary>
        /// Локальный файл Microsoft Excel.
        /// </summary>
        Excel,

        /// <summary>
        /// Онлайн-таблица Google Sheets.
        /// </summary>
        GoogleSheets
    }
}
