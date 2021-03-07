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

            "H:m:s",
            "H:m",
        };
        private static readonly string[] warekiDateTimeFormats =
        {
            "gyy.MM.dd H:m:s",
            "gyy.MM.dd H:m",
            "gyy.MM.dd H",
            "gyy.MM.dd",
            "gyy.MM",
            "gyy",
        };
        private static readonly Regex calenderWeekRegex = new Regex(@"^CW(\d\d)(?:\.([1-7]))?(?:/(\d{4}))?$");
        private static readonly string weekPattern = @"(?:([+-]?\d+(?:\.\d+)?)(?:w|week))?";
        private static readonly string dayPattern = @"(?:([+-]?\d+(?:\.\d+)?)(?:d|day))?";
        private static readonly string hourPattern = @"(?:([+-]?\d+(?:\.\d+)?)(?:h|hour))?";
        private static readonly string minutePattern = @"(?:([+-]?\d+(?:\.\d+)?)(?:m|min|minute))?";
        private static readonly string seccondPattern = @"(?:([+-]?\d+(?:\.\d+)?)(?:s|sec|second))?";
        private static readonly string milliSeccondPattern = @"(?:([+-]?\d+(?:\.\d+)?)(?:ms|msec|millisecond))?";
        private static readonly Regex unitSpecifiedTimeRegex = new Regex($"^{weekPattern}\\s*{dayPattern}\\s*{hourPattern}\\s*{minutePattern}\\s*{seccondPattern}\\s*{milliSeccondPattern}$");
        private static readonly Regex colonSeparatedTimeRegex = new Regex(@"^([+-])(?:(\d+)d\s*)?(\d+):(\d+)(?::(\d+)(?:\.(\d+))?)?$");
        private static readonly CultureInfo cultureInfoJp = new CultureInfo("ja-JP", false) { DateTimeFormat = { Calendar = new JapaneseCalendar() } };
        private static readonly EraInfo eraInfoJp = new EraInfo(cultureInfoJp);
        private static readonly Calendar gregorianCalendar = new GregorianCalendar();
        private static readonly Calendar jpOldCalendar = new JapaneseLunisolarCalendar();
        private static readonly string[] rokuyoStrings = new[] { "大安", "赤口", "先勝", "友引", "先負", "仏滅" };
        private static readonly string[] kanStrings = new [] { "甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸" };
        private static readonly string[] shiStrings = new [] { "子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥" };

        public static DateTime ParseDateTime(string input)
        {
            if (DateTimeUtility.TryParseDateTime(input, out var result))
            {
                return result;
            }
            throw new FormatException();
        }

        public static TimeSpan ParseTimeSpan(string input)
        {
            if (DateTimeUtility.TryParseTimeSpan(input, out var result))
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
            if (DateTimeUtility.TryParseCalenderWeek(input, out result))
            {
                return true;
            }
            if (DateTimeUtility.TryParseJisWareki(input, out result))
            {
                return true;
            }

            return false;
        }

        public static bool TryParseTimeSpan(string input, out TimeSpan result)
        {
            result = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            var match = DateTimeUtility.colonSeparatedTimeRegex.Match(input);
            if (match.Success)
            {
                // 26:00:00 -> 1day + 02:00:00
                var sign = match.Groups[1].Value;
                var d = match.Groups[2].Value;
                var h = match.Groups[3].Value;
                var m = match.Groups[4].Value;
                var s = match.Groups[5].Value;
                var ms = match.Groups[6].Value;
                var days = string.IsNullOrEmpty(d) ? 0 : long.Parse(d);
                var hours = long.Parse(h);
                var minutes = long.Parse(m);
                var seconds = string.IsNullOrEmpty(s) ? 0 : long.Parse(s);
                double milliseconds = string.IsNullOrEmpty(ms) ? 0 : long.Parse(ms);
                milliseconds = milliseconds * 1000.0 / Math.Pow(10.0, ms.Length);
                var ticks = DateTimeUtility.GetTicks(days, hours, minutes, seconds + milliseconds / 1000.0);
                ticks = (sign == "-") ? -ticks : ticks;
                result = new TimeSpan(ticks);
                return true;
            }
            match = DateTimeUtility.unitSpecifiedTimeRegex.Match(input);
            if (match.Success)
            {
                var w = match.Groups[1].Value;
                var d = match.Groups[2].Value;
                var h = match.Groups[3].Value;
                var m = match.Groups[4].Value;
                var s = match.Groups[5].Value;
                var ms = match.Groups[6].Value;
                var weeks = string.IsNullOrEmpty(w) ? 0 : double.Parse(w);
                var days = string.IsNullOrEmpty(d) ? 0 : double.Parse(d);
                var hours = string.IsNullOrEmpty(h) ? 0 : double.Parse(h);
                var minutes = string.IsNullOrEmpty(m) ? 0 : double.Parse(m);
                var seconds = string.IsNullOrEmpty(s) ? 0 : double.Parse(s);
                var milliseconds = string.IsNullOrEmpty(ms) ? 0 : double.Parse(ms);
                var ticks = DateTimeUtility.GetTicks(weeks * 7 + days, hours, minutes, seconds + milliseconds / 1000.0);
                result = new TimeSpan(ticks);
                return true;
            }
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

        // {yyyy}/{MM}/{dd} {HH}:{mm}:{ss}[.{fff}]
        public static string DateTimeToString(DateTime d)
        {
            var sb = new StringBuilder();
            sb.Append(d.ToString("yyyy'/'MM'/'dd' 'HH':'mm':'ss"));
            var milliseconds = GetMillisecondPartFromTicks(d.Ticks) / 1000.0;
            if (milliseconds != 0)
            {
                sb.Append(milliseconds.ToString().Substring(1).TrimEnd('0'));
            }
            return sb.ToString();
        }

        // {N}{yy}.{MM}.{dd}       // jisFormat=true
        // {N}{yy}年{MM}月{dd}日   // jisFormat=false
        public static string DateTimeToWarekiString(DateTime d, bool jisFormat)
        {
            if (jisFormat)
            {
                // M, T, S, ...
                int eraIndex = DateTimeUtility.cultureInfoJp.DateTimeFormat.Calendar.GetEra(d);
                var eraName = DateTimeUtility.eraInfoJp.TryGetName(eraIndex, out var name) ? name : null;
                return eraName + d.ToString("yy'.'MM'.'dd", DateTimeUtility.cultureInfoJp);
            }
            else
            {
                // 明治, 大正, 昭和, ...
                return d.ToString("gyy'年'MM'月'dd'日'", DateTimeUtility.cultureInfoJp);
            }
        }

        public static string DateTimeToKyurekiString(DateTime input)
        {
            int e = DateTimeUtility.jpOldCalendar.GetEra(input);
            int y = DateTimeUtility.jpOldCalendar.GetYear(input);
            int m = DateTimeUtility.jpOldCalendar.GetMonth(input);
            int d = DateTimeUtility.jpOldCalendar.GetDayOfMonth(input);
            var oldFirstDateOfYear = DateTimeUtility.jpOldCalendar.AddDays(
                input,
                1 - DateTimeUtility.jpOldCalendar.GetDayOfYear(input));
            int oldEra = DateTimeUtility.jpOldCalendar.GetEra(oldFirstDateOfYear);
            int oldYear = DateTimeUtility.jpOldCalendar.GetYear(oldFirstDateOfYear);
            int leapMonth = DateTimeUtility.jpOldCalendar.GetLeapMonth(oldYear, oldEra);
            var leapPrefix = (0 < leapMonth && m == leapMonth) ? "閏" : null;
            m = (0 < leapMonth && 0 <= m - leapMonth) ? (m - 1) : m;

            var eraName = DateTimeUtility.cultureInfoJp.DateTimeFormat.GetEraName(e);
            var yearName = (y == 1) ? "元" : $"{y:00}";
            return $"{eraName}{yearName}年{leapPrefix}{m:00}月{d:00}日";
        }

        // (+|-)[{day}d ]{HH}:{mm}:{ss}[.{fff}]
        public static string TimeSpanToString(TimeSpan t)
        {
            var sb = new StringBuilder();
            sb.Append(0 <= t.Ticks ? "+" : "-");
            if (0 != t.Days)
            {
                sb.Append(t.ToString("d'd '"));
            }
            sb.Append(t.ToString("hh':'mm':'ss"));
            var milliseconds = GetMillisecondPartFromTicks(t.Ticks) / 1000.0;
            if (milliseconds != 0)
            {
                sb.Append(milliseconds.ToString().Substring(1).TrimEnd('0'));
            }
            return sb.ToString();
        }

        public static int GetDaysInYear(int year)
        {
            return DateTimeUtility.gregorianCalendar.GetDaysInYear(year);
        }

        public static int GetDaysInMonth(int year, int month)
        {
            return DateTimeUtility.gregorianCalendar.GetDaysInMonth(year, month);
        }

        public static bool TryGetRokuyoString(DateTime input, out string output)
        {
            if (input < DateTimeUtility.jpOldCalendar.MinSupportedDateTime ||
                DateTimeUtility.jpOldCalendar.MaxSupportedDateTime < input)
            {
                output = null;
                return false;
            }
            // https://qiita.com/yo-i/items/21650243f4e08314afd3
            int m = DateTimeUtility.jpOldCalendar.GetMonth(input);
            int d = DateTimeUtility.jpOldCalendar.GetDayOfMonth(input);
            var oldFirstDateOfYear = DateTimeUtility.jpOldCalendar.AddDays(
                input,
                1 - DateTimeUtility.jpOldCalendar.GetDayOfYear(input));
            int e = DateTimeUtility.jpOldCalendar.GetEra(oldFirstDateOfYear);
            int y = DateTimeUtility.jpOldCalendar.GetYear(oldFirstDateOfYear);
            int leapMonth = DateTimeUtility.jpOldCalendar.GetLeapMonth(y, e);
            m = (0 < leapMonth && 0 <= (m - leapMonth)) ? (m - 1) : m;
            int index = (m + d) % DateTimeUtility.rokuyoStrings.Length;
            output = DateTimeUtility.rokuyoStrings[index];
            return true;
        }

        public static bool TryGetEtoString(DateTime input, out string output)
        {
            // http://zecl.hatenablog.com/entry/20090218/p1
            var kan = DateTimeUtility.kanStrings[(input.Year - 594) % 10];
            var shi = DateTimeUtility.shiStrings[(input.Year - 592) % 12];
            output = kan + shi;
            return true;
        }

        private static bool TryParseCalenderWeek(string input, out DateTime result)
        {
            var match = calenderWeekRegex.Match(input);
            if (!match.Success)
            {
                result = DateTime.MinValue;
                return false;
            }
            var w = match.Groups[1].Value;
            var d = match.Groups[2].Value;
            var y = match.Groups[3].Value;
            var week = int.Parse(w);
            var day = string.IsNullOrEmpty(d) ? 1 : int.Parse(d);
            var year = string.IsNullOrEmpty(y) ? DateTime.Now.Year : int.Parse(y);
            var t = new DateTime(year, 1, 1);
            var offset = (int)t.DayOfWeek;
            // 0:mon 1:tue 2:wed 3:thu 4:fri 5:sat 6:sun
            offset = (offset == 0) ? 6 : (offset - 1);
            offset = (3 < offset) ? (offset - 7) : offset;
            result = t.AddDays((week - 1) * 7 + (day - 1) - offset);
            return true;
        }

        private static bool TryParseJisWareki(string input, out DateTime result)
        {
            // Era accepts only english symbol (e.g. 'M', 'T', 'S', ...)
            var s = input;
            if (0 < s.Length && DateTimeUtility.eraInfoJp.TryGetIndex(input.Substring(0, 1), out var index))
            {
                s = DateTimeUtility.cultureInfoJp.DateTimeFormat.GetEraName(index) + input.Substring(1);
                return DateTime.TryParseExact(s, DateTimeUtility.warekiDateTimeFormats, DateTimeUtility.cultureInfoJp, DateTimeStyles.None, out result);
            }
            result = DateTime.MinValue;
            return false;
        }

        private static long GetTicks(double days, double hours, double minutes, double seconds)
        {
            double ticks = 0;
            ticks += TimeSpan.TicksPerSecond * seconds;
            ticks += TimeSpan.TicksPerMinute * minutes;
            ticks += TimeSpan.TicksPerHour * hours;
            ticks += TimeSpan.TicksPerDay * days;
            if (ticks < long.MinValue || long.MaxValue < ticks) throw new OverflowException();
            return (long)ticks;
        }

        private static double GetMillisecondPartFromTicks(long ticks)
        {
            var millisecondTicks = ticks % TimeSpan.TicksPerSecond;
            return (double)millisecondTicks / TimeSpan.TicksPerMillisecond;
        }
    }

    class EraInfo
    {
        private Dictionary<int, string> indexToName = new Dictionary<int, string>();
        private Dictionary<string, int> nameToIndex = new Dictionary<string, int>();

        public EraInfo(CultureInfo cultureInfo)
        {
            // https://www.atmarkit.co.jp/ait/articles/1506/10/news022.html
            for (var c = 'A'; c <= 'Z'; c++)
            {
                int index = cultureInfo.DateTimeFormat.GetEra(c.ToString());
                if (index > 0)
                {
                    this.indexToName.Add(index, c.ToString());
                    this.nameToIndex.Add(c.ToString(), index);
                }
            }
            return;
        }

        public bool TryGetIndex(string name, out int index)
        {
            return this.nameToIndex.TryGetValue(name, out index);
        }

        public bool TryGetName(int index, out string name)
        {
            return this.indexToName.TryGetValue(index, out name);
        }
    }
}
