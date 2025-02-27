using System.Security.Cryptography;
using System.Text;

namespace CodeLineCounter.Utils
{
    public class HashUtilsService : IHashUtils
    {
        public string ComputeHash(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));

            return string.Create(bytes.Length * 2, bytes, static (span, byteArray) =>
            {
                const string format = "x2";
                for (int i = 0; i < byteArray.Length; i++)
                {
                    byteArray[i].TryFormat(span.Slice(i * 2, 2), out _, format);
                }
            });
        }
    }
}