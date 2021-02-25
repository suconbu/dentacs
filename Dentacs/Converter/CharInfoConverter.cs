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
        public static readonly string InvalidValueString = string.Empty;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = CharInfoConverter.InvalidValueString;
            if (!(values[0] is string currentText)) return result;
            if (!(values[1] is int selectionLength)) return result;
            return (selectionLength == 0) ?
                (CharInfoConvertHelper.ConvertToElementInfoString(currentText) ?? result) :
                (CharInfoConvertHelper.ConvertToElementLengthInfoString(currentText) ?? result);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    static class CharInfoConvertHelper
    {
        public static string ConvertToElementInfoString(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;

            var ci = new CharInfo(str);
            var s = CharInfoConvertHelper.Ellipsize(ci.PrintableText, 20);
            return $"'{s}' | {ci.CodePointText} | {ci.Utf16BECodeText} (16BE) | {ci.Utf8CodeText} (8)";
        }

        public static string ConvertToElementLengthInfoString(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;

            var ci = new CharInfo(str);
            var s = CharInfoConvertHelper.Ellipsize(ci.PrintableText, 20);
            return $"'{s}' | {ci.ElementCount} elements | {ci.Utf16Count} bytes (16BE) | {ci.Utf8Count} bytes (8)";
        }

        public static string ConvertToElementInfoTableString(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;

            var tt = new TextTable("Char", "Unicode", "UTF-16BE", "UTF-8");
            var si = new StringInfo(str);
            for (int i = 0; i < si.LengthInTextElements; i++)
            {
                var element = si.SubstringByTextElements(i, 1);
                var ci = new CharInfo(element);
                tt.Rows.Add(new[] { ci.PrintableText, ci.CodePointText, ci.Utf16BECodeText, ci.Utf8CodeText });
            }
            return tt.ToMarkdownString();
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

    class CharInfo
    {
        public string Text { get; }
        public string PrintableText { get; }
        public string CodePointText { get; }
        public string Utf16BECodeText { get; }
        public string Utf8CodeText { get; }
        public int ElementCount { get; }
        public int Utf16Count { get; }
        public int Utf8Count { get; }

        public CharInfo(string input)
        {
            var si = new StringInfo(input);
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
            this.Text = input;
            this.PrintableText = CharInfoConvertHelper.ConvertToPrintable(input);
            this.CodePointText = string.Join(" ", codePoints);
            this.Utf8CodeText = string.Join(" ", utf8Codes);
            this.Utf16BECodeText = string.Join(" ", utf16Codes);
            this.ElementCount = si.LengthInTextElements;
            this.Utf8Count = Encoding.UTF8.GetBytes(input).Length;
            this.Utf16Count = Encoding.BigEndianUnicode.GetBytes(input).Length;
        }
    }

    class TextTable
    {
        public List<string> Columns { get; } = new List<string>();
        public List<string[]> Rows { get; } = new List<string[]>();

        public TextTable(params string[] columns)
        {
            this.Columns.AddRange(columns);
        }

        public string ToMarkdownString()
        {
            var widthOfColumns = new int[this.Columns.Count];
            for (int i = 0; i < widthOfColumns.Length; i++)
            {
                widthOfColumns[i] = Math.Max(this.Rows.Max(r => r[i].Length), this.Columns[i].Length);
            }
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(" | ", this.Columns.Select((c, i) => string.Format($"{{0,-{widthOfColumns[i]}}}", c))));
            sb.AppendLine(string.Join("-|-", this.Columns.Select((c, i) => new string('-', widthOfColumns[i]))));
            foreach (var row in this.Rows)
            {
                sb.AppendLine(string.Join(" | ", row.Select((r, i) => string.Format($"{{0,-{widthOfColumns[i]}}}", r))));
            }
            return sb.ToString();
        }
    }
}
