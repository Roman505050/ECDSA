namespace ECDSA;

public class CurveHelper
{
    public static Curve GetCurveByOid(List<int> oid)
    {
        Dictionary<string, Curve> curves = new()
        {
            { "1.2.840.10045.3.1.7", Curve.Prime256v1 },
            { "1.3.132.0.10", Curve.Secp256k1 },
        };
        
        string key = string.Join(".", oid);
        
        if (curves.ContainsKey(key))
        {
            return curves[key];
        }
        
        throw new InvalidOperationException("Unsupported curve OID");
    }
    
    public static byte[] ConvertOidToBytes(List<int> oid)
    {
        using (var stream = new MemoryStream())
        {
            foreach (var item in oid)
            {
                int value = item;
                if (value < 0x80)
                {
                    stream.WriteByte((byte)value);
                }
                else
                {
                    var bytes = new List<byte>();
                    while (value > 0)
                    {
                        bytes.Add((byte)(value & 0x7F));
                        value >>= 7;
                    }
                    bytes.Reverse();
                    
                    bytes[0] |= 0x80;
                    stream.Write(bytes.ToArray(), 0, bytes.Count);
                }
            }

            return stream.ToArray();
        }
    }
    
    public static List<int> ConvertBytesToOid(byte[] bytes)
    {
        var oid = new List<int>();
        int value = 0;

        foreach (var b in bytes)
        {
            if ((b & 0x80) == 0)
            {
                value = (value << 7) | b;
                oid.Add(value);
                value = 0;
            }
            else
            {
                value = (value << 7) | (b & 0x7F);
            }
        }

        return oid;
    }
}
