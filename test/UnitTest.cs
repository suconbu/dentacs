using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using Suconbu.Dentacs;
using Suconbu.Scripting.Memezo;
using System.Collections.Generic;
using System.Diagnostics;

namespace test
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestCalculator()
        {
            var calculator = new Calculator();

            calculator.Expression = "0";
            Assert.IsTrue("0" == calculator.Result);
            Assert.IsTrue(calculator.IsResultEnabled);

            calculator.Expression = "1 + (2 * 3) / 4";
            Assert.IsTrue("2.5" == calculator.Result);
            Assert.IsTrue(calculator.IsResultEnabled);

            calculator.Expression = "1 + ";
            Assert.IsTrue("2.5" == calculator.Result);
            Assert.IsFalse(calculator.IsResultEnabled);

            calculator.Expression = "0x01234567 + 0x89aBcDeF + 0o01234567 + 0b01";
            Assert.IsTrue("2329169102" == calculator.Result);
            Assert.IsTrue(calculator.IsResultEnabled);
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
            Assert.IsTrue(MathmaticsLibrary.Number(new[] { new Value("0") }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Number(new[] { new Value("0.5") }).Number == 0.5m);
            Assert.IsTrue(MathmaticsLibrary.Number(new[] { new Value("-1") }).Number == -1m);
            Assert.IsTrue(MathmaticsLibrary.Number(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Number(new[] { new Value(0.5) }).Number == 0.5m);
            Assert.IsTrue(MathmaticsLibrary.Number(new[] { new Value(-1) }).Number == -1m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Number(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Number(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(MathmaticsLibrary.Truncate(new[] { new Value(1.0) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Truncate(new[] { new Value(0.5) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Truncate(new[] { new Value(0.4) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Truncate(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Truncate(new[] { new Value(-0.4) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Truncate(new[] { new Value(-0.5) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Truncate(new[] { new Value(-1.0) }).Number == -1m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Truncate(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Truncate(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Truncate(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(MathmaticsLibrary.Round(new[] { new Value(2.0) }).Number == 2m);
            Assert.IsTrue(MathmaticsLibrary.Round(new[] { new Value(1.5) }).Number == 2m);
            Assert.IsTrue(MathmaticsLibrary.Round(new[] { new Value(1.4) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Round(new[] { new Value(1.0) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Round(new[] { new Value(0.5) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Round(new[] { new Value(0.4) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Round(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Round(new[] { new Value(-0.4) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Round(new[] { new Value(-0.5) }).Number == -1m);
            Assert.IsTrue(MathmaticsLibrary.Round(new[] { new Value(-1.0) }).Number == -1m);
            Assert.IsTrue(MathmaticsLibrary.Round(new[] { new Value(-1.4) }).Number == -1m);
            Assert.IsTrue(MathmaticsLibrary.Round(new[] { new Value(-1.5) }).Number == -2m);
            Assert.IsTrue(MathmaticsLibrary.Round(new[] { new Value(-2.0) }).Number == -2m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Round(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Round(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Round(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(MathmaticsLibrary.Floor(new[] { new Value(1.0) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Floor(new[] { new Value(0.5) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Floor(new[] { new Value(0.4) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Floor(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Floor(new[] { new Value(-0.4) }).Number == -1m);
            Assert.IsTrue(MathmaticsLibrary.Floor(new[] { new Value(-0.5) }).Number == -1m);
            Assert.IsTrue(MathmaticsLibrary.Floor(new[] { new Value(-1.0) }).Number == -1m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Floor(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Floor(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Floor(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(MathmaticsLibrary.Ceiling(new[] { new Value(1.0) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Ceiling(new[] { new Value(0.5) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Ceiling(new[] { new Value(0.4) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Ceiling(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Ceiling(new[] { new Value(-0.4) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Ceiling(new[] { new Value(-0.5) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Ceiling(new[] { new Value(-1.0) }).Number == -1m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Ceiling(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Ceiling(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Ceiling(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(MathmaticsLibrary.Absolute(new[] { new Value(1.0) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Absolute(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Absolute(new[] { new Value(-1.0) }).Number == 1m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Absolute(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Absolute(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Absolute(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(MathmaticsLibrary.Sign(new[] { new Value(9.0) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Sign(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Sign(new[] { new Value(-9.0) }).Number == -1m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Sign(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Sign(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Sign(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(MathmaticsLibrary.Factorial(new[] { new Value(0) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Factorial(new[] { new Value(1) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Factorial(new[] { new Value(10) }).Number == 3628800m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Factorial(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Factorial(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Factorial(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Factorial(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(MathmaticsLibrary.Sqrt(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Sqrt(new[] { new Value(0.5) }).Number == 0.707106781186548m);
            Assert.IsTrue(MathmaticsLibrary.Sqrt(new[] { new Value(1) }).Number == 1m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Sqrt(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Sqrt(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Sqrt(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Sqrt(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(MathmaticsLibrary.Power(new[] { new Value(0), new Value(0) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Power(new[] { new Value(0), new Value(1) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Power(new[] { new Value(2), new Value(0) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Power(new[] { new Value(2), new Value(1) }).Number == 2m);
            Assert.IsTrue(MathmaticsLibrary.Power(new[] { new Value(-2), new Value(0) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Power(new[] { new Value(-2), new Value(1) }).Number == -2m);
            Assert.IsTrue(MathmaticsLibrary.Power(new[] { new Value(2), new Value(2) }).Number == 4m);
            Assert.IsTrue(MathmaticsLibrary.Power(new[] { new Value(2), new Value(-1) }).Number == 0.5m);
            Assert.IsTrue(MathmaticsLibrary.Power(new[] { new Value(2), new Value(-2) }).Number == 0.25m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Power(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Power(new[] { new Value(0) }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Power(new[] { new Value(1), new Value("1") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Power(new[] { new Value("1"), new Value(1) }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Power(new[] { new Value("1"), new Value("1") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Power(new[] { new Value(1), new Value(1), new Value(1) }));

            Assert.IsTrue(MathmaticsLibrary.Exponent(new[] { new Value(0) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Exponent(new[] { new Value(1) }).Number == 2.71828182845904m);
            Assert.IsTrue(MathmaticsLibrary.Exponent(new[] { new Value(-1) }).Number == 0.367879441171442m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Exponent(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Exponent(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Exponent(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(MathmaticsLibrary.Logarithm(new[] { new Value(0.5) }).Number == -0.693147180559945m);
            Assert.IsTrue(MathmaticsLibrary.Logarithm(new[] { new Value(1) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Logarithm(new[] { new Value(2) }).Number == 0.693147180559945m);
            Assert.IsTrue(MathmaticsLibrary.Logarithm(new[] { new Value(1), new Value(0) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Logarithm(new[] { new Value(16), new Value(2) }).Number == 4m);
            Assert.IsTrue(MathmaticsLibrary.Logarithm(new[] { new Value(10000), new Value(10) }).Number == 4m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm(new[] { new Value(0) }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm(new[] { new Value(1), new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm(new[] { new Value(1), new Value(1) }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm(new[] { new Value(1), new Value(2), new Value(1) }));

            Assert.IsTrue(MathmaticsLibrary.Logarithm2(new[] { new Value(0.5) }).Number == -1m);
            Assert.IsTrue(MathmaticsLibrary.Logarithm2(new[] { new Value(1) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Logarithm2(new[] { new Value(2) }).Number == 1m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm2(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm2(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm2(new[] { new Value(0) }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm2(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm2(new[] { new Value(1), new Value(1) }));

            Assert.IsTrue(MathmaticsLibrary.Logarithm10(new[] { new Value(0.1) }).Number == -1m);
            Assert.IsTrue(MathmaticsLibrary.Logarithm10(new[] { new Value(1) }).Number == 0m);
            Assert.IsTrue(MathmaticsLibrary.Logarithm10(new[] { new Value(10) }).Number == 1m);
            Assert.IsTrue(MathmaticsLibrary.Logarithm10(new[] { new Value(100) }).Number == 2m);
            Assert.IsTrue(MathmaticsLibrary.Logarithm10(new[] { new Value(1000) }).Number == 3m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm10(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm10(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm10(new[] { new Value(0) }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm10(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Logarithm10(new[] { new Value(1), new Value(1) }));

            Assert.IsTrue(MathmaticsLibrary.Pi(new List<Value>()).Number == 3.14159265358979m);
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Pi(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => MathmaticsLibrary.Pi(new[] { new Value(0) }));
        }
    }
}
