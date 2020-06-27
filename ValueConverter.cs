using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace Suconbu.Dentacs
{
    class DecResultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            // Show value as-is if fails
            if (!ResultConvertHelper.ConvertTo(value.ToString(), 10, out var result)) return value.ToString();
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class HexResultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            if (!ResultConvertHelper.ConvertTo(value.ToString(), 16, out var result)) return "--";
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class BinResultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            if (!ResultConvertHelper.ConvertTo(value.ToString(), 2, out var result)) return "--";
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    static class ResultConvertHelper
    {
        public static bool ConvertTo(string value, int radix, out string result)
        {
            result = null;

            if (!decimal.TryParse(value.ToString(), out var number)) return false;

            if (radix == 10)
            {
                result = $"{number:#,0.################}";
            }
            else if (radix == 16 || radix == 2)
            {
                var fixedNumber = (long)Math.Floor(number);

                int bitCount =
                        (fixedNumber < -0x10000 || 0xFFFFFFFF < fixedNumber) ? 64 :
                        (fixedNumber < 0 || 0xFFFF < fixedNumber) ? 32 :
                        16;
                var digits = System.Convert.ToString(fixedNumber, radix).ToUpper();
                int digitCount = (radix == 16) ? (bitCount / 4) : bitCount;

                if(digits.Length <= digitCount)
                {
                    digits = digits.PadLeft(digitCount, '0');
                }
                else
                {
                    digits = digits.Substring(digits.Length - digitCount, digitCount);
                }

                // Insert a space each 4 digits
                var parts = new List<string>();
                for (int i = 0; i < digitCount; i += 4)
                {
                    parts.Add(digits.Substring(i, 4));
                }

                result = string.Join(' ', parts);
            }
            else
            {
                throw new NotSupportedException();
            }
            return true;
        }
    }
}
