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
            var math = new MathmaticsModule();
            Assert.IsTrue(math.Number(new[] { new Value("0") }).Number == 0m);
            Assert.IsTrue(math.Number(new[] { new Value("0.5") }).Number == 0.5m);
            Assert.IsTrue(math.Number(new[] { new Value("-1") }).Number == -1m);
            Assert.IsTrue(math.Number(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(math.Number(new[] { new Value(0.5) }).Number == 0.5m);
            Assert.IsTrue(math.Number(new[] { new Value(-1) }).Number == -1m);
            Assert.ThrowsException<ErrorException>(() => math.Number(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Number(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(math.Truncate(new[] { new Value(1.0) }).Number == 1m);
            Assert.IsTrue(math.Truncate(new[] { new Value(0.5) }).Number == 0m);
            Assert.IsTrue(math.Truncate(new[] { new Value(0.4) }).Number == 0m);
            Assert.IsTrue(math.Truncate(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(math.Truncate(new[] { new Value(-0.4) }).Number == 0m);
            Assert.IsTrue(math.Truncate(new[] { new Value(-0.5) }).Number == 0m);
            Assert.IsTrue(math.Truncate(new[] { new Value(-1.0) }).Number == -1m);
            Assert.ThrowsException<ErrorException>(() => math.Truncate(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Truncate(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Truncate(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(math.Round(new[] { new Value(2.0) }).Number == 2m);
            Assert.IsTrue(math.Round(new[] { new Value(1.5) }).Number == 2m);
            Assert.IsTrue(math.Round(new[] { new Value(1.4) }).Number == 1m);
            Assert.IsTrue(math.Round(new[] { new Value(1.0) }).Number == 1m);
            Assert.IsTrue(math.Round(new[] { new Value(0.5) }).Number == 1m);
            Assert.IsTrue(math.Round(new[] { new Value(0.4) }).Number == 0m);
            Assert.IsTrue(math.Round(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(math.Round(new[] { new Value(-0.4) }).Number == 0m);
            Assert.IsTrue(math.Round(new[] { new Value(-0.5) }).Number == -1m);
            Assert.IsTrue(math.Round(new[] { new Value(-1.0) }).Number == -1m);
            Assert.IsTrue(math.Round(new[] { new Value(-1.4) }).Number == -1m);
            Assert.IsTrue(math.Round(new[] { new Value(-1.5) }).Number == -2m);
            Assert.IsTrue(math.Round(new[] { new Value(-2.0) }).Number == -2m);
            Assert.ThrowsException<ErrorException>(() => math.Round(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Round(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Round(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(math.Floor(new[] { new Value(1.0) }).Number == 1m);
            Assert.IsTrue(math.Floor(new[] { new Value(0.5) }).Number == 0m);
            Assert.IsTrue(math.Floor(new[] { new Value(0.4) }).Number == 0m);
            Assert.IsTrue(math.Floor(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(math.Floor(new[] { new Value(-0.4) }).Number == -1m);
            Assert.IsTrue(math.Floor(new[] { new Value(-0.5) }).Number == -1m);
            Assert.IsTrue(math.Floor(new[] { new Value(-1.0) }).Number == -1m);
            Assert.ThrowsException<ErrorException>(() => math.Floor(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Floor(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Floor(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(math.Ceiling(new[] { new Value(1.0) }).Number == 1m);
            Assert.IsTrue(math.Ceiling(new[] { new Value(0.5) }).Number == 1m);
            Assert.IsTrue(math.Ceiling(new[] { new Value(0.4) }).Number == 1m);
            Assert.IsTrue(math.Ceiling(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(math.Ceiling(new[] { new Value(-0.4) }).Number == 0m);
            Assert.IsTrue(math.Ceiling(new[] { new Value(-0.5) }).Number == 0m);
            Assert.IsTrue(math.Ceiling(new[] { new Value(-1.0) }).Number == -1m);
            Assert.ThrowsException<ErrorException>(() => math.Ceiling(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Ceiling(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Ceiling(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(math.Absolute(new[] { new Value(1.0) }).Number == 1m);
            Assert.IsTrue(math.Absolute(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(math.Absolute(new[] { new Value(-1.0) }).Number == 1m);
            Assert.ThrowsException<ErrorException>(() => math.Absolute(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Absolute(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Absolute(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(math.Sign(new[] { new Value(9.0) }).Number == 1m);
            Assert.IsTrue(math.Sign(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(math.Sign(new[] { new Value(-9.0) }).Number == -1m);
            Assert.ThrowsException<ErrorException>(() => math.Sign(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Sign(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Sign(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(math.Factorial(new[] { new Value(0) }).Number == 1m);
            Assert.IsTrue(math.Factorial(new[] { new Value(1) }).Number == 1m);
            Assert.IsTrue(math.Factorial(new[] { new Value(10) }).Number == 3628800m);
            Assert.ThrowsException<ErrorException>(() => math.Factorial(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Factorial(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => math.Factorial(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Factorial(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(math.Sqrt(new[] { new Value(0) }).Number == 0m);
            Assert.IsTrue(math.Sqrt(new[] { new Value(0.5) }).Number == 0.707106781186548m);
            Assert.IsTrue(math.Sqrt(new[] { new Value(1) }).Number == 1m);
            Assert.ThrowsException<ErrorException>(() => math.Sqrt(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Sqrt(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => math.Sqrt(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Sqrt(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(math.Power(new[] { new Value(0), new Value(0) }).Number == 1m);
            Assert.IsTrue(math.Power(new[] { new Value(0), new Value(1) }).Number == 0m);
            Assert.IsTrue(math.Power(new[] { new Value(2), new Value(0) }).Number == 1m);
            Assert.IsTrue(math.Power(new[] { new Value(2), new Value(1) }).Number == 2m);
            Assert.IsTrue(math.Power(new[] { new Value(-2), new Value(0) }).Number == 1m);
            Assert.IsTrue(math.Power(new[] { new Value(-2), new Value(1) }).Number == -2m);
            Assert.IsTrue(math.Power(new[] { new Value(2), new Value(2) }).Number == 4m);
            Assert.IsTrue(math.Power(new[] { new Value(2), new Value(-1) }).Number == 0.5m);
            Assert.IsTrue(math.Power(new[] { new Value(2), new Value(-2) }).Number == 0.25m);
            Assert.ThrowsException<ErrorException>(() => math.Power(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Power(new[] { new Value(0) }));
            Assert.ThrowsException<ErrorException>(() => math.Power(new[] { new Value(1), new Value("1") }));
            Assert.ThrowsException<ErrorException>(() => math.Power(new[] { new Value("1"), new Value(1) }));
            Assert.ThrowsException<ErrorException>(() => math.Power(new[] { new Value("1"), new Value("1") }));
            Assert.ThrowsException<ErrorException>(() => math.Power(new[] { new Value(1), new Value(1), new Value(1) }));

            Assert.IsTrue(math.Exponent(new[] { new Value(0) }).Number == 1m);
            Assert.IsTrue(math.Exponent(new[] { new Value(1) }).Number == 2.71828182845904m);
            Assert.IsTrue(math.Exponent(new[] { new Value(-1) }).Number == 0.367879441171442m);
            Assert.ThrowsException<ErrorException>(() => math.Exponent(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Exponent(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Exponent(new[] { new Value(0), new Value(0) }));

            Assert.IsTrue(math.Logarithm(new[] { new Value(0.5) }).Number == -0.693147180559945m);
            Assert.IsTrue(math.Logarithm(new[] { new Value(1) }).Number == 0m);
            Assert.IsTrue(math.Logarithm(new[] { new Value(2) }).Number == 0.693147180559945m);
            Assert.IsTrue(math.Logarithm(new[] { new Value(1), new Value(0) }).Number == 0m);
            Assert.IsTrue(math.Logarithm(new[] { new Value(16), new Value(2) }).Number == 4m);
            Assert.IsTrue(math.Logarithm(new[] { new Value(10000), new Value(10) }).Number == 4m);
            Assert.ThrowsException<ErrorException>(() => math.Logarithm(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm(new[] { new Value(0) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm(new[] { new Value(1), new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm(new[] { new Value(1), new Value(1) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm(new[] { new Value(1), new Value(2), new Value(1) }));

            Assert.IsTrue(math.Logarithm2(new[] { new Value(0.5) }).Number == -1m);
            Assert.IsTrue(math.Logarithm2(new[] { new Value(1) }).Number == 0m);
            Assert.IsTrue(math.Logarithm2(new[] { new Value(2) }).Number == 1m);
            Assert.ThrowsException<ErrorException>(() => math.Logarithm2(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm2(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm2(new[] { new Value(0) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm2(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm2(new[] { new Value(1), new Value(1) }));

            Assert.IsTrue(math.Logarithm10(new[] { new Value(0.1) }).Number == -1m);
            Assert.IsTrue(math.Logarithm10(new[] { new Value(1) }).Number == 0m);
            Assert.IsTrue(math.Logarithm10(new[] { new Value(10) }).Number == 1m);
            Assert.IsTrue(math.Logarithm10(new[] { new Value(100) }).Number == 2m);
            Assert.IsTrue(math.Logarithm10(new[] { new Value(1000) }).Number == 3m);
            Assert.ThrowsException<ErrorException>(() => math.Logarithm10(new List<Value>()));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm10(new[] { new Value("0") }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm10(new[] { new Value(0) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm10(new[] { new Value(-1) }));
            Assert.ThrowsException<ErrorException>(() => math.Logarithm10(new[] { new Value(1), new Value(1) }));

            Assert.IsTrue(math.PI.Number == 3.14159265358979m);
        }
    }
}
