using System.Numerics;

namespace ECDSA;

public static class Program
{
    private static void Main()
    {
        
        var curve = Curve.Secp256k1;

        var privateKey = new PrivateKey(curve, new BigInteger(1234567890));
        var publicKey = privateKey.PublicKey();
        
        var message = "Hello, ECDSA!";
        
        var signature = privateKey.Sign(message);
        Console.WriteLine($"signature: {signature}");
        var isValid = publicKey.Verify(message, signature);
        Console.WriteLine($"isValid: {isValid}");
    }
}

