using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using Suconbu.Dentacs;

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
            var converter = new ResultValueConverter();

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
    }
}
