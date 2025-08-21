using System;
using System.Globalization;
using System.Windows.Data;

namespace DetailViewer.Modules.Explorer.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() is not string enumValue || parameter?.ToString() is not string targetValue)
            {
                return false;
            }

            return enumValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolValue || !boolValue)
            {
                return null;
            }

            if (parameter?.ToString() is not string parameterString)
            {
                return null;
            }

            return Enum.Parse(targetType, parameterString);
        }
    }
}