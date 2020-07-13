using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Linq;
using System.Windows;

namespace Suconbu.Dentacs
{
    public class CharInfoConverter : IMultiValueConverter
    {
        public static readonly string InvalidValueString = "-";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = CharInfoConverter.InvalidValueString;
            if (!(values[0] is string currentText)) return result;
            if (!(values[1] is int selectionLength)) return result;
            return (selectionLength == 0) ?
                (CharInfoConvertHelper.ConvertToElementInfoString(currentText, true) ?? result) :
                (CharInfoConvertHelper.ConvertToElementLengthInfoString(currentText, true) ?? result);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    static class CharInfoConvertHelper
    {
        public static string ConvertToElementInfoString(string str, bool shorten)
        {
            if (string.IsNullOrEmpty(str)) return null;

            var si = new StringInfo(str);
            var codePoints = new List<string>();
            var utf8Codes = new List<string>();
            var utf16Codes = new List<string>();
            for (int i = 0; i < si.LengthInTextElements; i++)
            {
                var element = si.SubstringByTextElements(i, 1);
                codePoints.Add($"U+{char.ConvertToUtf32(element, 0):X}");
                utf8Codes.Add(string.Join(":", Encoding.UTF8.GetBytes(element).Select(c => $"{c:X2}")));
                utf16Codes.Add(string.Join(":", Encoding.BigEndianUnicode.GetBytes(element).Select(c => $"{c:X2}")));
            }
            var codePointText = string.Join(" ", codePoints);
            var utf8CodeText = string.Join(" ", utf8Codes);
            var utf16beCodeText = string.Join(" ", utf16Codes);
            var s = CharInfoConvertHelper.ConvertToPrintable(str);
            s = CharInfoConvertHelper.Ellipsize(s, 20);
            return shorten ?
                $"'{s}' | {codePointText} | {utf16beCodeText} (16BE) | {utf8CodeText} (8)" :
                $"'{s}' | {codePointText} | {utf16beCodeText} (UTF-16BE) | {utf8CodeText} (UTF-8)";
        }

        public static string ConvertToElementLengthInfoString(string str, bool shorten)
        {
            if (string.IsNullOrEmpty(str)) return null;

            var si = new StringInfo(str);
            var elementCount = si.LengthInTextElements;
            var utf8Count = Encoding.UTF8.GetBytes(str).Length;
            var utf16Count = Encoding.BigEndianUnicode.GetBytes(str).Length;
            var s = CharInfoConvertHelper.ConvertToPrintable(str);
            s = CharInfoConvertHelper.Ellipsize(s, 20);
            return shorten ?
                $"'{s}' | {elementCount} elements | {utf16Count} bytes (16BE) | {utf8Count} bytes (8)" :
                $"'{s}' | {elementCount} elements | {utf16Count} bytes (UTF-16BE) | {utf8Count} bytes (UTF-8)";
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

        public static string ConvertToPrintable(string str)
        {
            return str
                .Replace("\a", "\\a")
                .Replace("\b", "\\b")
                .Replace("\f", "\\f")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\v", "\\v");
        }

        public static string Ellipsize(string str, int elementCount)
        {
            var si = new StringInfo(str);
            var ellipsis = (elementCount < si.LengthInTextElements) ? "..." : string.Empty;
            var count = Math.Min(si.LengthInTextElements, elementCount);
            return si.SubstringByTextElements(0, count) + ellipsis;
        }
    }
}
