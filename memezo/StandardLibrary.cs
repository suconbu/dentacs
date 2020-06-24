using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Suconbu.Scripting.Memezo
{
    class StandardLibrary : IFunctionLibrary
    {
        public string Name { get { return "standard"; } }

        public IEnumerable<KeyValuePair<string, Function>> GetFunctions()
        {
            return new Dictionary<string, Function>()
            {
                { "typeof", TypeOf }, { "str", Str }, { "num", Num },
                { "abs", Abs }, { "min", Min }, { "max", Max },
                { "floor", Floor }, { "ceil", Ceil }, { "truncate", Truncate }, { "round", Round },
                { "len", Len }, { "strlen", Len }, { "chr", Chr }, { "ord", Ord }, { "slice", Slice }
            };
        }

        public static Value TypeOf(List<Value> args)
        {
            if (args.Count != 1) throw new InternalErrorException(ErrorType.InvalidNumberOfArguments);
            var value = args[0];
            return new Value(
                (value.Type == DataType.Number) ? "number" :
                (value.Type == DataType.String) ? "string" :
                throw new InternalErrorException(ErrorType.InvalidDataType));
        }

        [Description("str(v) : Convert a value to string.")]
        public static Value Str(List<Value> args)
        {
            if (args.Count != 1) throw new InternalErrorException(ErrorType.InvalidNumberOfArguments);
            return new Value(args[0].String);
        }

        [Description("num(v) : Convert a value to number.")]
        public static Value Num(List<Value> args)
        {
            if (args.Count != 1) throw new InternalErrorException(ErrorType.InvalidNumberOfArguments);
            var value = args[0];
            return new Value(
                (value.Type == DataType.String) ? (double.TryParse(value.String, out var n) ? n : throw new InternalErrorException(ErrorType.InvalidParameter)) :
                (value.Type == DataType.Number) ? value.Number :
                throw new InternalErrorException(ErrorType.InvalidDataType));
        }

        [Description("abs(n) -> : n < 0 ? -n : n")]
        public static Value Abs(List<Value> args)
        {
            if (args.Count != 1) throw new InternalErrorException(ErrorType.InvalidNumberOfArguments);
            if (args[0].Type != DataType.Number) throw new InternalErrorException(ErrorType.InvalidDataType);
            return new Value(Math.Abs(args[0].Number));
        }

        [Description("min(n1, n2[, ...]) : Get a minimum value in arguments.")]
        public static Value Min(List<Value> args)
        {
            if (args.Count < 2) throw new InternalErrorException(ErrorType.InvalidNumberOfArguments);
            var min = double.MaxValue;
            foreach (var arg in args)
            {
                if (arg.Type != DataType.Number) throw new InternalErrorException(ErrorType.InvalidDataType);
                min = Math.Min(min, arg.Number);
            }
            return new Value(min);
        }

        [Description("max(n1, n2[, ...]) : Get a maximum value in arguments.")]
        public static Value Max(List<Value> args)
        {
            if (args.Count < 2) throw new InternalErrorException(ErrorType.InvalidNumberOfArguments);
            var max = double.MinValue;
            foreach (var arg in args)
            {
                if (arg.Type != DataType.Number) throw new InternalErrorException(ErrorType.InvalidDataType);
                max = Math.Max(max, arg.Number);
            }
            return new Value(max);
        }

        [Description("floor(n) : Largest integer less than or equal to the specified number.")]
        public static Value Floor(List<Value> args)
        {
            if (args.Count != 1) throw new InternalErrorException(ErrorType.InvalidNumberOfArguments);
            if (args[0].Type != DataType.Number) throw new InternalErrorException(ErrorType.InvalidDataType);
            return new Value(Math.Floor(args[0].Number));
        }

        [Description("ceil(n) : Smallest integer greater than or equal to the specified number.")]
        public static Value Ceil(List<Value> args)
        {
            if (args.Count != 1) throw new InternalErrorException(ErrorType.InvalidNumberOfArguments);
            if (args[0].Type != DataType.Number) throw new InternalErrorException(ErrorType.InvalidDataType);
            return new Value(Math.Ceiling(args[0].Number));
        }

        [Description("truncate(n) : Get a integral part of a specified number.")]
        public static Value Truncate(List<Value> args)
        {
            if (args.Count != 1) throw new InternalErrorException(ErrorType.InvalidNumberOfArguments);
            if (args[0].Type != DataType.Number) throw new InternalErrorException(ErrorType.InvalidDataType);
            return new Value(Math.Truncate(args[0].Number));
        }

        [Description("round(n) Rounds a specified number to the nearest even integer.")]
        public static Value Round(List<Value> args)
        {
            if (args.Count != 1) throw new InternalErrorException(ErrorType.InvalidNumberOfArguments);
            if (args[0].Type != DataType.Number) throw new InternalErrorException(ErrorType.InvalidDataType);
            return new Value(Math.Round(args[0].Number));
        }

        [Description("len(s), strlen(s) : Return a length of string.")]
        public static Value Len(List<Value> args)
        {
            if (args.Count != 1) throw new InternalErrorException(ErrorType.InvalidNumberOfArguments);
            if (args[0].Type != DataType.String) throw new InternalErrorException(ErrorType.InvalidDataType);
            return new Value(args[0].String.Length);
        }

        [Description("chr(code) : Code to character. (97 -> 'a')")]
        public static Value Chr(List<Value> args)
        {
            if (args.Count != 1) throw new InternalErrorException(ErrorType.InvalidNumberOfArguments);
            if (args[0].Type != DataType.Number) throw new InternalErrorException(ErrorType.InvalidDataType);
            return new Value(Convert.ToChar((int)args[0].Number).ToString());
        }

        [Description("ord(char) : Character to code. ('a' -> 97)")]
        public static Value Ord(List<Value> args)
        {
            if (args.Count != 1) throw new InternalErrorException(ErrorType.InvalidNumberOfArguments);
            if (args[0].Type != DataType.String) throw new InternalErrorException(ErrorType.InvalidDataType);
            if (args[0].String.Length != 1) throw new InternalErrorException(ErrorType.InvalidParameter);
            return new Value(Convert.ToInt32(args[0].String[0]));
        }

        [Description("slice(s, [start[, stop]]) : Take a part of string.")]
        public static Value Slice(List<Value> args)
        {
            if (args.Count > 3) throw new InternalErrorException(ErrorType.InvalidNumberOfArguments);
            if (args[0].Type != DataType.String) throw new InternalErrorException(ErrorType.InvalidDataType);
            var s = args[0].String;
            var start = 0;
            var stop = s.Length;
            if (args.Count > 1)
            {
                if (args[1].Type != DataType.Number) throw new InternalErrorException(ErrorType.InvalidDataType);
                start = (int)args[1].Number;
                start = (start < 0) ? Math.Max(0, (s.Length + start)) : start;
                if (args.Count > 2)
                {
                    if (args[2].Type != DataType.Number) throw new InternalErrorException(ErrorType.InvalidDataType);
                    stop = (int)args[2].Number;
                    stop = (stop < 0) ? Math.Max(0, s.Length + stop) : Math.Min(stop, s.Length);
                }
            }
            return new Value((start < s.Length && start < stop) ? s.Substring(start, stop - start) : string.Empty);
        }
    }
}
