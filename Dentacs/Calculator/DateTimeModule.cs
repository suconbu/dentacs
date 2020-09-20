using System.Collections.Generic;
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
                { "seconds", this.Seconds },
                { "minutes", this.Minutes },
                { "hours", this.Hours },
                { "days", this.Days },
                { "weeks", this.Weeks },
            };
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
