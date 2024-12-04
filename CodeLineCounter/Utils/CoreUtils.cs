
namespace CodeLineCounter.Utils
{
    public static class CoreUtils
    {
        public enum ExportFormat
        {
            CSV,
            JSON
        }
        public static (bool Verbose, string? DirectoryPath, bool Help, ExportFormat format) ParseArguments(string[] args)
        {
            bool verbose = false;
            bool help = false;
            ExportFormat format = ExportFormat.CSV;
            string? directoryPath = null;
            int argIndex = 0;
            while (argIndex < args.Length)
            {
                switch (args[argIndex])
                {
                    case "-help":
                        help = true;
                        argIndex++;
                        break;
                    case "-verbose":
                        verbose = true;
                        argIndex++;
                        break;
                    case "-d":
                        if (argIndex + 1 < args.Length)
                        {
                            directoryPath = args[argIndex + 1];
                            argIndex += 2; // Increment by 2 to skip the next argument
                        }
                        else
                        {
                            argIndex++; // Increment by 1 if there's no next argument
                        }
                        break;
                    case "-format":
                        if (argIndex + 1 < args.Length)
                        {
                            string formatString = args[argIndex + 1];
                            argIndex += 2; // Increment by 2 to skip the next argument
                            if (Enum.TryParse<ExportFormat>(formatString, true, out ExportFormat result))
                            {
                                format = result;
                            }
                            else
                            {
                                Console.WriteLine($"Invalid format: {formatString}. Valid formats are: {string.Join(", ", Enum.GetNames(typeof(ExportFormat)))}. Using default format {format}");
                            }
                        }
                        break;
                    default:
                        argIndex++;
                        break;
                }
            }
            return (verbose, directoryPath, help, format);
        }

        /// <summary>
        /// Gets a valid user selection from the available solutions.
        /// </summary>
        /// <param name="solutionCount">The total number of available solutions</param>
        /// <returns>Selected solution number (1-based index) or -1 if invalid</returns>
        public static int GetUserChoice(int solutionCount)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(solutionCount, 1);

            Console.Write($"Choose a solution to analyze (1-{solutionCount}): ");

            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("No input provided. Please enter a valid number.");
                return -1;
            }

            if (!int.TryParse(input, out int choice))
            {
                Console.WriteLine("Invalid input. Please enter a numeric value.");
                return -1;
            }

            if (choice < 1 || choice > solutionCount)
            {
                Console.WriteLine($"Selection must be between 1 and {solutionCount}.");
                return -1;
            }

            return choice;
        }

        /// <summary>
        /// Processes a list of solution file paths and extracts their filenames.
        /// </summary>
        /// <param name="solutionFiles">List of solution file paths to process</param>
        /// <returns>List of filenames or original paths for non-existing files</returns>
        /// <exception cref="ArgumentNullException">Thrown when solutionFiles is null</exception>
        public static List<string> GetFilenamesList(List<string> solutionFiles)
        {
            ArgumentNullException.ThrowIfNull(solutionFiles);

            return solutionFiles
                .Select(file => File.Exists(file)
                    ? Path.GetFileName(file)
                    : file)
                .ToList();
        }

        public static void DisplaySolutions(List<string> solutionFiles)
        {
            Console.WriteLine("Available solutions:");
            for (int i = 0; i < solutionFiles.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {solutionFiles[i]}");
            }
        }

        public static bool CheckSettings((bool Verbose, string? DirectoryPath, bool Help, ExportFormat format) settings)
        {
            if (settings.Help)
            {
                Console.WriteLine("Usage: CodeLineCounter.exe [-verbose] [-d <directory_path>] [-help, -h] (-format <csv, json>)");
                return false;
            }

            if (settings.DirectoryPath == null)
            {
                Console.WriteLine("Please provide the directory path containing the solutions to analyze using the -d switch.");
                return false;
            }

            return true;
        }

        public static string GetExportFileNameWithExtension(string filePath, CoreUtils.ExportFormat format)
        {
            string newExtension = format switch
            {
                CoreUtils.ExportFormat.CSV => ".csv",
                CoreUtils.ExportFormat.JSON => ".json",
                _ => throw new ArgumentException($"Unsupported format: {format}", nameof(format))
            };

            string currentExtension = Path.GetExtension(filePath);

            // If the file already has the desired extension (case-insensitive)
            if (currentExtension.Equals(newExtension, StringComparison.OrdinalIgnoreCase))
            {
                return filePath;
            }

            // If the file has no extension, add the new one
            if (string.IsNullOrEmpty(currentExtension))
            {
                return filePath + newExtension;
            }

            // If the file has a different extension, replace it
            return Path.ChangeExtension(filePath, newExtension);
        }
    }

}