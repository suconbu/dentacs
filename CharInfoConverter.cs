using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Linq;

namespace Suconbu.Dentacs
{
    public class CharInfoConverter : IValueConverter
    {
        public static readonly string InvalidValueString = "-";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return CharInfoConvertHelper.ConvertToInfoString(value as string, false) ?? CharInfoConverter.InvalidValueString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    static class CharInfoConvertHelper
    {
        public static string ConvertToInfoString(string str, bool verbose)
        {
            if (string.IsNullOrEmpty(str)) return null;

            var codePoint = char.ConvertToUtf32(str, 0);
            var utf8Bytes = Encoding.UTF8.GetBytes(str);
            var utf8CodeText = string.Join(":", utf8Bytes.Select(c => $"{c:X2}"));
            var utf16beBytes = Encoding.BigEndianUnicode.GetBytes(str);
            var utf16beCodeText = string.Join(":", utf16beBytes.Select(c => $"{c:X2}"));

            return verbose ?
                $"'{str}' | U+{codePoint:X} | {utf16beCodeText} (UTF-16BE) | {utf8CodeText} (UTF-8)" :
                $"'{str}' | U+{codePoint:X} | {utf16beCodeText} | {utf8CodeText}";
        }

        public static string GetUnicodeElement(string str, int index)
        {
            var elements = StringInfo.GetTextElementEnumerator(str);
            while(elements.MoveNext())
            {
                var element = elements.GetTextElement();
                if (elements.ElementIndex <= index && index < (elements.ElementIndex + element.Length))
                {
                    return element;
                }
            }
            return null;
        }
    }
}
