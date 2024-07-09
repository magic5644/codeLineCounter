using System.Security.Cryptography;
using System.Text;

namespace CodeLineCounter.Utils
{
    public static class HashUtils
    {
        public static string ComputeHash(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
            return "";
            }

            using (SHA256 sha256 = SHA256.Create())
            {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
            }
        }
    }
}
