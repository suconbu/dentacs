using System;
using System.Collections.Generic;
using System.Linq;
using Suconbu.Scripting.Memezo;

namespace Suconbu.Dentacs
{
    public class MathmaticsModule : IModule
    {
        public string Name { get; } = "mathmatics";

        public IReadOnlyDictionary<string, Function> Functions { get; }
        public IReadOnlyDictionary<string, Value> Constants { get; }

        public MathmaticsModule()
        {
            this.Functions = new Dictionary<string, Function>()
            {
                { "trunc", this.Truncate },
                { "round", this.Round },
                { "floor", this.Floor },
                { "ceil", this.Ceiling },
                { "abs", this.Absolute },
                { "sign", this.Sign },
                { "fact", this.Factorial },
                { "sqrt", this.Sqrt },
                { "pow", this.Power },
                { "exp", this.Exponent },
                { "log", this.Logarithm },
                { "log2", this.Logarithm2 },
                { "log10", this.Logarithm10 },

                { "sin", this.Sin },
                { "cos", this.Cos },
                { "tan", this.Tan },
                { "asin", this.Asin },
                { "acos", this.Acos },
                { "atan", this.Atan },
                { "atan2", this.Atan2 },
                { "sinh", this.Sinh },
                { "cosh", this.Cosh },
                { "tanh", this.Tanh },
                { "asinh", this.Asinh },
                { "acosh", this.Acosh },
                { "atanh", this.Atanh },

                { "min", this.Min },
                { "max", this.Max },
                { "sum", this.Sum },
                { "avg", this.Average },
                { "med", this.Median },
            };
            this.Constants = new Dictionary<string, Value>()
            {
                { "PI", new Value(Math.PI) },
                { "E", new Value(Math.E) }
            };
        }

        public Value Truncate(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Truncate(args[0].Number));
        }

        public Value Round(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Round(args[0].Number, MidpointRounding.AwayFromZero));
        }

        public Value Floor(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Floor(args[0].Number));
        }

        public Value Ceiling(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Ceiling(args[0].Number));
        }

        public Value Absolute(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Abs(args[0].Number));
        }

        public Value Sign(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Sign(args[0].Number));
        }

        public Value Factorial(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            if (args[0].Number < 0m) throw new ErrorException(ErrorType.InvalidArgument);
            var f = 1m;
            for (var n = args[0].Number; n > 1; n--)
            {
                f *= n;
            }
            return new Value(f);
        }

        public Value Sqrt(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            if (args[0].Number < 0m) throw new ErrorException(ErrorType.InvalidArgument);
            return new Value(Math.Sqrt((double)args[0].Number));
        }

        public Value Power(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "nn", ErrorType.InvalidArgument);
            return new Value(Math.Pow((double)args[0].Number, (double)args[1].Number));
        }

        public Value Exponent(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Exp((double)args[0].Number));
        }

        public Value Logarithm(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "nn?", ErrorType.InvalidArgument);
            if (args[0].Number <= 0m) throw new ErrorException(ErrorType.InvalidArgument);
            if (args.Count == 2 && args[1].Number < 0m) throw new ErrorException(ErrorType.InvalidArgument);
            if (args.Count == 2 && args[1].Number == 1m) throw new ErrorException(ErrorType.InvalidArgument);
            return (args.Count == 1) ?
                new Value(Math.Log((double)args[0].Number)) :
                new Value(Math.Log((double)args[0].Number, (double)args[1].Number));
        }

        public Value Logarithm2(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            if (args[0].Number <= 0m) throw new ErrorException(ErrorType.InvalidArgument);
            return new Value(Math.Log((double)args[0].Number, 2.0));
        }

        public Value Logarithm10(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            if (args[0].Number <= 0m) throw new ErrorException(ErrorType.InvalidArgument);
            return new Value(Math.Log((double)args[0].Number, 10.0));
        }

        public Value Sin(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Round(Math.Sin(Radians(args[0].Number % 360)), 15, MidpointRounding.AwayFromZero));
        }

        public Value Cos(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Round(Math.Cos(Radians(args[0].Number % 360)), 15, MidpointRounding.AwayFromZero));
        }

        public Value Tan(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Round(Math.Tan(Radians(args[0].Number % 360)), 15, MidpointRounding.AwayFromZero));
        }

        public Value Sinh(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Sinh(Radians(args[0].Number)));
        }

        public Value Cosh(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Cosh(Radians(args[0].Number)));
        }

        public Value Tanh(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Math.Tanh(Radians(args[0].Number)));
        }

        public Value Asin(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Degrees(Math.Asin((double)args[0].Number)));
        }

        public Value Acos(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Degrees(Math.Acos((double)args[0].Number)));
        }

        public Value Atan(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Degrees(Math.Atan((double)args[0].Number)));
        }

        public Value Atan2(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "nn", ErrorType.InvalidArgument);
            return new Value(Degrees(Math.Atan2((double)args[0].Number, (double)args[1].Number)));
        }

        public Value Asinh(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Degrees(Math.Asinh((double)args[0].Number)));
        }

        public Value Acosh(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Degrees(Math.Acosh((double)args[0].Number)));
        }

        public Value Atanh(IReadOnlyList<Value> args)
        {
            ArgumentsVerifier.VerifyAndThrow(args, "n", ErrorType.InvalidArgument);
            return new Value(Degrees(Math.Atanh((double)args[0].Number)));
        }

        public Value Min(IReadOnlyList<Value> args)
        {
            var nums = args.Where(a => a.Type == DataType.Number).ToList();
            ArgumentsVerifier.VerifyAndThrow(nums, "n+", ErrorType.InvalidArgument);
            return new Value(nums.Min(n => n.Number));
        }

        public Value Max(IReadOnlyList<Value> args)
        {
            var nums = args.Where(a => a.Type == DataType.Number).ToList();
            ArgumentsVerifier.VerifyAndThrow(nums, "n+", ErrorType.InvalidArgument);
            return new Value(nums.Max(n => n.Number));
        }

        public Value Sum(IReadOnlyList<Value> args)
        {
            var nums = args.Where(a => a.Type == DataType.Number).ToList();
            ArgumentsVerifier.VerifyAndThrow(nums, "n+", ErrorType.InvalidArgument);
            return new Value(nums.Sum(n => n.Number));
        }

        public Value Average(IReadOnlyList<Value> args)
        {
            var nums = args.Where(a => a.Type == DataType.Number).ToList();
            ArgumentsVerifier.VerifyAndThrow(nums, "n+", ErrorType.InvalidArgument);
            return new Value(nums.Average(n => n.Number));
        }

        public Value Median(IReadOnlyList<Value> args)
        {
            var nums = args.Where(a => a.Type == DataType.Number).ToList();
            ArgumentsVerifier.VerifyAndThrow(nums, "n+", ErrorType.InvalidArgument);
            var sorted = nums.OrderBy(n => n.Number).ToList();
            var a = sorted[(sorted.Count - 1) / 2].Number / 2m;
            var b = sorted[(sorted.Count + 0) / 2].Number / 2m;
            return new Value(a + b);
        }

        static double Radians(decimal degree)
        {
            return Math.PI * (double)degree / 180.0;
        }

        static decimal Degrees(double radian)
        {
            return (decimal)(radian / Math.PI * 180.0);
        }
    }
}
