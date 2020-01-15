using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NBitcoinCore.Model;

namespace NBitcoinCore.Uteis
{
    public class Functions
    {
        /// <summary>
        /// Converter hash em string Hexadecimal
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static string HashToString(byte[] hash)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }

            return result.ToString();
        }

        /// <summary>
        /// Converter String Hexadecimal para Bytes
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] HexStringToBytes(string hexString)
        {
            if (hexString == null)
                throw new ArgumentNullException("hexString");
            if (hexString.Length % 2 != 0)
                throw new ArgumentException("hexString must have an even length", "hexString");

            var bytes = new byte[hexString.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                string currentHex = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(currentHex, 16);
            }

            return bytes;
        }

        /// <summary>
        /// Gerar hash SHA256 do conjunto de bytes
        /// </summary>
        /// <param name="bytesFile"></param>
        /// <returns></returns>
        public static byte[] GenerateHashFile(byte[] bytesFile)
        {
            SHA256 sha256 = SHA256.Create();
            return sha256.ComputeHash(bytesFile);
        }

        /// <summary>
        /// Verifica se a string é uma string hexadecimal
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool OnlyHexInString(string str)
        {
            // For C-style hex notation (0xFF) you can use @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z"
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        /// <summary>
        /// Converte a string hexadecimal na codificação UTF8
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static string HexToUTF8(string hexString)
        {
            return Encoding.UTF8.GetString(HexStringToBytes(hexString));
        }

        public static string ConvertStringToHex(string asciiString)
        {
            string hex = "";
            foreach (char c in asciiString)
            {
                int tmp = c;
                hex += String.Format("{0:x2}", (uint)System.Convert.ToUInt32(tmp.ToString()));
            }
            return hex;
        }
    }
}
