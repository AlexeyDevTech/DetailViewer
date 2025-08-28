using System;
using System.Globalization;
using System.Windows.Data;

namespace DetailViewer.Modules.Dialogs.Converters
{
    /// <summary>
    /// Конвертер, инвертирующий логическое значение (true становится false, false становится true).
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Инвертирует логическое значение.
        /// </summary>
        /// <param name="value">Исходное значение (ожидается bool).</param>
        /// <param name="targetType">Целевой тип.</param>
        /// <param name="parameter">Параметр конвертера (не используется).</param>
        /// <param name="culture">Информация о культуре (не используется).</param>
        /// <returns>Инвертированное логическое значение или исходное значение, если оно не является bool.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }

        /// <summary>
        /// Инвертирует логическое значение обратно.
        /// </summary>
        /// <param name="value">Исходное значение (ожидается bool).</param>
        /// <param name="targetType">Целевой тип.</param>
        /// <param name="parameter">Параметр конвертера (не используется).</param>
        /// <param name="culture">Информация о культуре (не используется).</param>
        /// <returns>Инвертированное логическое значение или исходное значение, если оно не является bool.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }
    }
}