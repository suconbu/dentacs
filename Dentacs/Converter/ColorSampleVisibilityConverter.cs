using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Suconbu.Dentacs
{
    public sealed class ColorSampleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = false;
            var input = value as string;
            if (!string.IsNullOrEmpty(input) && input.StartsWith("\'"))
            {
                visible = ColorUtility.TryParseColor(input.Trim('\''), out var _);
            }
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
