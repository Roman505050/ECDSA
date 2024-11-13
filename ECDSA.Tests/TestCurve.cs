using System.Numerics;
using NUnit.Framework;

namespace ECDSA.Tests
{
    public class CurveTests
    {
        [Test]
        public void Contains_PointOnCurve_ReturnsTrue()
        {
            var curve = Curve.Secp256k1;
            var point = new Point(BigInteger.Parse("55066263022277343669578718895168534326250603453777594175500187360389116729240"), 
                                  BigInteger.Parse("32670510020758816978083085130507043184471273380659243275938904335757337482424"), 0);
            
            if (curve.Contains(point))
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail("Point is on the curve");
            }
        }

        [Test]
        public void Contains_PointNotOnCurve_ReturnsFalse()
        {
            var curve = Curve.Secp256k1;
            var point = new Point(BigInteger.Parse("79BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798", System.Globalization.NumberStyles.HexNumber), 
                                  BigInteger.Parse("483ADA7726A3C4655DA4FBFC0E1108A8FD17B448A68554199C47D08FFB10D4B9", System.Globalization.NumberStyles.HexNumber), 0);

            if (curve.Contains(point))
            {
                Assert.Fail("Point is not on the curve");
            }
            else
            {
                Assert.Pass();
            }
        }

        [Test]
        public void Contains_PointXOutOfRange_ReturnsFalse()
        {
            var curve = Curve.Secp256k1;
            var point = new Point(BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007908834671664"), 
                                  BigInteger.Parse("32670510020758816978083085130507043184471273380659243275938904335757337482424"), 0);
            
            if (curve.Contains(point))
            {
                Assert.Fail("Point is not on the curve");
            }
            else
            {
                Assert.Pass();
            }
        }

        [Test]
        public void Contains_PointYOutOfRange_ReturnsFalse()
        {
            var curve = Curve.Secp256k1;
            var point = new Point(BigInteger.Parse("55066263022277343669578718895168534326250603453777594175500187360389116729240"), 
                                  BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007908834671664"), 0);

            if (curve.Contains(point))
            {
                Assert.Fail("Point is not on the curve");
            }
            else
            {
                Assert.Pass();
            }
        }

        [Test]
        public void Length_ReturnsCorrectLength()
        {
            var curve = Curve.Secp256k1;

            var expectedLength = 32;
            var actualLength = curve.Length();
            
            if (expectedLength == actualLength)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail($"Expected length: {expectedLength}, Actual length: {actualLength}");
            }
        }

        [Test]
        public void Y_ValidXAndEven_ReturnsCorrectY()
        {
            var curve = Curve.Secp256k1;
            var x = new BigInteger(3);
            var y = curve.Y(x, true);
            var expectedY = BigInteger.Parse("94471189679404635060807731153122836805497974241028285133722790318709222555876");
            
            if (y == expectedY)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail($"Expected Y: 6, Actual Y: {y}");
            }
        }

        [Test]
        public void Y_ValidXAndOdd_ReturnsCorrectY()
        {
            var curve = Curve.Secp256k1;
            var x = new BigInteger(3);
            var y = curve.Y(x, false);
            var expectedY = BigInteger.Parse("21320899557911560362763253855565071047772010424612278905734793689199612115787");
            
            if (y == expectedY)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail($"Expected Y: 91, Actual Y: {y}");
            }
        }
    }
}