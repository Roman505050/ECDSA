/*
 * Elliptic Curve Equation
 *
 * y^2 = x^3 + A*x + B (mod P)
 * 
 */
using System;
using System.Numerics;
using ECDSA.Utils;

namespace ECDSA;

public class Curve
{
    public BigInteger A { get; }
    public BigInteger B { get; }
    public BigInteger P { get; }
    public BigInteger N { get; }
    public Point G { get; }
    public string Name { get; }
    public List<int> Oid { get; }
    
    private Curve(BigInteger a, BigInteger b, BigInteger p, BigInteger n, Point g, string name, List<int> oid)
    {
        A = a;
        B = b;
        P = p;
        N = n;
        G = g;
        Name = name;
        Oid = oid;
    }
    
    private static Curve? secp256k1;
    public static Curve Secp256k1
    {
        get
        {
            if (secp256k1 == null)
            {
                secp256k1 = new Curve(
                    HexBigIntegerConvertor.HexToBigInteger("0x0000000000000000000000000000000000000000000000000000000000000000"),
                    HexBigIntegerConvertor.HexToBigInteger("0x0000000000000000000000000000000000000000000000000000000000000007"),
                    HexBigIntegerConvertor.HexToBigInteger("0xfffffffffffffffffffffffffffffffffffffffffffffffffffffffefffffc2f"),
                    HexBigIntegerConvertor.HexToBigInteger("0xfffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364141"),
                    new Point(
                        HexBigIntegerConvertor.HexToBigInteger("0x79be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798"),
                        HexBigIntegerConvertor.HexToBigInteger("0x483ada7726a3c4655da4fbfc0e1108a8fd17b448a68554199c47d08ffb10d4b8"),
                        0
                    ),
                    "secp256k1",
                    new List<int> { 1, 3, 132, 0, 10 }
                );
            }
            return secp256k1;
        }
    }
    
    private static Curve? prime256v1;
    public static Curve Prime256v1
    {
        get
        {
            if (prime256v1 == null)
            {
                prime256v1 = new Curve(
                    HexBigIntegerConvertor.HexToBigInteger("ffffffff00000001000000000000000000000000fffffffffffffffffffffffc"),
                    HexBigIntegerConvertor.HexToBigInteger("5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b"),
                    HexBigIntegerConvertor.HexToBigInteger("ffffffff00000001000000000000000000000000ffffffffffffffffffffffff"),
                    HexBigIntegerConvertor.HexToBigInteger("ffffffff00000000ffffffffffffffffbce6faada7179e84f3b9cac2fc632551"),
                    new Point(
                        HexBigIntegerConvertor.HexToBigInteger("6b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296"),
                        HexBigIntegerConvertor.HexToBigInteger("4fe342e2fe1a7f9b8ee7eb4a7c0f9e162bce33576b315ececbb6406837bf51f5"),
                        0
                    ),
                    "prime256v1",
                    new List<int> { 1, 2, 840, 10045, 3, 1, 7 }
                );   
            }
            
            return prime256v1;
        }
    }
    
    /// <summary>
    /// Determines whether the specified point lies on the elliptic curve.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <returns>
    /// <c>true</c> if the point lies on the curve; otherwise, <c>false</c>.
    /// </returns>
    public bool Contains(Point point)
    {
        // Check if the x-coordinate is within the valid range [0, P-1]
        if (point.X < 0 || point.X >= P)
        {
            return false;
        }
    
        // Check if the y-coordinate is within the valid range [0, P-1]
        if (point.Y < 0 || point.Y >= P)
        {
            return false;
        }
    
        // Check if the point satisfies the elliptic curve equation y^2 = x^3 + A*x + B (mod P)
        if ((BigInteger.Pow(point.Y, 2) - (BigInteger.Pow(point.X, 3) + A * point.X + B)) % P != 0)
        {
            return false;
        }
    
        return true;
    }
    
    /// <summary>
    /// Gets the length of the hexadecimal representation of the prime number N divided by 2.
    /// </summary>
    /// <returns>
    /// The length of the hexadecimal representation of N divided by 2.
    /// </returns>
    public int Length()
    {
        return N.ToString("x").Length / 2;
    }
    
    /// <summary>
    /// Computes the y-coordinate on the elliptic curve for a given x-coordinate.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="isEven">A boolean indicating whether the y-coordinate should be even.</param>
    /// <returns>
    /// The y-coordinate corresponding to the given x-coordinate on the elliptic curve.
    /// </returns>
    public BigInteger Y(BigInteger x, bool isEven)
    {
        // y^2 = x^3 + A*x + B (mod P)
        BigInteger ySquared = (BigInteger.Pow(x, 3) + A * x + B) % P;
        BigInteger y = MathHelper.ModularSquareRoot(ySquared, P);
    
        // If y's parity does not match, return P - y
        if (isEven != (y % 2 == 0))
        {
            y = P - y;
        }
        return y;
        }
    }


