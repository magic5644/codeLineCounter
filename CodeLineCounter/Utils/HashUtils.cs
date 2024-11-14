using System.Security.Cryptography;
using System.Text;

namespace CodeLineCounter.Utils
{
    public static class HashUtils
    {
        public static string ComputeHash(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            StringBuilder builder = new();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
