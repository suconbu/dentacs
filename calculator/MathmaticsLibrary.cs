using System;
using System.Collections.Generic;
using Suconbu.Scripting.Memezo;

namespace Suconbu.Dentacs
{
    public class MathmaticsLibrary : IFunctionLibrary
    {
        public string Name { get { return "mathmatics"; } }

        public IEnumerable<KeyValuePair<string, Function>> GetFunctions()
        {
            return new Dictionary<string, Function>()
            {
                { "num", Number },
                { "trunc", Truncate },
                { "round", Round },
                { "floor", Floor },
                { "ceil", Ceiling },
                { "abs", Absolute },
                { "sign", Sign },
                { "fact", Factorial },
                { "sqrt", Sqrt },
                { "pow", Power },
                { "exp", Exponent },
                { "log", Logarithm },
                { "log2", Logarithm2 },
                { "log10", Logarithm10 },
                { "pi", Pi },
            };
        }

        public static Value Number(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "x", ErrorType.InvalidArgument);
            var v = args[0];
            return new Value(
                (v.Type == DataType.String) ? (decimal.TryParse(v.String, out var n) ? n : throw new ErrorException(ErrorType.InvalidArgument)) :
                (v.Type == DataType.Number) ? v.Number :
                throw new ErrorException(ErrorType.InvalidDataType));
        }

        public static Value Truncate(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Truncate(args[0].Number));
        }

        public static Value Round(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Round(args[0].Number, MidpointRounding.AwayFromZero));
        }

        public static Value Floor(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Floor(args[0].Number));
        }

        public static Value Ceiling(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Ceiling(args[0].Number));
        }

        public static Value Absolute(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Abs(args[0].Number));
        }

        public static Value Sign(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Sign(args[0].Number));
        }

        public static Value Factorial(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            if (args[0].Number < 0m) throw new ErrorException(ErrorType.InvalidArgument);
            var f = 1m;
            for(var n = args[0].Number; n > 1; n--)
            {
                f *= n;
            }
            return new Value(f);
        }

        public static Value Sqrt(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            if (args[0].Number < 0m) throw new ErrorException(ErrorType.InvalidArgument);
            return new Value(Math.Sqrt((double)args[0].Number));
        }

        public static Value Power(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "nn", ErrorType.InvalidArgument);
            return new Value(Math.Pow((double)args[0].Number, (double)args[1].Number));
        }

        public static Value Exponent(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Exp((double)args[0].Number));
        }

        public static Value Logarithm(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "nn?", ErrorType.InvalidArgument);
            if (args[0].Number <= 0m) throw new ErrorException(ErrorType.InvalidArgument);
            if (args.Count == 2 && args[1].Number < 0m) throw new ErrorException(ErrorType.InvalidArgument);
            if (args.Count == 2 && args[1].Number == 1m) throw new ErrorException(ErrorType.InvalidArgument);
            return (args.Count == 1) ?
                new Value(Math.Log((double)args[0].Number)) :
                new Value(Math.Log((double)args[0].Number, (double)args[1].Number));
        }

        public static Value Logarithm2(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            if (args[0].Number <= 0m) throw new ErrorException(ErrorType.InvalidArgument);
            return new Value(Math.Log((double)args[0].Number, 2.0));
        }

        public static Value Logarithm10(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            if (args[0].Number <= 0m) throw new ErrorException(ErrorType.InvalidArgument);
            return new Value(Math.Log((double)args[0].Number, 10.0));
        }

        public static Value Pi(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "", ErrorType.InvalidArgument);
            return new Value(Math.PI);
        }
    }
}
