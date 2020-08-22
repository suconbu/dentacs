using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Suconbu.Dentacs
{
    public class DateTimeUtility
    {
        private static readonly string[] isoDateTimeWithTimeZoneFormats =
        {
            "yyyy-MM-ddTHH:mm:sszzz",
            //"yyyy-MM-ddTHH:mm:sszz",
            "yyyy-MM-ddTHH:mmzzz",
            //"yyyy-MM-ddTHH:mmzz",
            "yyyy-MM-ddTHHzzz",
            //"yyyy-MM-ddTHHzz",

            "yyyyMMddTHHmmsszzz",
            //"yyyyMMddTHHmmsszz",
            "yyyyMMddTHHmmzzz",
            //"yyyyMMddTHHmmzz",
            "yyyyMMddTHHzzz",
            //"yyyyMMddTHHzz",

            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mmZ",
            "yyyy-MM-ddTHHZ",

            "yyyyMMddTHHmmssZ",
            "yyyyMMddTHHmmZ",
            "yyyyMMddTHHZ",
        };
        private static readonly string[] isoDateTimeWithoutTimeZoneformats =
        {
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-ddTHH",
            "yyyy-MM-dd",
            "yyyy-MM",
            //"yyyy",

            "yyyyMMddTHHmmss",
            "yyyyMMddTHHmm",
            "yyyyMMddTHH",
            //"yyyyMMdd",
        };
        private static readonly string[] generalDateTimeFormats =
        {
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
        private static readonly Regex colonSeparatedTimeRegex = new Regex(@"^(\d{1,2}):(\d{1,2})(?::(\d{1,2}))?$");
        private static readonly string dayPattern = @"(?:(\d+(?:\.\d+)?)(?:d|day))?";
        private static readonly string hourPattern = @"(?:(\d+(?:\.\d+)?)(?:h|hour))?";
        private static readonly string minutePattern = @"(?:(\d+(?:\.\d+)?)(?:m|min|minute))?";
        private static readonly string seccondPattern = @"(?:(\d+(?:\.\d+)?)(?:s|sec|second))?";
        private static readonly string milliSeccondPattern = @"(?:(\d+(?:\.\d+)?)(?:ms|msec|millisecond))?";
        private static readonly Regex unitSpecifiedTimeRegex = new Regex($"^{dayPattern}\\s*{hourPattern}\\s*{minutePattern}\\s*{seccondPattern}\\s*{milliSeccondPattern}$");

        public static DateTime Parse(string input)
        {
            if (DateTimeUtility.TryParseDateTime(input, out var result))
            {
                return result;
            }
            throw new FormatException();
        }

        public static bool TryParseDateTime(string input, out DateTime result)
        {
            var culture = CultureInfo.InvariantCulture;
            var style = DateTimeStyles.None;
            if (DateTimeOffset.TryParseExact(input, DateTimeUtility.isoDateTimeWithTimeZoneFormats, culture, style, out var date))
            {
                result = date.UtcDateTime;
                return true;
            }
            if (DateTime.TryParseExact(input, DateTimeUtility.isoDateTimeWithoutTimeZoneformats, culture, style, out result))
            {
                return true;
            }
            if (DateTime.TryParseExact(input, DateTimeUtility.generalDateTimeFormats, culture, style, out result))
            {
                return true;
            }
            return false;
        }

        public static bool TryParseTimeSpan(string input, out TimeSpan result)
        {
            var match = DateTimeUtility.colonSeparatedTimeRegex.Match(input);
            if (match.Success)
            {
                // 26:00:00 -> 1day + 02:00:00
                var h = match.Groups[1].Value;
                var m = match.Groups[2].Value;
                var s = match.Groups[3].Value;
                var hours = int.Parse(h);
                var minutes = string.IsNullOrEmpty(m) ? 0 : int.Parse(m);
                var seconds = string.IsNullOrEmpty(s) ? 0 : int.Parse(s);
                result = new TimeSpan(hours, minutes, seconds);
                return true;
            }
            match = DateTimeUtility.unitSpecifiedTimeRegex.Match(input);
            if (match.Success)
            {
                var d = match.Groups[1].Value;
                var h = match.Groups[2].Value;
                var m = match.Groups[3].Value;
                var s = match.Groups[4].Value;
                var ms = match.Groups[5].Value;
                var days = string.IsNullOrEmpty(d) ? 0 : double.Parse(d);
                var hours = string.IsNullOrEmpty(h) ? 0 : double.Parse(h);
                var minutes = string.IsNullOrEmpty(m) ? 0 : double.Parse(m);
                var seconds = string.IsNullOrEmpty(s) ? 0 : double.Parse(s);
                var milliSeconds = string.IsNullOrEmpty(ms) ? 0 : double.Parse(ms);
                result = new TimeSpan(DateTimeUtility.GetTicks(days, hours, minutes, seconds + milliSeconds / 1000.0));
                return true;
            }
            // If this function call put first, the hours part might be parsed as days like as:
            // 48:10:20 -> 48days + 10:20:00 (Not as expected)
            if (!int.TryParse(input, out var _) &&
                TimeSpan.TryParse(input, CultureInfo.InvariantCulture, out result))
            {
                return true;
            }
            result = TimeSpan.Zero;
            return false;
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

        private static long GetTicks(double days, double hours, double minutes, double seconds)
        {
            double ticks = 0;
            ticks += TimeSpan.TicksPerSecond * seconds;
            ticks += TimeSpan.TicksPerMinute * minutes;
            ticks += TimeSpan.TicksPerHour * hours;
            ticks += TimeSpan.TicksPerDay * days;
            return (long)ticks;
        }
    }
}
