using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace Suconbu.Dentacs
{
    public class ResultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null) return null;
            if (!int.TryParse(parameter.ToString(), out var radix)) return null;

            if (!decimal.TryParse(value.ToString(), out var number))
            {
                return (radix == 10) ? value.ToString() : "--";
            }

            string result;
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
                int digitCount = (radix == 16) ? (bitCount / 4) : bitCount;
                var digits = System.Convert.ToString(fixedNumber, radix).ToUpper();
                if (digits.Length <= digitCount)
                {
                    digits = digits.PadLeft(digitCount, '0');
                }
                else
                {
                    digits = digits.Substring(digits.Length - digitCount, digitCount);
                }

                var sb = new StringBuilder();
                int brInterval = 32;
                int spInterval = 4;
                for (int i = 0; i < digits.Length; i++)
                {
                    if (0 < i && (digits.Length - i) % brInterval == 0)
                    {
                        sb.AppendLine();
                    }
                    else if (0 < i && (digits.Length - i) % spInterval == 0)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(digits[i]);
                }
                result = sb.ToString();
            }
            else
            {
                throw new NotSupportedException();
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
