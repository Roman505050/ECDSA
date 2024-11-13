using System.Numerics;
using System.Text;
using System.Security.Cryptography;

namespace ECDSA;

public class PublicKey
{
    public Point Point { get; }
    public Curve Curve { get; }

    public PublicKey(Point point, Curve curve)
    {
        this.Point = point;
        this.Curve = curve;
    }
    
    public bool Verify(string message, Signature signature)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var hashBytes = SHA256.HashData(messageBytes);
        var numberMessage = new BigInteger(hashBytes.Reverse().ToArray());
        var curve = this.Curve;
        
        if (signature.R < 1 || signature.R > curve.N - 1)
        {
            return false;
        }
        
        if (signature.S < 1 || signature.S > curve.N - 1)
        {
            return false;
        }
        
        var w = MathHelper.Inv(signature.S, curve.N);
        var u1 = (numberMessage * w) % curve.N;
        var u2 = (signature.R * w) % curve.N;
        var point = MathHelper.Add(MathHelper.Multiply(curve.G, u1, curve.N, curve.A, curve.P), MathHelper.Multiply(this.Point, u2, curve.N, curve.A, curve.P), curve.A, curve.P);
        
        if (point.X == 0 && point.Y == 0)
        {
            return false;
        }
        
        return point.X % curve.N == signature.R;
    }
    
    public byte[] ToDer()
    {
        using (var stream = new MemoryStream())
        {
            // Encode R and S as ASN.1 INTEGERs
            byte[] rBytes = Point.X.ToByteArray();
            byte[] sBytes = Point.Y.ToByteArray();
        
            stream.WriteByte(0x30); // ASN.1 SEQUENCE tag
            stream.WriteByte((byte)(rBytes.Length + sBytes.Length + 4)); // Length

            stream.WriteByte(0x02); // INTEGER tag
            stream.WriteByte((byte)rBytes.Length);
            stream.Write(rBytes, 0, rBytes.Length);

            stream.WriteByte(0x02); // INTEGER tag
            stream.WriteByte((byte)sBytes.Length);
            stream.Write(sBytes, 0, sBytes.Length);
            
            byte[] oidBytes = CurveHelper.ConvertOidToBytes(Curve.Oid);

            stream.WriteByte(0x06); // OBJECT IDENTIFIER tag
            stream.WriteByte((byte)oidBytes.Length);
            stream.Write(oidBytes, 0, oidBytes.Length);
            
            return stream.ToArray();
        }
    }

    public static PublicKey FromDer(byte[] derBytes)
    {
        
        using (var stream = new MemoryStream(derBytes))
        {
            if (stream.ReadByte() != 0x30) throw new Exception("Invalid DER encoding.");
        
            stream.ReadByte(); // Skip length byte

            // Read X
            if (stream.ReadByte() != 0x02) throw new Exception("Invalid DER encoding.");
            int xLength = stream.ReadByte();
            byte[] xBytes = new byte[xLength];
            stream.Read(xBytes, 0, xLength);
            BigInteger x = new BigInteger(xBytes);

            // Read Y
            if (stream.ReadByte() != 0x02) throw new Exception("Invalid DER encoding.");
            int yLength = stream.ReadByte();
            byte[] yBytes = new byte[yLength];
            stream.Read(yBytes, 0, yLength);
            BigInteger y = new BigInteger(yBytes);
            
            // Read OID
            if (stream.ReadByte() != 0x06) throw new Exception("Invalid DER encoding.");
            int oidLength = stream.ReadByte();
            byte[] oidBytes = new byte[oidLength];
            stream.Read(oidBytes, 0, oidLength);
            List<int> oid = CurveHelper.ConvertBytesToOid(oidBytes);

            Curve curve = CurveHelper.GetCurveByOid(oid);
            Point point = new Point(x, y, 0);
            
            return new PublicKey(point, curve);
        }
    }
    
    public string ToBase64()
    {
        byte[] derBytes = ToDer();
        return Convert.ToBase64String(derBytes);
    }
    
    public static PublicKey FromBase64(string base64)
    {
        byte[] derBytes = Convert.FromBase64String(base64);
        return FromDer(derBytes);
    }
}
