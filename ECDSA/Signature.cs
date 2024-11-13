using System.Numerics;

namespace ECDSA;

public class Signature
{
    public BigInteger R { get; }
    public BigInteger S { get; }
    public int? RecoveryId { get; }
    
    public Signature(BigInteger r, BigInteger s, int? recoveryId = null)
    {
        this.R = r;
        this.S = s;
        this.RecoveryId = recoveryId;
    }

    public override string ToString()
    {
        return ToBase64();
    }

    public byte[] ToDer(bool includeRecoveryId = false)
    {
        using (var stream = new MemoryStream())
        {
            // Encode R and S as ASN.1 INTEGERs
            byte[] rBytes = R.ToByteArray();
            byte[] sBytes = S.ToByteArray();
        
            stream.WriteByte(0x30); // ASN.1 SEQUENCE tag
            stream.WriteByte((byte)(rBytes.Length + sBytes.Length + 4)); // Length

            stream.WriteByte(0x02); // INTEGER tag
            stream.WriteByte((byte)rBytes.Length);
            stream.Write(rBytes, 0, rBytes.Length);

            stream.WriteByte(0x02); // INTEGER tag
            stream.WriteByte((byte)sBytes.Length);
            stream.Write(sBytes, 0, sBytes.Length);

            if (includeRecoveryId && RecoveryId.HasValue)
            {
                stream.WriteByte((byte)(27 + RecoveryId.Value)); // Add recovery ID if provided
            }

            return stream.ToArray();
        }
    }

    public static Signature FromDer(byte[] derEncoded, bool hasRecoveryId = false)
    {
        using (var stream = new MemoryStream(derEncoded))
        {
            if (stream.ReadByte() != 0x30) throw new Exception("Invalid DER encoding.");
        
            stream.ReadByte(); // Skip length byte

            // Read R
            if (stream.ReadByte() != 0x02) throw new Exception("Invalid DER encoding.");
            int rLength = stream.ReadByte();
            byte[] rBytes = new byte[rLength];
            stream.Read(rBytes, 0, rLength);
            BigInteger r = new BigInteger(rBytes);

            // Read S
            if (stream.ReadByte() != 0x02) throw new Exception("Invalid DER encoding.");
            int sLength = stream.ReadByte();
            byte[] sBytes = new byte[sLength];
            stream.Read(sBytes, 0, sLength);
            BigInteger s = new BigInteger(sBytes);

            // Read RecoveryId if present
            int? recoveryId = null;
            if (hasRecoveryId && stream.Position < stream.Length)
            {
                recoveryId = stream.ReadByte() - 27;
            }

            return new Signature(r, s, recoveryId);
        }
    }
    
    public string ToBase64(bool includeRecoveryId = false)
    {
        var der = ToDer(includeRecoveryId);
        return Convert.ToBase64String(der);
    }
    
    public static Signature FromBase64(string base64, bool hasRecoveryId = false)
    {
        var der = Convert.FromBase64String(base64);
        return FromDer(der, hasRecoveryId);
    }

}