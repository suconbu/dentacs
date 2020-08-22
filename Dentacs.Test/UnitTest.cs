using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using Suconbu.Scripting.Memezo;
using System.Collections.Generic;
using System.Diagnostics;

namespace Suconbu.Dentacs
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestCalculator()
        {
            var calculator = new Calculator();

            Assert.IsTrue(calculator.Calculate("0"));
            Assert.AreEqual("0", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("1 + (2 * 3) / 4"));
            Assert.AreEqual("2.5", calculator.Result.ToString());

            Assert.IsFalse(calculator.Calculate("1 + "));
            Assert.AreEqual("", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("0x01234567 + 0x89aBcDeF + 0o01234567 + 0b01"));
            Assert.AreEqual("2329169102", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("0x8000000000000000"));
            Assert.AreEqual("-9223372036854775808", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("10 % 3"));
            Assert.AreEqual("1", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("10 % -3"));
            Assert.AreEqual("-2", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("-10 % 3"));
            Assert.AreEqual("2", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("-10 % -3"));
            Assert.AreEqual("-1", calculator.Result.ToString());
        }

        [TestMethod]
        public void TestCalculatorPrecedence()
        {
            var calculator = new Calculator();

            Assert.IsTrue(calculator.Calculate("1+2*2"));
            Assert.AreEqual("5", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("(1+2)*2"));
            Assert.AreEqual("6", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("-2+2"));
            Assert.AreEqual("0", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("-2**2"));
            Assert.AreEqual("-4", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("(-2)**2"));
            Assert.AreEqual("4", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("not \"str\""));
            Assert.AreEqual("0", calculator.Result.ToString());
        }

        [TestMethod]
        public void TestCalculatorBitwiseOperation()
        {
            var calculator = new Calculator();

            Assert.IsTrue(calculator.Calculate("~0"));
            Assert.AreEqual("-1", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("~-1"));
            Assert.AreEqual("0", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("0xFFF0 & 0x0FFF"));
            Assert.AreEqual($"{0x0FF0}", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("0xFFF0 | 0x0FFF"));
            Assert.AreEqual($"{0xFFFF}", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("0xFFF0 ^ 0x0FFF"));
            Assert.AreEqual($"{0xF00F}", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("1 << 0"));
            Assert.AreEqual("1", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("1 << 1"));
            Assert.AreEqual("2", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("1 << 62"));
            Assert.AreEqual("4611686018427387904", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("1 << 63"));
            Assert.AreEqual("-9223372036854775808", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("1 << 64"));
            Assert.AreEqual("0", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("4294967296 >> 0"));
            Assert.AreEqual("4294967296", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("4294967296 >> 1"));
            Assert.AreEqual("2147483648", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("4294967296 >> 32"));
            Assert.AreEqual("1", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("4294967296 >> 33"));
            Assert.AreEqual("0", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("-4294967296 >> 1"));
            Assert.AreEqual("-2147483648", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("-4294967296 >> 32"));
            Assert.AreEqual("-1", calculator.Result.ToString());

            Assert.IsTrue(calculator.Calculate("-4294967296 >> 33"));
            Assert.AreEqual("-1", calculator.Result.ToString());
        }

        [TestMethod]
        public void TestCalculatorDateTimeOperation()
        {
            var calculator = new Calculator();

            var patterns = new Dictionary<string, string>()
            {
                { "2019-08-18T07:36:13+01:00", "2019/08/18 06:36:13" }, // 0
                { "2019-08-18T07:36:13+01", "2019/08/18 06:36:13" },
                { "2019-08-18T07:36+01:00", "2019/08/18 06:36:00" },
                { "2019-08-18T07:36+01", "2019/08/18 06:36:00" },
                { "2019-08-18T07+01:00", "2019/08/18 06:00:00" },
                { "2019-08-18T07+01", "2019/08/18 06:00:00" },
                { "20190818T073613+01:00", "2019/08/18 06:36:13" },
                { "20190818T073613+01", "2019/08/18 06:36:13" },
                { "20190818T0736+01:00", "2019/08/18 06:36:00" },
                { "20190818T0736+01", "2019/08/18 06:36:00" },
                { "20190818T07+01:00", "2019/08/18 06:00:00" },         // 10
                { "20190818T07+01", "2019/08/18 06:00:00" },
                { "2019-08-18T06:36:13Z", "2019/08/18 06:36:13" },
                { "2019-08-18T06:36Z", "2019/08/18 06:36:00" },
                { "2019-08-18T06Z", "2019/08/18 06:00:00" },
                { "20190818T063613Z", "2019/08/18 06:36:13" },
                { "20190818T0636Z", "2019/08/18 06:36:00" },
                { "20190818T06Z", "2019/08/18 06:00:00" },

                { "2019-08-18T07:36:13", "2019/08/18 07:36:13" },
                { "2019-08-18T07:36", "2019/08/18 07:36:00" },
                { "2019-08-18T07", "2019/08/18 07:00:00" },             // 20
                { "2019-08-18", "2019/08/18 00:00:00" },
                { "2019-08", "2019/08/01 00:00:00" },
                { "2019", "2019/01/01 00:00:00" },
                { "20190818T07", "2019/08/18 07:00:00" },
                { "20190818", "2019/08/18 00:00:00" },

                { "2019/08/18 07:36:13", "2019/08/18 07:36:13" },
                { "2019/08/18 07:36", "2019/08/18 07:36:00" },
                { "2019/08/18 07", "2019/08/18 07:00:00" },
                { "2019/08/18", "2019/08/18 00:00:00" },
                { "2019/08", "2019/08/01 00:00:00" },                   // 30
                { "08/18/2019 07:36:13", "2019/08/18 07:36:13" },
                { "08/18/2019 07:36", "2019/08/18 07:36:00" },
                { "08/18/2019 07", "2019/08/18 07:00:00" },
                { "08/18/2019", "2019/08/18 00:00:00" },
                { "08/2019", "2019/08/01 00:00:00" },
            };
            var operations = new Dictionary<string, string>()
            {
                { "'2019/08/18 07:36:13'-'2019/07/17 06:35:12'", "32day 1hour 1min 1sec" },
                { "'2019/08/18 07:36:13'+'17:1:1'", "2019/08/19 0:37:14" },
                { "'2019/08/18 07:36:13'-'8:30'", "2019/08/17 23:06:13" },
                { "'2019/08/18 07:36:13'-'31:36:13'", "2019/08/17 0:00:00" },
                { "'23:59:59'+'0:0:1'", "1day 0hour 0min 0sec" },
                { "'12:00:00'-'0:0:1'", "0day 11hour 59min 59sec" },
                { "'48:00:00'-'0:0:1'", "1day 23hour 59min 59sec" },
            };

            var format = "yyyy'/'MM'/'dd' 'HH':'mm':'ss";
            int count = 0;
            foreach(var date in patterns)
            {
                Assert.IsTrue(DateTimeUtility.TryParseDateTime(date.Key, out var result));
                Assert.AreEqual($"{count}:{date.Value}", $"{count}:" + result.ToString(format));
                count++;
            }
            count = 0;
            foreach (var operation in operations)
            {
                Assert.IsTrue(calculator.Calculate(operation.Key));
                Assert.AreEqual($"{count}:{operation.Value}", $"{count}:" + calculator.Result);
                count++;
            }
        }

        [TestMethod]
        public void TestResultValueConverter()
        {
            var calture = System.Globalization.CultureInfo.CurrentCulture;
            var converter = new ResultConverter();

            var input = "0";
            Assert.IsTrue("0" == converter.Convert(input, typeof(string), "10", calture) as string);
            Assert.IsTrue("0000" == converter.Convert(input, typeof(string), "16", calture) as string);
            Assert.IsTrue("0000 0000 0000 0000" == converter.Convert(input, typeof(string), "2", calture) as string);

            input = "-1";
            Assert.IsTrue("-1" == converter.Convert(input, typeof(string), "10", calture) as string);
            Assert.IsTrue("FFFF FFFF" == converter.Convert(input, typeof(string), "16", calture) as string);
            Assert.IsTrue("1111 1111 1111 1111 1111 1111 1111 1111" == converter.Convert(input, typeof(string), "2", calture) as string);

            input = "65535";
            Assert.IsTrue("65,535" == converter.Convert(input, typeof(string), "10", calture) as string);
            Assert.IsTrue("FFFF" == converter.Convert(input, typeof(string), "16", calture) as string);
            Assert.IsTrue("1111 1111 1111 1111" == converter.Convert(input, typeof(string), "2", calture) as string);

            input = "65536";
            Assert.IsTrue("65,536" == converter.Convert(input, typeof(string), "10", calture) as string);
            Assert.IsTrue("0001 0000" == converter.Convert(input, typeof(string), "16", calture) as string);
            Assert.IsTrue("0000 0000 0000 0001 0000 0000 0000 0000" == converter.Convert(input, typeof(string), "2", calture) as string);

            input = "-65536";
            Assert.IsTrue("-65,536" == converter.Convert(input, typeof(string), "10", calture) as string);
            Assert.IsTrue("FFFF 0000" == converter.Convert(input, typeof(string), "16", calture) as string);
            Assert.IsTrue("1111 1111 1111 1111 0000 0000 0000 0000" == converter.Convert(input, typeof(string), "2", calture) as string);

            input = "-65537";
            Assert.IsTrue("-65,537" == converter.Convert(input, typeof(string), "10", calture) as string);
            Assert.IsTrue("FFFF FFFF FFFE FFFF" == converter.Convert(input, typeof(string), "16", calture) as string);
            Assert.IsTrue("1111 1111 1111 1111 1111 1111 1111 1111\r\n1111 1111 1111 1110 1111 1111 1111 1111" == converter.Convert(input, typeof(string), "2", calture) as string);
        }

        [TestMethod]
        public void TestMathmaticsLibrary()
        {
            var math = new MathmaticsModule();

            //Assert.AreEqual(math.Number(new[] { new Value("0") }).Number, 0m);
            //Assert.AreEqual(math.Number(new[] { new Value("0.5") }).Number, 0.5m);
            //Assert.AreEqual(math.Number(new[] { new Value("-1") }).Number, -1m);
            //Assert.AreEqual(math.Number(new[] { new Value(0) }).Number, 0m);
            //Assert.AreEqual(math.Number(new[] { new Value(0.5) }).Number, 0.5m);
            //Assert.AreEqual(math.Number(new[] { new Value(-1) }).Number, -1m);
            //Assert.ThrowsException<ErrorException>(() => math.Number(new List<Value>()));
            //Assert.ThrowsException<ErrorException>(() => math.Number(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Truncate(new[] { new Value(1.0) }).Number, 1m);
            Assert.AreEqual(math.Truncate(new[] { new Value(0.5) }).Number, 0m);
            Assert.AreEqual(math.Truncate(new[] { new Value(0.4) }).Number, 0m);
            Assert.AreEqual(math.Truncate(new[] { new Value(0) }).Number, 0m);
            Assert.AreEqual(math.Truncate(new[] { new Value(-0.4) }).Number, 0m);
            Assert.AreEqual(math.Truncate(new[] { new Value(-0.5) }).Number, 0m);
            Assert.AreEqual(math.Truncate(new[] { new Value(-1.0) }).Number, -1m);
            Assert.ThrowsException<ErrorException>(() => math.Truncate(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Truncate(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Truncate(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Round(new[] { new Value(2.0) }).Number, 2m);
            Assert.AreEqual(math.Round(new[] { new Value(1.5) }).Number, 2m);
            Assert.AreEqual(math.Round(new[] { new Value(1.4) }).Number, 1m);
            Assert.AreEqual(math.Round(new[] { new Value(1.0) }).Number, 1m);
            Assert.AreEqual(math.Round(new[] { new Value(0.5) }).Number, 1m);
            Assert.AreEqual(math.Round(new[] { new Value(0.4) }).Number, 0m);
            Assert.AreEqual(math.Round(new[] { new Value(0) }).Number, 0m);
            Assert.AreEqual(math.Round(new[] { new Value(-0.4) }).Number, 0m);
            Assert.AreEqual(math.Round(new[] { new Value(-0.5) }).Number, -1m);
            Assert.AreEqual(math.Round(new[] { new Value(-1.0) }).Number, -1m);
            Assert.AreEqual(math.Round(new[] { new Value(-1.4) }).Number, -1m);
            Assert.AreEqual(math.Round(new[] { new Value(-1.5) }).Number, -2m);
            Assert.AreEqual(math.Round(new[] { new Value(-2.0) }).Number, -2m);
            Assert.ThrowsException<ErrorException>(() => math.Round(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Round(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Round(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Floor(new[] { new Value(1.0) }).Number, 1m);
            Assert.AreEqual(math.Floor(new[] { new Value(0.5) }).Number, 0m);
            Assert.AreEqual(math.Floor(new[] { new Value(0.4) }).Number, 0m);
            Assert.AreEqual(math.Floor(new[] { new Value(0) }).Number, 0m);
            Assert.AreEqual(math.Floor(new[] { new Value(-0.4) }).Number, -1m);
            Assert.AreEqual(math.Floor(new[] { new Value(-0.5) }).Number, -1m);
            Assert.AreEqual(math.Floor(new[] { new Value(-1.0) }).Number, -1m);
            Assert.ThrowsException<ErrorException>(() => math.Floor(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Floor(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Floor(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Ceiling(new[] { new Value(1.0) }).Number, 1m);
            Assert.AreEqual(math.Ceiling(new[] { new Value(0.5) }).Number, 1m);
            Assert.AreEqual(math.Ceiling(new[] { new Value(0.4) }).Number, 1m);
            Assert.AreEqual(math.Ceiling(new[] { new Value(0) }).Number, 0m);
            Assert.AreEqual(math.Ceiling(new[] { new Value(-0.4) }).Number, 0m);
            Assert.AreEqual(math.Ceiling(new[] { new Value(-0.5) }).Number, 0m);
            Assert.AreEqual(math.Ceiling(new[] { new Value(-1.0) }).Number, -1m);
            Assert.ThrowsException<ErrorException>(() => math.Ceiling(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Ceiling(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Ceiling(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Absolute(new[] { new Value(1.0) }).Number, 1m);
            Assert.AreEqual(math.Absolute(new[] { new Value(0) }).Number, 0m);
            Assert.AreEqual(math.Absolute(new[] { new Value(-1.0) }).Number, 1m);
            Assert.ThrowsException<ErrorException>(() => math.Absolute(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Absolute(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Absolute(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Sign(new[] { new Value(9.0) }).Number, 1m);
            Assert.AreEqual(math.Sign(new[] { new Value(0) }).Number, 0m);
            Assert.AreEqual(math.Sign(new[] { new Value(-9.0) }).Number, -1m);
            Assert.ThrowsException<ErrorException>(() => math.Sign(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Sign(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Sign(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Factorial(new[] { new Value(0) }).Number, 1m);
            Assert.AreEqual(math.Factorial(new[] { new Value(1) }).Number, 1m);
            Assert.AreEqual(math.Factorial(new[] { new Value(10) }).Number, 3628800m);
            Assert.ThrowsException<ErrorException>(() => math.Factorial(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Factorial(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => math.Factorial(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Factorial(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Sqrt(new[] { new Value(0) }).Number, 0m);
            Assert.AreEqual(math.Sqrt(new[] { new Value(0.5) }).Number, 0.707106781186548m);
            Assert.AreEqual(math.Sqrt(new[] { new Value(1) }).Number, 1m);
            Assert.ThrowsException<ErrorException>(() => math.Sqrt(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Sqrt(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => math.Sqrt(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Sqrt(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Power(new[] { new Value(0), new Value(0) }).Number, 1m);
            Assert.AreEqual(math.Power(new[] { new Value(0), new Value(1) }).Number, 0m);
            Assert.AreEqual(math.Power(new[] { new Value(2), new Value(0) }).Number, 1m);
            Assert.AreEqual(math.Power(new[] { new Value(2), new Value(1) }).Number, 2m);
            Assert.AreEqual(math.Power(new[] { new Value(-2), new Value(0) }).Number, 1m);
            Assert.AreEqual(math.Power(new[] { new Value(-2), new Value(1) }).Number, -2m);
            Assert.AreEqual(math.Power(new[] { new Value(2), new Value(2) }).Number, 4m);
            Assert.AreEqual(math.Power(new[] { new Value(2), new Value(-1) }).Number, 0.5m);
            Assert.AreEqual(math.Power(new[] { new Value(2), new Value(-2) }).Number, 0.25m);
            Assert.ThrowsException<ErrorException>(() => math.Power(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Power(new[] { new Value(0) }));
            Assert.ThrowsException<ErrorException>(() => math.Power(new[] { new Value(1), new Value("1") }));
            Assert.ThrowsException<ErrorException>(() => math.Power(new[] { new Value("1"), new Value(1) }));
            Assert.ThrowsException<ErrorException>(() => math.Power(new[] { new Value("1"), new Value("1") }));
            Assert.ThrowsException<ErrorException>(() => math.Power(new[] { new Value(1), new Value(1), new Value(1) }));

            Assert.AreEqual(math.Exponent(new[] { new Value(0) }).Number, 1m);
            Assert.AreEqual(math.Exponent(new[] { new Value(1) }).Number, 2.71828182845904m);
            Assert.AreEqual(math.Exponent(new[] { new Value(-1) }).Number, 0.367879441171442m);
            Assert.ThrowsException<ErrorException>(() => math.Exponent(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Exponent(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Exponent(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Logarithm(new[] { new Value(0.5) }).Number, -0.693147180559945m);
            Assert.AreEqual(math.Logarithm(new[] { new Value(1) }).Number, 0m);
            Assert.AreEqual(math.Logarithm(new[] { new Value(2) }).Number, 0.693147180559945m);
            Assert.AreEqual(math.Logarithm(new[] { new Value(1), new Value(0) }).Number, 0m);
            Assert.AreEqual(math.Logarithm(new[] { new Value(16), new Value(2) }).Number, 4m);
            Assert.AreEqual(math.Logarithm(new[] { new Value(10000), new Value(10) }).Number, 4m);
            Assert.ThrowsException<ErrorException>(() => math.Logarithm(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm(new[] { new Value(0) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm(new[] { new Value(1), new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm(new[] { new Value(1), new Value(1) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm(new[] { new Value(1), new Value(2), new Value(1) }));

            Assert.AreEqual(math.Logarithm2(new[] { new Value(0.5) }).Number, -1m);
            Assert.AreEqual(math.Logarithm2(new[] { new Value(1) }).Number, 0m);
            Assert.AreEqual(math.Logarithm2(new[] { new Value(2) }).Number, 1m);
            Assert.ThrowsException<ErrorException>(() => math.Logarithm2(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm2(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm2(new[] { new Value(0) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm2(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm2(new[] { new Value(1), new Value(1) }));

            Assert.AreEqual(math.Logarithm10(new[] { new Value(0.1) }).Number, -1m);
            Assert.AreEqual(math.Logarithm10(new[] { new Value(1) }).Number, 0m);
            Assert.AreEqual(math.Logarithm10(new[] { new Value(10) }).Number, 1m);
            Assert.AreEqual(math.Logarithm10(new[] { new Value(100) }).Number, 2m);
            Assert.AreEqual(math.Logarithm10(new[] { new Value(1000) }).Number, 3m);
            Assert.ThrowsException<ErrorException>(() => math.Logarithm10(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm10(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm10(new[] { new Value(0) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm10(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm10(new[] { new Value(1), new Value(1) }));

            Assert.AreEqual(math.Sin(new[] { new Value(0) }).Number, 0m);
            Assert.ThrowsException<ErrorException>(() => math.Sin(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Sin(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Sin(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Cos(new[] { new Value(0) }).Number, 1m);
            Assert.ThrowsException<ErrorException>(() => math.Cos(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Cos(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Cos(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Tan(new[] { new Value(0) }).Number, 0m);
            Assert.ThrowsException<ErrorException>(() => math.Tan(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Tan(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Tan(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Sinh(new[] { new Value(0) }).Number, 0m);
            Assert.ThrowsException<ErrorException>(() => math.Sinh(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Sinh(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Sinh(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Cosh(new[] { new Value(0) }).Number, 1m);
            Assert.ThrowsException<ErrorException>(() => math.Cosh(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Cosh(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Cosh(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Tanh(new[] { new Value(0) }).Number, 0m);
            Assert.ThrowsException<ErrorException>(() => math.Tanh(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Tanh(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Tanh(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Asin(new[] { new Value(0) }).Number, 0m);
            Assert.ThrowsException<ErrorException>(() => math.Asin(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Asin(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Asin(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Acos(new[] { new Value(0) }).Number, 90m);
            Assert.ThrowsException<ErrorException>(() => math.Acos(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Acos(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Acos(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Atan(new[] { new Value(0) }).Number, 0m);
            Assert.ThrowsException<ErrorException>(() => math.Atan(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Atan(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Atan(new[] { new Value(0), new Value(0) }));

            Assert.AreEqual(math.Atan2(new[] { new Value(0), new Value(0) }).Number, 0m);
            Assert.ThrowsException<ErrorException>(() => math.Atan2(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Atan2(new[] { new Value(0) }));
            Assert.ThrowsException<ErrorException>(() => math.Atan2(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Atan2(new[] { new Value(0), new Value(0), new Value(0) }));

            Assert.AreEqual(math.Min(new[] { new Value(0), new Value(1), new Value(-1) }).Number, -1m);
            Assert.AreEqual(math.Min(new[] { new Value(0), new Value(1), new Value("-1") }).Number, 0m);
            Assert.ThrowsException<ErrorException>(() => math.Min(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Min(new[] { new Value("0") }));

            Assert.AreEqual(math.Max(new[] { new Value(0), new Value(1), new Value(-1) }).Number, 1m);
            Assert.AreEqual(math.Max(new[] { new Value(0), new Value("1"), new Value(-1) }).Number, 0m);
            Assert.ThrowsException<ErrorException>(() => math.Max(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Max(new[] { new Value("0") }));

            Assert.AreEqual(math.Sum(new[] { new Value(0), new Value(2), new Value(-1) }).Number, 1m);
            Assert.AreEqual(math.Sum(new[] { new Value(0), new Value(2), new Value("-1") }).Number, 2m);
            Assert.ThrowsException<ErrorException>(() => math.Sum(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Sum(new[] { new Value("0") }));

            Assert.AreEqual(math.Average(new[] { new Value(-1), new Value(0), new Value(1), new Value(10) }).Number, 2.5m);
            Assert.AreEqual(math.Average(new[] { new Value(-1), new Value(0), new Value("1"), new Value(10) }).Number, 3m);
            Assert.ThrowsException<ErrorException>(() => math.Average(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Average(new[] { new Value("0") }));

            Assert.AreEqual(math.Median(new[] { new Value(-1), new Value(0), new Value(1), new Value(10) }).Number, 0.5m);
            Assert.AreEqual(math.Median(new[] { new Value(-1), new Value(0), new Value("1"), new Value(10) }).Number, 0m);
            Assert.ThrowsException<ErrorException>(() => math.Median(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Median(new[] { new Value("0") }));
        }

        [TestMethod]
        public void TestTextBoxHelper()
        {
            string[] lines =
            {
                            // 0  1  2  3  4
                "xxx",      // x  x  x  \r \n

                            // 5  6  7  8  9
                "xxx",      // x  x  x  \r \n

                            // 10 11 12 13 14
                "xxx",      // x  x  x  \r \n

                            // 15 16 17 18 19
                "xxx",      // x  x  x  \r \n
            };
            int s, e;
            TextBoxHelper.GetStartEndLineIndex(lines, 0, 9, out s, out e);
            Assert.AreEqual(s, 0);
            Assert.AreEqual(e, 1);
            TextBoxHelper.GetStartEndLineIndex(lines, 0, 10, out s, out e);
            Assert.AreEqual(s, 0);
            Assert.AreEqual(e, 1);
            TextBoxHelper.GetStartEndLineIndex(lines, 0, 11, out s, out e);
            Assert.AreEqual(s, 0);
            Assert.AreEqual(e, 2);

            TextBoxHelper.GetStartEndLineIndex(lines, 4, 9, out s, out e);
            Assert.AreEqual(s, 0);
            Assert.AreEqual(e, 1);
            TextBoxHelper.GetStartEndLineIndex(lines, 5, 9, out s, out e);
            Assert.AreEqual(s, 1);
            Assert.AreEqual(e, 1);
            TextBoxHelper.GetStartEndLineIndex(lines, 6, 9, out s, out e);
            Assert.AreEqual(s, 1);
            Assert.AreEqual(e, 1);

            TextBoxHelper.GetStartEndLineIndex(lines, 0, 0, out s, out e);
            Assert.AreEqual(s, 0);
            Assert.AreEqual(e, 0);
            TextBoxHelper.GetStartEndLineIndex(lines, 4, 4, out s, out e);
            Assert.AreEqual(s, 0);
            Assert.AreEqual(e, 0);
            TextBoxHelper.GetStartEndLineIndex(lines, 4, 5, out s, out e);
            Assert.AreEqual(s, 0);
            Assert.AreEqual(e, 0);
            TextBoxHelper.GetStartEndLineIndex(lines, 5, 5, out s, out e);
            Assert.AreEqual(s, 1);
            Assert.AreEqual(e, 1);

            TextBoxHelper.GetStartEndLineIndex(lines, 4, 0, out s, out e);
            Assert.AreEqual(s, 0);
            Assert.AreEqual(e, 0);
            TextBoxHelper.GetStartEndLineIndex(lines, 5, 4, out s, out e);
            Assert.AreEqual(s, 0);
            Assert.AreEqual(e, 0);
            TextBoxHelper.GetStartEndLineIndex(lines, 6, 4, out s, out e);
            Assert.AreEqual(s, 0);
            Assert.AreEqual(e, 1);

            TextBoxHelper.GetStartEndLineIndex(lines, 0, 9999, out s, out e);
            Assert.AreEqual(s, 0);
            Assert.AreEqual(e, -1);
            TextBoxHelper.GetStartEndLineIndex(lines, 9999, 0, out s, out e);
            Assert.AreEqual(s, 0);
            Assert.AreEqual(e, -1);
            TextBoxHelper.GetStartEndLineIndex(lines, 9999, 9999, out s, out e);
            Assert.AreEqual(s, -1);
            Assert.AreEqual(e, -1);

            s = TextBoxHelper.GetCharacterIndexOfLineStartFromLineIndex(lines, 0);
            Assert.AreEqual(s, 0);
            s = TextBoxHelper.GetCharacterIndexOfLineStartFromLineIndex(lines, 1);
            Assert.AreEqual(s, 5);
            e = TextBoxHelper.GetCharacterIndexOfLineEndFromLineIndex(lines, 0);
            Assert.AreEqual(e, 5);
            e = TextBoxHelper.GetCharacterIndexOfLineEndFromLineIndex(lines, 1);
            Assert.AreEqual(e, 10);
        }
    }
}
