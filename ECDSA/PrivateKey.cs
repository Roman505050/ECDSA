using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using ECDSA.Utils;

namespace ECDSA
{
    public class PrivateKey
    {
        private Curve Curve { get; }
        private BigInteger Secret { get; }

        public PrivateKey(Curve curve, BigInteger secret)
        {
            this.Curve = curve;
            this.Secret = secret;
        }

        public PublicKey PublicKey()
        {
            var publicPoint = MathHelper.Multiply(this.Curve.G, this.Secret, this.Curve.N, this.Curve.A, this.Curve.P);
            return new PublicKey(publicPoint, this.Curve);
        }

        public Signature Sign(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var hashBytes = SHA256.HashData(messageBytes);
            var numberMessage = new BigInteger(hashBytes.Reverse().ToArray());
            var curve = this.Curve;

            BigInteger r;
            BigInteger s;
            Point randSignPoint;
            do
            {
                var randNum = BigIntegerRandom.Between(1, curve.N - 1);
                randSignPoint = MathHelper.Multiply(curve.G, randNum, curve.N, curve.A, curve.P);
                r = randSignPoint.X % curve.N;
                s = ((numberMessage + r * this.Secret) * (MathHelper.Inv(randNum, curve.N))) % curve.N;
            } while (r == 0 || s == 0);

            int recoveryId = (int)(randSignPoint.Y & 1);
            if (randSignPoint.Y > curve.N)
            {
                recoveryId += 2;
            }

            return new Signature(r, s, recoveryId);
        }

        public byte[] ToDer()
        {
            byte[] secretBytes = Secret.ToByteArray(true, true);
            byte[] oidBytes = CurveHelper.ConvertOidToBytes(Curve.Oid);

            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write((byte)0x30); // SEQUENCE
                    using (var innerMs = new MemoryStream())
                    {
                        using (var innerWriter = new BinaryWriter(innerMs))
                        {
                            innerWriter.Write((byte)0x06); // OBJECT IDENTIFIER (OID)
                            innerWriter.Write((byte)oidBytes.Length);
                            innerWriter.Write(oidBytes);

                            // Write private key as INTEGER (0x02), which is how it is expected in the DER format
                            innerWriter.Write((byte)0x04); // OCTET STRING for secret key
                            innerWriter.Write((byte)(secretBytes.Length)); // Length of the private key
                            innerWriter.Write(secretBytes);

                            byte[] innerBytes = innerMs.ToArray();
                            writer.Write((byte)innerBytes.Length); // Write length of the inner sequence
                            writer.Write(innerBytes);
                        }
                    }

                    return ms.ToArray();
                }
            }
        }

        public string ToBase64()
        {
            byte[] derBytes = ToDer();
            return Convert.ToBase64String(derBytes);
        }

        public static PrivateKey FromDer(byte[] derBytes)
        {
            using (var ms = new MemoryStream(derBytes))
            {
                using (var reader = new BinaryReader(ms))
                {
                    if (reader.ReadByte() != 0x30) // SEQUENCE
                        throw new InvalidDataException("Expected SEQUENCE in DER format");

                    int length = reader.ReadByte();
                    if (length > 0x80)
                    {
                        int lengthBytes = length & 0x7F;
                        length = 0;
                        for (int i = 0; i < lengthBytes; i++)
                        {
                            length = (length << 8) | reader.ReadByte();
                        }
                    }

                    // Read OID (Curve identifier)
                    byte identifier = reader.ReadByte();
                    if (identifier != 0x06) // OBJECT IDENTIFIER (OID)
                        throw new InvalidDataException("Expected OBJECT IDENTIFIER (OID)");

                    int oidLength = reader.ReadByte();
                    byte[] oidBytes = reader.ReadBytes(oidLength);

                    var oid = CurveHelper.ConvertBytesToOid(oidBytes);
                    var curve = CurveHelper.GetCurveByOid(oid);
                    

                    // Read the private key (OCTET STRING)
                    identifier = reader.ReadByte();
                    if (identifier != 0x04) // OCTET STRING for private key
                        throw new InvalidDataException("Expected OCTET STRING for the private key");

                    int secretLength = reader.ReadByte();
                    if (secretLength > 0x80)
                    {
                        int lengthBytes = secretLength & 0x7F;
                        secretLength = 0;
                        for (int i = 0; i < lengthBytes; i++)
                        {
                            secretLength = (secretLength << 8) | reader.ReadByte();
                        }
                    }

                    byte[] secretBytes = reader.ReadBytes(secretLength);

                    BigInteger secret = new BigInteger(secretBytes, true, true);
                    return new PrivateKey(curve, secret);
                }
            }
        }

        public static PrivateKey FromBase64(string base64String)
        {
            byte[] derBytes = Convert.FromBase64String(base64String);
            return FromDer(derBytes);
        }
        
    }
}
