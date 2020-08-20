using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suconbu.Dentacs
{
    public class DateTimeUtility
    {
        private static string[] withTimeZoneFormats =
        {
            "yyyy-MM-ddTHH:mm:sszzz",
            "yyyy-MM-ddTHH:mm:sszz",
            "yyyy-MM-ddTHH:mmzzz",
            "yyyy-MM-ddTHH:mmzz",
            "yyyy-MM-ddTHHzzz",
            "yyyy-MM-ddTHHzz",

            "yyyyMMddTHHmmsszzz",
            "yyyyMMddTHHmmsszz",
            "yyyyMMddTHHmmzzz",
            "yyyyMMddTHHmmzz",
            "yyyyMMddTHHzzz",
            "yyyyMMddTHHzz",

            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mmZ",
            "yyyy-MM-ddTHHZ",

            "yyyyMMddTHHmmssZ",
            "yyyyMMddTHHmmZ",
            "yyyyMMddTHHZ",
        };
        private static string[] withoutTimeZoneformats =
        {
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-ddTHH",
            "yyyy-MM-dd",
            "yyyy-MM",
            "yyyy",

            "yyyyMMddTHHmmss",
            "yyyyMMddTHHmm",
            "yyyyMMddTHH",
            "yyyyMMdd",

            "yyyy/M/d H:m:s",
            "yyyy/M/d H:m",
            "yyyy/M/d H",
            "yyyy/M/d",
            "yyyy/M",

            "M/d/yyyy H:m:s",
            "M/d/yyyy H:m",
            "M/d/yyyy H",
            "M/d/yyyy",
            "M/yyyy",

            "M/d H:m:s",
            "M/d H:m",
            "M/d H",
            "M/d",
        };

        public static DateTime Parse(string s)
        {
            if (DateTimeUtility.TryParseDateTime(s, out var result))
            {
                return result;
            }
            throw new FormatException();
        }

        public static bool TryParseDateTime(string s, out DateTime result)
        {
            if (DateTimeOffset.TryParseExact(s, DateTimeUtility.withTimeZoneFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                result = date.UtcDateTime;
                return true;
            }
            if (DateTime.TryParseExact(s, DateTimeUtility.withoutTimeZoneformats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return true;
            }
            return false;
        }

        public static bool TryParseTimeSpan(string s, out TimeSpan result)
        {
            return TimeSpan.TryParse(s, CultureInfo.InvariantCulture, out result);
        }

        public static DateTime DateTimeFromSeconds(decimal seconds)
        {
            return new DateTime((long)(seconds * TimeSpan.TicksPerSecond));
        }

        public static TimeSpan TimeSpanFromSeconds(decimal seconds)
        {
            return new TimeSpan((long)(seconds * TimeSpan.TicksPerSecond));
        }

        public static decimal DateTimeToSeconds(DateTime d)
        {
            return (decimal)d.Ticks / TimeSpan.TicksPerSecond;
        }

        public static decimal TimeSpanToSeconds(TimeSpan t)
        {
            return (decimal)t.Ticks / TimeSpan.TicksPerSecond;
        }
    }
}
