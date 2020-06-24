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

            int bits = (fixedNumber <= 0xFFFF) ? 16 : (fixedNumber <= 0xFFFFFFFF) ? 32 : 64;
            if (radix == 16)
            {
                var hex = System.Convert.ToString(fixedNumber, 16);
                result = "0x" + new string('0', ((bits / 4) - hex.Length)) + hex;
            }
            else if (radix == 2)
            {
                var bin = System.Convert.ToString(fixedNumber, 2);
                bin = new string('0', bits - bin.Length) + bin;
                var fours = new List<string>();
                for (int i = 0; i < bits; i += 4)
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
