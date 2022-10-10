using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Suconbu.Dentacs
{
    public class ColorSampleConverter : IValueConverter
    {
        public static readonly string InvalidValueString = string.Empty;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var input = value.ToString();
            SolidColorBrush brush = null;
            if (!string.IsNullOrEmpty(input) && input.StartsWith("\'"))
            {
                if (ColorUtility.TryParseColor(input.Trim('\''), out var color))
                {
                    brush = new SolidColorBrush(color);
                }
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
