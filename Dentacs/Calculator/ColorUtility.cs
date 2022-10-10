using System;
using System.Windows.Media;

namespace Suconbu.Dentacs
{
    public class ColorUtility
    {
        public static Color ParseColor(string input)
        {
            if (TryParseColor(input, out var result))
            {
                return result;
            }
            throw new FormatException();
        }

        public static bool TryParseColor(string input, out Color result)
        {
            if (TryParseHex(input, out result))
            {
                return true;
            }
            if (TryParseColorName(input, out result))
            {
                return true;
            }
            return false;
        }

        public static bool TryParseHex(string input, out Color result)
        {
            // #rrggbbaa
            // #rrggbb
            // #rgba
            // #rgb
            result = default;
            if (!input.StartsWith("#")) return false;

            var s = input.TrimStart('#');
            if (s.Length == 3 || s.Length == 4)
            {
                if (byte.TryParse(s.Substring(0, 1), System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                    byte.TryParse(s.Substring(1, 1), System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                    byte.TryParse(s.Substring(2, 1), System.Globalization.NumberStyles.HexNumber, null, out var b))
                {
                    byte a = 0xF;
                    if (s.Length == 3 || byte.TryParse(s.Substring(3, 1), System.Globalization.NumberStyles.HexNumber, null, out a))
                    {
                        //a = (byte)(a * 17);
                        r = (byte)(r * 17);
                        g = (byte)(g * 17);
                        b = (byte)(b * 17);
                        result = Color.FromArgb(0xFF, r, g, b);
                        return true;
                    }
                }
            }
            else if(s.Length == 6 || s.Length == 8)
            {
                if (byte.TryParse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                    byte.TryParse(s.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                    byte.TryParse(s.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
                {
                    byte a = 0xFF;
                    if (s.Length == 6 || byte.TryParse(s.Substring(6, 2), System.Globalization.NumberStyles.HexNumber, null, out a))
                    {
                        result = Color.FromArgb(0xFF, r, g, b);
                        return true;
                    }
                }
            }
            else
            {
            }
            return false;
        }

        public static bool TryParseColorName(string input, out Color result)
        {
            result = default;
            try
            {
                result = (Color)ColorConverter.ConvertFromString(input);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}
