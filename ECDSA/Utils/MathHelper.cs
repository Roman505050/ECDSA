using System.Numerics;

namespace ECDSA;

public class MathHelper
{
    /// <summary>
    /// Computes the modular square root of a number.
    /// </summary>
    /// <param name="a">The number to find the square root of.</param>
    /// <param name="p">The modulus.</param>
    /// <returns>The modular square root of <paramref name="a"/> modulo <paramref name="p"/>.</returns>
    public static BigInteger ModularSquareRoot(BigInteger a, BigInteger p)
    {
        return BigInteger.ModPow(a, (p + 1) / 4, p);
    }
    
    /// <summary>
    /// Multiplies a point on the elliptic curve by a scalar.
    /// </summary>
    /// <param name="p">The point on the elliptic curve.</param>
    /// <param name="n">The scalar to multiply the point by.</param>
    /// <param name="N">The order of the elliptic curve.</param>
    /// <param name="A">The coefficient A of the elliptic curve equation.</param>
    /// <param name="P">The prime modulus of the elliptic curve.</param>
    /// <returns>The resulting point after multiplication.</returns>
    public static Point Multiply(Point p, BigInteger n, BigInteger N, BigInteger A, BigInteger P)
    {
        return FromJacobian(JacobianMultiply(ToJacobian(p), n, N, A, P), P);
    }
    
    /// <summary>
    /// Adds two points on the elliptic curve.
    /// </summary>
    /// <param name="p">The first point on the elliptic curve.</param>
    /// <param name="q">The second point on the elliptic curve.</param>
    /// <param name="A">The coefficient A of the elliptic curve equation.</param>
    /// <param name="P">The prime modulus of the elliptic curve.</param>
    /// <returns>The resulting point after addition.</returns>
    public static Point Add(Point p, Point q, BigInteger A, BigInteger P)
    {
        return FromJacobian(JacobianAdd(ToJacobian(p), ToJacobian(q), A, P), P);
    }
    
    /// <summary>
    /// Computes the modular inverse of a number using the Extended Euclidean Algorithm.
    /// </summary>
    /// <param name="x">The number to find the inverse of.</param>
    /// <param name="n">The modulus.</param>
    /// <returns>The modular inverse of <paramref name="x"/> modulo <paramref name="n"/>.</returns>
    /// <remarks>
    /// The algorithm works by iteratively applying the Euclidean algorithm to find the greatest common divisor (GCD) of `x` and `n`,
    /// while keeping track of coefficients that express the GCD as a linear combination of `x` and `n`.
    /// These coefficients are then used to compute the modular inverse.
    /// </remarks>
    public static BigInteger Inv(BigInteger x, BigInteger n)
    {
        if (x == 0)
        {
            return 0;
        }

        BigInteger lm = 1;
        BigInteger hm = 0;
        var low = x % n;
        var high = n;

        while (low > 1)
        {
            var r = high / low;
            var nm = hm - lm * r;
            var nw = high - low * r;
            high = low;
            hm = lm;
            low = nw;
            lm = nm;
        }

        return (lm % n + n) % n;
    }
    
    /// <summary>
    /// Multiplies a point on the elliptic curve in Jacobian coordinates by a scalar.
    /// </summary>
    /// <param name="p">The point on the elliptic curve in Jacobian coordinates.</param>
    /// <param name="n">The scalar to multiply the point by.</param>
    /// <param name="N">The order of the elliptic curve.</param>
    /// <param name="A">The coefficient A of the elliptic curve equation.</param>
    /// <param name="P">The prime modulus of the elliptic curve.</param>
    /// <returns>The resulting point after multiplication in Jacobian coordinates.</returns>
    /// <remarks>
    /// The algorithm uses a recursive approach to perform the multiplication:
    /// - If the y-coordinate of the point is zero or the scalar is zero, it returns the identity point.
    /// - If the scalar is one, it returns the point itself.
    /// - If the scalar is negative or greater than or equal to the order of the curve, it reduces the scalar modulo the order.
    /// - If the scalar is even, it recursively doubles the point and halves the scalar.
    /// - If the scalar is odd, it recursively doubles the point, halves the scalar, and adds the original point.
    /// </remarks>
    public static Point JacobianMultiply(Point p, BigInteger n, BigInteger N, BigInteger A, BigInteger P)
    {
        if (p.Y == 0 || n == 0)
        {
            return new Point(0, 0, 1);
        }

        if (n == 1)
        {
            return p;
        }
    
        if (n < 0 || n >= N)
        {
            return JacobianMultiply(p, n % N, N, A, P);
        }

        if (n % 2 == 0)
        {
            return JacobianDouble(JacobianMultiply(p, n / 2, N, A, P), A, P);
        }

        return JacobianAdd(
            JacobianDouble(
                JacobianMultiply(p, n / 2, N, A, P), A, P
            ), p, A, P
        );
    }


    /// <summary>
    /// Converts a point from Jacobian coordinates to affine coordinates.
    /// </summary>
    /// <param name="point">The point in Jacobian coordinates.</param>
    /// <param name="primeNumber">The prime modulus of the elliptic curve.</param>
    /// <returns>The point in affine coordinates.</returns>
    public static Point FromJacobian(Point point, BigInteger primeNumber)
    {
        BigInteger z = Inv(point.Z, primeNumber);
        BigInteger x = (point.X * BigInteger.ModPow(z, 2, primeNumber)) % primeNumber;
        BigInteger y = (point.Y * BigInteger.ModPow(z, 3, primeNumber)) % primeNumber;

        return new Point(x, y, BigInteger.Zero);
    }
    
    /// <summary>
    /// Adds two points on the elliptic curve in Jacobian coordinates.
    /// </summary>
    /// <param name="p">The first point on the elliptic curve in Jacobian coordinates.</param>
    /// <param name="q">The second point on the elliptic curve in Jacobian coordinates.</param>
    /// <param name="primeModulus">The prime modulus of the elliptic curve.</param>
    /// <param name="coefficientA">The coefficient A of the elliptic curve equation.</param>
    /// <returns>The resulting point after addition in Jacobian coordinates.</returns>
    /// <remarks>
    /// The algorithm handles special cases where one of the points is the identity point (0, 0, 0).
    /// It computes intermediate values U1, U2, S1, S2, H, R, H2, H3, U1H2, nx, ny, and nz to perform the addition.
    /// </remarks>
    public static Point JacobianAdd(Point p, Point q, BigInteger primeModulus, BigInteger coefficientA)
    {
        if (p.Y == 0)
            return q;
        if (q.Y == 0)
            return p;

        BigInteger U1 = (p.X * BigInteger.ModPow(q.Z, 2, coefficientA)) % coefficientA;
        if (U1 < 0) U1 += coefficientA;

        BigInteger U2 = (q.X * BigInteger.ModPow(p.Z, 2, coefficientA)) % coefficientA;
        if (U2 < 0) U2 += coefficientA;

        BigInteger S1 = (p.Y * BigInteger.ModPow(q.Z, 3, coefficientA)) % coefficientA;
        if (S1 < 0) S1 += coefficientA;

        BigInteger S2 = (q.Y * BigInteger.ModPow(p.Z, 3, coefficientA)) % coefficientA;
        if (S2 < 0) S2 += coefficientA;

        if (U1 == U2)
        {
            if (S1 != S2)
                return new Point(0, 0, 1);
            return JacobianDouble(p, primeModulus, coefficientA);
        }

        BigInteger H = U2 - U1;
        BigInteger R = S2 - S1;
        BigInteger H2 = (H * H) % coefficientA;
        if (H2 < 0) H2 += coefficientA;

        BigInteger H3 = (H * H2) % coefficientA;
        if (H3 < 0) H3 += coefficientA;

        BigInteger U1H2 = (U1 * H2) % coefficientA;
        if (U1H2 < 0) U1H2 += coefficientA;

        BigInteger nx = (BigInteger.ModPow(R, 2, coefficientA) - H3 - 2 * U1H2) % coefficientA;
        if (nx < 0) nx += coefficientA;

        BigInteger ny = (R * (U1H2 - nx) - S1 * H3) % coefficientA;
        if (ny < 0) ny += coefficientA;

        BigInteger nz = (H * p.Z * q.Z) % coefficientA;
        if (nz < 0) nz += coefficientA;

        return new Point(nx, ny, nz);
    }


    /// <summary>
    /// Doubles a point on the elliptic curve in Jacobian coordinates.
    /// </summary>
    /// <param name="p">The point on the elliptic curve in Jacobian coordinates.</param>
    /// <param name="A">The coefficient A of the elliptic curve equation.</param>
    /// <param name="P">The prime modulus of the elliptic curve.</param>
    /// <returns>The resulting point after doubling in Jacobian coordinates.</returns>
    /// <remarks>
    /// The algorithm handles the special case where the y-coordinate of the point is zero,
    /// which results in the identity point (0, 0, 0).
    /// It computes intermediate values ysq, S, M, M2, nx, ny, and nz to perform the doubling.
    /// </remarks>
    public static Point JacobianDouble(Point p, BigInteger A, BigInteger P)
    {
        if (p.Y == 0)
        {
            return new Point(0, 0, 0);
        }
        
        BigInteger ysq = BigInteger.ModPow(p.Y, 2, P);
        BigInteger S = (4 * p.X * ysq) % P;
        BigInteger M = (3 * BigInteger.ModPow(p.X, 2, P) + A * BigInteger.ModPow(p.Z, 4, P)) % P;
        BigInteger M2 = BigInteger.ModPow(M, 2, P); // M^2 % P
        BigInteger nx = (M2 - 2 * S) % P;
        if (nx < 0) nx += P;
        
        BigInteger ny = (M * (S - nx) - 8 * BigInteger.ModPow(ysq, 2, P)) % P;
        if (ny < 0) ny += P;
        
        BigInteger nz = (2 * p.Y * p.Z) % P;
        if (nz < 0) nz += P;

        return new Point(nx, ny, nz);
    }

    /// <summary>
    /// Converts a point from affine coordinates to Jacobian coordinates.
    /// </summary>
    /// <param name="p">The point in affine coordinates.</param>
    /// <returns>The point in Jacobian coordinates.</returns>
    public static Point ToJacobian(Point p)
    {
        return new Point(p.X, p.Y, 1);
    }
    
}