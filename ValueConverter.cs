using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace Suconbu.Dentacs
{
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
            var fixedNumber = (long)Math.Floor(number);

            int bitCount =
                    (fixedNumber < -0x10000 || 0xFFFFFFFF < fixedNumber) ? 64 :
                    (fixedNumber < 0 || 0xFFFF < fixedNumber) ? 32 :
                    16;

            if (radix == 16)
            {
                var hex = System.Convert.ToString(fixedNumber, 16);
                int digitCount = bitCount / 4;
                if(hex.Length <= digitCount)
                {
                    hex = hex.PadLeft(digitCount, '0');
                }
                else
                {
                    hex = hex.Substring(hex.Length - digitCount, digitCount);
                }
                result = $"0x{hex}";
            }
            else if (radix == 2)
            {
                var bin = System.Convert.ToString(fixedNumber, 2);
                if (bin.Length <= bitCount)
                {
                    bin = bin.PadLeft(bitCount, '0');
                }
                else
                {
                    bin = bin.Substring(bin.Length - bitCount, bitCount);
                }
                var fours = new List<string>();
                // Insert a space each 4 digits
                for (int i = 0; i < bitCount; i += 4)
                {
                    fours.Add(bin.Substring(i, 4));
                }
                result = string.Join(" ", fours);
            }
            else
            {
                throw new NotSupportedException();
            }
            return true;
        }
    }
}
