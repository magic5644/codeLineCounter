using System.Security.Cryptography;
using System.Text;

namespace CodeLineCounter.Utils
{
    public static class HashUtils
    {
        private static readonly IHashUtils _defaultImplementation = new HashUtilsService();
        
        internal static IHashUtils Implementation { get; } = _defaultImplementation;
        public static string ComputeHash(string? input)
        {
            return Implementation.ComputeHash(input);
        }
    }
}