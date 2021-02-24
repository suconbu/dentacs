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
