using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Suconbu.Dentacs
{
    public class ResultConverter : IValueConverter
    {
        public static readonly string InvalidValueString = "--";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return null;

            int radix = int.Parse((string)parameter);

            if (!decimal.TryParse(value.ToString(), out var number))
            {
                return (radix == 10) ? value.ToString() : ResultConverter.InvalidValueString;
            }

            return ResultConvertHelper.ConvertToResultString(
                number, radix, ResultConvertHelper.Styles.Separator);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    static class ResultConvertHelper
    {
        [Flags]
        public enum Styles
        {
            Prefix = 0x1,
            Separator = 0x2
        }

        static readonly Dictionary<int, string> kPrefixes = new Dictionary<int, string>() { { 16, "0x" }, { 2, "0b" } };

        public static string ConvertToResultString(decimal value, int radix, Styles styles)
        {
            var result = new StringBuilder();

            if (styles.HasFlag(Styles.Prefix))
            {
                if (kPrefixes.TryGetValue(radix, out var prefix)) result.Append(prefix);
            }

            if (radix == 10)
            {
                result.Append(styles.HasFlag(Styles.Separator) ?
                    $"{value:#,0.################}" :
                    $"{value:0.################}");
            }
            else if (radix == 16 || radix == 2)
            {
                var integer = (Int64)Math.Clamp(value, Int64.MinValue, Int64.MaxValue);
                int bitCount =
                        (integer < -0x10000 || 0xFFFFFFFF < integer) ? 64 :
                        (integer < 0 || 0xFFFF < integer) ? 32 :
                        16;
                int digitCount = (radix == 16) ? (bitCount / 4) : bitCount;
                var digits = System.Convert.ToString(integer, radix).ToUpper();
                if (digits.Length <= digitCount)
                {
                    digits = digits.PadLeft(digitCount, '0');
                }
                else
                {
                    digits = digits.Substring(digits.Length - digitCount, digitCount);
                }

                if (styles.HasFlag(Styles.Separator))
                {
                    int brInterval = 32;
                    int spInterval = 4;
                    for (int i = 0; i < digits.Length; i++)
                    {
                        if (0 < i && (digits.Length - i) % brInterval == 0)
                        {
                            result.AppendLine();
                        }
                        else if (0 < i && (digits.Length - i) % spInterval == 0)
                        {
                            result.Append(' ');
                        }
                        result.Append(digits[i]);
                    }
                }
                else
                {
                    result.Append(digits);
                }
            }
            else
            {
                throw new NotSupportedException($"radix:{radix} must be 10, 16 or 2");
            }
            return result.ToString();
        }
    }
}
