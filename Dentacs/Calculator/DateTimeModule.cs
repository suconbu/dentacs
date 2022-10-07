using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Suconbu.Scripting.Memezo;

namespace Suconbu.Dentacs
{
    public class DateTimeModule : IModule
    {
        public string Name { get; } = "datetime";

        public IReadOnlyDictionary<string, Function> Functions { get; }
        public IReadOnlyDictionary<string, Value> Constants { get; }

        public DateTimeModule()
        {
            this.Functions = new Dictionary<string, Function>()
            {
                // DateTime
                { "dayofyear", this.DayOfYear },
                { "dayofweek", this.DayOfWeek },
                { "daysinyear", this.DaysInYear },
                { "daysinmonth", this.DaysInMonth },
                { "weekofyear", this.WeekOfYear },
                { "cw", this.CalendarWeek },
                { "wareki", this.Wareki },
                { "kyureki", this.Kyureki },
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
            return new Value(DateTimeUtility.GetDaysInYear(date.Year));
        }

        public Value DaysInMonth(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            var date = DateTimeUtility.ParseDateTime(args[0].String);
            return new Value(DateTimeUtility.GetDaysInMonth(date.Year, date.Month));
        }

        public Value Wareki(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            var date = DateTimeUtility.ParseDateTime(args[0].String);
            var sb = new StringBuilder();
            sb.Append(DateTimeUtility.DateTimeToWarekiString(date, false));
            sb.Append(DateTimeUtility.TryGetEtoString(date, out var eto) ? $" {eto}" : null);
            sb.Append(DateTimeUtility.TryGetRokuyoString(date, out var rokuyo) ? $" {rokuyo}" : null);
            return new Value(sb.ToString());
        }

        public Value Kyureki(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            var date = DateTimeUtility.ParseDateTime(args[0].String);
            var sb = new StringBuilder();
            sb.Append(DateTimeUtility.DateTimeToKyurekiString(date));
            sb.Append(DateTimeUtility.TryGetEtoString(date, out var eto) ? $" {eto}" : null);
            sb.Append(DateTimeUtility.TryGetRokuyoString(date, out var rokuyo) ? $" {rokuyo}" : null);
            return new Value(sb.ToString());
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

        public Value WeekOfYear(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            var date = DateTimeUtility.ParseDateTime(args[0].String);
            var week = DateTimeUtility.GetWeekOfYear(date);
            return new Value(week);
        }

        public Value CalendarWeek(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "s", ErrorType.InvalidArgument);
            var date = DateTimeUtility.ParseDateTime(args[0].String);
            var year = date.Year;
            double week = DateTimeUtility.GetWeekOfYear(date);
            var dow = (date.DayOfWeek == 0) ? 7 : (int)date.DayOfWeek;
            if (week == 0)
            {
                var last = new DateTime(date.Year, 1, 1).AddDays(-1);
                year = last.Year;
                week = DateTimeUtility.GetWeekOfYear(last); 
            }
            return new Value($"{week:00}.{dow}/{year:0000}");
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
