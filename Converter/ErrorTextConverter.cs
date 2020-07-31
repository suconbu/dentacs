using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Linq;
using System.Windows;

namespace Suconbu.Dentacs
{
    public class ErrorTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string errorText)) return null;

            var tokens = errorText.Split(':', 2);
            var errorType = tokens[0];
            if (!string.IsNullOrEmpty(errorType))
            {
                errorType =
                    Application.Current.TryFindResource($"Error.{errorType}") as string ??
                    Application.Current.TryFindResource($"Error.UnknownError") as string;
            }
            var errorObject = (2 <= tokens.Length) ? $": {tokens[1]}" : string.Empty;
            return "😟" + errorType + errorObject;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
