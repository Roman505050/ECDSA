using System.Numerics;

namespace ECDSA.Utils;

public class BigIntegerRandom
{
    private static readonly Random Random = new Random();

    public static BigInteger Between(BigInteger min, BigInteger max)
    {
        if (min > max)
            throw new ArgumentException("Min value should be less than or equal to max value.");

        BigInteger range = max - min + 1;
        byte[] bytes = range.ToByteArray();
        BigInteger result;

        do
        {
            Random.NextBytes(bytes);
            result = new BigInteger(bytes);
        } while (result < 0 || result >= range);

        return result + min;
    }
}
