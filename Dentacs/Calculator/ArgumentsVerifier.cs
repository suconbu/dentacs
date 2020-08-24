using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Suconbu.Scripting.Memezo;

namespace Suconbu.Dentacs
{
    public static class ArgumentsVerifier
    {
        static readonly Regex regexNs = new Regex("[ns]");
        static readonly Regex regexX = new Regex("x");
        static readonly Dictionary<DataType, string> replacer = new Dictionary<DataType, string>()
        {
            { DataType.Number, "n" }, { DataType.String, "s" }
        };

        public static bool Verify(IReadOnlyList<Value> args, string pattern)
        {
            var argsString = string.Join(null, args.Select(a => replacer[a.DataType]));
            var verifyPattern = pattern;
            verifyPattern = regexNs.Replace(verifyPattern, @"[$0]");
            verifyPattern = regexX.Replace(verifyPattern, @"[ns]");
            verifyPattern = "^" + verifyPattern + "$";
            return Regex.IsMatch(argsString, verifyPattern);
        }

        public static void VerifyAndThrow(IReadOnlyList<Value> args, string pattern, ErrorType errorType)
        {
            if (!Verify(args, pattern))
            {
                throw new ErrorException(errorType);
            }
        }
    }
}
