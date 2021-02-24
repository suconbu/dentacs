using System;
using System.Collections.Generic;
using System.Globalization;
using Suconbu.Scripting.Memezo;

namespace Suconbu.Dentacs
{
    public class DateTimeModule : IModule
    {
        public string Name { get; } = "datetime";

        public IReadOnlyDictionary<string, Function> Functions { get; }
        public IReadOnlyDictionary<string, Value> Constants { get; }

        private readonly Calendar gregorianCalendar = new GregorianCalendar();
        private readonly Calendar jpOldCalendar = new JapaneseLunisolarCalendar();
        private readonly string[] rokuyoStrings = new[] { "大安", "赤口", "先勝", "友引", "先負", "仏滅" };
        private readonly string[] kanStrings = new string[] { "甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸" };
        private readonly string[] shiStrings = new string[] { "子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥" };

        public DateTimeModule()
        {
            this.Functions = new Dictionary<string, Function>()
            {
                // DateTime
                { "dayofyear", this.DayOfYear },
                { "dayofweek", this.DayOfWeek },
                { "daysinyear", this.DaysInYear },
                { "daysinmonth", this.DaysInMonth },
                { "wareki", this.Wareki },
                { "rokuyo", this.Rokuyo },
                { "eto", this.Eto },
                { "now", this.Now },
                { "today", this.Today },

                // TimeSpan
                { "seconds", this.Seconds },
                { "minutes", this.Minutes },
                { "hours", this.Hours },
                { "days", this.Days },
                { "weeks", this.Weeks },
            };
        }

        public Value DayOfYear(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            return new Value(DateTimeUtility.ParseDateTime(args[0].String).DayOfYear);
        }

        public Value DayOfWeek(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            return new Value(DateTimeUtility.ParseDateTime(args[0].String).DayOfWeek.ToString().Substring(0, 3).ToLower());
        }

        public Value DaysInYear(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            var date = DateTimeUtility.ParseDateTime(args[0].String);
            return new Value(this.gregorianCalendar.GetDaysInYear(date.Year));
        }

        public Value DaysInMonth(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            var date = DateTimeUtility.ParseDateTime(args[0].String);
            return new Value(this.gregorianCalendar.GetDaysInMonth(date.Year, date.Month));
        }

        public Value Wareki(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            var date = DateTimeUtility.ParseDateTime(args[0].String);
            return new Value(DateTimeUtility.DateTimeToWarekiString(date, false));
        }

        public Value Rokuyo(IReadOnlyList<Value> args)
        {
            // https://qiita.com/yo-i/items/21650243f4e08314afd3
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            var date = DateTimeUtility.ParseDateTime(args[0].String);
            int e = this.jpOldCalendar.GetEra(date);
            int y = this.jpOldCalendar.GetYear(date);
            int m = this.jpOldCalendar.GetMonth(date);
            int d = this.jpOldCalendar.GetDayOfMonth(date);
            int leapMonth = this.jpOldCalendar.GetLeapMonth(y, e);
            m = (0 < leapMonth && 0 <= (m - leapMonth)) ? (m - 1) : m;
            int index = (m + d) % this.rokuyoStrings.Length;
            return new Value(this.rokuyoStrings[index]);
        }

        public Value Eto(IReadOnlyList<Value> args)
        {
            // http://zecl.hatenablog.com/entry/20090218/p1
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            var date = DateTimeUtility.ParseDateTime(args[0].String);
            var k = this.kanStrings[(date.Year - 594) % 10];
            var s = this.shiStrings[(date.Year - 592) % 12];
            return new Value(k + s);
        }

        public Value Now(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "", ErrorType.InvalidArgument);
            var ticks = DateTime.Now.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond;
            return new Value(DateTimeUtility.DateTimeToString(new DateTime(ticks)));
        }

        public Value Today(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "", ErrorType.InvalidArgument);
            var ticks = DateTime.Today.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond;
            return new Value(DateTimeUtility.DateTimeToString(new DateTime(ticks)));
        }

        public Value Seconds(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            return new Value(DateTimeUtility.ParseTimeSpan(args[0].String).TotalSeconds);
        }

        public Value Minutes(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            return new Value(DateTimeUtility.ParseTimeSpan(args[0].String).TotalMinutes);
        }

        public Value Hours(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            return new Value(DateTimeUtility.ParseTimeSpan(args[0].String).TotalHours);
        }

        public Value Days(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            return new Value(DateTimeUtility.ParseTimeSpan(args[0].String).TotalDays);
        }

        public Value Weeks(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            return new Value(DateTimeUtility.ParseTimeSpan(args[0].String).TotalDays / 7.0);
        }
    }
}
