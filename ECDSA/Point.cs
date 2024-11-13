using System.Numerics;

namespace ECDSA;

public class Point
{
    public BigInteger X { get; }
    public BigInteger Y { get; }
    public BigInteger Z { get; }
    
    public Point(BigInteger x, BigInteger y, BigInteger z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

    /// <summary>
    /// Determines whether the point is at infinity.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the point is at infinity; otherwise, <c>false</c>.
    /// </returns>
    public bool IsAtInfinity()
    {
        // The point at infinity on an elliptic curve does not have defined coordinates (x, y),
        // but in mathematical terms, it can be treated as a point where Y = 0.
        // This is part of the abstraction used in cryptography for elliptic curves.

        // If a point has coordinates (X, Y), it must satisfy the curve equation:
        // Y^2 = X^3 + aX + b
        // In the case of the point at infinity, we can consider Y = 0 as a special case.
    
        // Therefore, to check if a point is at infinity, we simply check if Y = 0.
        return Y == 0;
    }
    
    /// <summary>
    /// Determines whether two <see cref="Point"/> instances are equal.
    /// </summary>
    /// <param name="p1">The first <see cref="Point"/> instance.</param>
    /// <param name="p2">The second <see cref="Point"/> instance.</param>
    /// <returns>
    /// <c>true</c> if the two <see cref="Point"/> instances are equal; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator ==(Point? p1, Point? p2)
    {
        if (ReferenceEquals(p1, p2)) return true;
    
        if (p1 is null || p2 is null) return false;
    
        return p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z;
    }

    /// <summary>
    /// Determines whether two <see cref="Point"/> instances are not equal.
    /// </summary>
    /// <param name="p1">The first <see cref="Point"/> instance.</param>
    /// <param name="p2">The second <see cref="Point"/> instance.</param>
    /// <returns>
    /// <c>true</c> if the two <see cref="Point"/> instances are not equal; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator !=(Point p1, Point p2)
    {
        return !(p1 == p2);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="Point"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="Point"/>.</param>
    /// <returns>
    /// <c>true</c> if the specified object is equal to the current <see cref="Point"/>; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        if (obj is Point other)
        {
            return this == other;
        }
        return false;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="Point"/>.
    /// </returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
}