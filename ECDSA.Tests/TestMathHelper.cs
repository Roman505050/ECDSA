using System;
using System.Numerics;
using NUnit.Framework;

namespace ECDSA.Tests
{
    public class TestMathHelper
    {
        [TestCase("4", "7", "2")]
        [TestCase("123", "456", "152")]
        [TestCase("555657345345", "7346457456756756756", "4897638304504504504")]
        public void Inv_ReturnsCorrectModularInverse(string xStr, string nStr, string expectedStr)
        {
            BigInteger x = BigInteger.Parse(xStr);
            BigInteger n = BigInteger.Parse(nStr);
            BigInteger expected = BigInteger.Parse(expectedStr);
            
            var result = MathHelper.Inv(x, n);
            Console.WriteLine($"Result: {result}");
            
            if (result == expected)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail($"Expected: {expected}, Actual: {result}");
            }
        }
        
        [TestCase("1", "56", "1111", "12", "324", "193", "25", "16")]
        [TestCase("1", "56", "3", "12", "324", "193", "97", "12")]
        [TestCase("1", "2", "3", "12", "324", "301", "313", "12")]
        public void JacobianDouble_ReturnsCorrectPoint(string pointXStr, string pointYStr, string pointZStr, string aStr, string pStr, string expectedXStr, string expectedYStr, string expectedZStr)
        {
            var point = new Point(BigInteger.Parse(pointXStr), BigInteger.Parse(pointYStr), BigInteger.Parse(pointZStr));
            var a = BigInteger.Parse(aStr);
            var p = BigInteger.Parse(pStr);
            var expected = new Point(BigInteger.Parse(expectedXStr), BigInteger.Parse(expectedYStr), BigInteger.Parse(expectedZStr));
            
            var result = MathHelper.JacobianDouble(point, a, p);
            
            if (result == expected)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail($"Expected: {expected}, Actual: {result}");
            }
        }
    }
}