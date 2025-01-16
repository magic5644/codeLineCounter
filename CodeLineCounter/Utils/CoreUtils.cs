using CodeLineCounter.Models;

namespace CodeLineCounter.Utils
{
    public static class CoreUtils
    {
        public enum ExportFormat
        {
            CSV,
            JSON
        }

        public static Settings ParseArguments(string[] args)
        {
            var settings = new Settings();
            int argIndex = 0;
            
            while (argIndex < args.Length)
            {
                switch (args[argIndex])
                {
                    case "-help":
                        settings.Help = true;
                        argIndex++;
                        break;
                    case "-verbose":
                        settings.Verbose = true;
                        argIndex++;
                        break;
                    case "-d":
                        if (argIndex + 1 < args.Length)
                        {
                            settings.DirectoryPath = args[argIndex + 1];
                            argIndex += 2;
                        }
                        else
                        {
                            argIndex++;
                        }
                        break;
                    case "-format":
                        if (argIndex + 1 < args.Length)
                        {
                            string formatString = args[argIndex + 1];
                            argIndex += 2;
                            if (Enum.TryParse<ExportFormat>(formatString, true, out ExportFormat result))
                            {
                                settings.Format = result;
                            }
                            else
                            {
                                Console.WriteLine($"Invalid format: {formatString}. Valid formats are: {string.Join(", ", Enum.GetNames<ExportFormat>())}. Using default format {settings.Format}");
                            }
                        }
                        break;
                    case "-output":
                        if (argIndex + 1 < args.Length)
                        {
                            settings.OutputPath = args[argIndex + 1];
                            argIndex += 2;
                        }
                        else
                        {
                            argIndex++;
                        }
                        break;
                    default:
                        argIndex++;
                        break;
                }
            }
            return settings;
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

        public static string GetExportFileNameWithExtension(string filePath, CoreUtils.ExportFormat format, string? outputPath = null)
        {
            string fileName = Path.GetFileName(filePath);
            if (filePath == null)
            {
                filePath = ".";
            }
            string newExtension = format switch
            {
                CoreUtils.ExportFormat.CSV => ".csv",
                CoreUtils.ExportFormat.JSON => ".json",
                _ => throw new ArgumentException($"Unsupported format: {format}", nameof(format))
            };

            // If file already has the desired extension, keep it, otherwise change it
            if (!Path.GetExtension(fileName).Equals(newExtension, StringComparison.OrdinalIgnoreCase))
            {
                fileName = Path.ChangeExtension(fileName, newExtension);
            }

            // If an output directory is specified, combine the path
            return outputPath != null 
                ? Path.Combine(Path.GetFullPath(outputPath), fileName)
                : Path.Combine(Path.GetDirectoryName(filePath) ?? Path.GetFullPath("."), fileName);
        } 
    }
}