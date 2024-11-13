using System.Numerics;

namespace ECDSA.Utils
{
    public static class HexBigIntegerConvertor
    {
        private static byte[] HexToByteArray(string hex)
        {
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hex = hex.Substring(2);
            }
            
            if (hex.Length % 2 != 0)
            {
                hex = "0" + hex;
            }
            
            byte[] byteArray = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                byteArray[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return byteArray;
        }
        
        public static BigInteger HexToBigInteger(string hex, bool isHexLittleEndian = false)
        {
            if (hex == "0x0") return 0;

            var encoded = HexToByteArray(hex);

            if (BitConverter.IsLittleEndian != isHexLittleEndian)
            {
                var listEncoded = encoded.ToList();
                listEncoded.Insert(0, 0x00);
                encoded = listEncoded.ToArray().Reverse().ToArray();
            }
            return new BigInteger(encoded);
        }
    }
}
