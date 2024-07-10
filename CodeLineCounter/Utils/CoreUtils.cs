using System.Text.RegularExpressions; // Add this line

namespace CodeLineCounter.Utils
{
    public static class CoreUtils
    {
        public static (bool Verbose, string? DirectoryPath) ParseArguments(string[] args)
        {
            bool verbose = false;
            string? directoryPath = null;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-verbose":
                        verbose = true;
                        break;
                    case "-d" when i + 1 < args.Length:
                        directoryPath = args[i + 1];
                        i++;
                        break;
                }
            }
            return (verbose, directoryPath);
        }
    }
}