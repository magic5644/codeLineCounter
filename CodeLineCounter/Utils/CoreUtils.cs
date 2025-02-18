using CodeLineCounter.Exceptions;
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
                        break;
                    case "-verbose":
                        settings.Verbose = true;
                        break;
                    case "-d":
                        HandleDirectory(args, settings, ref argIndex);
                        break;
                    case "-format":
                        HandleFormat(args, settings, ref argIndex);
                        break;
                    case "-output":
                        HandleOutput(args, settings, ref argIndex);
                        break;
                    default:
                        break;
                }
                argIndex++;
            }
            return settings;
        }

        private static void HandleDirectory(string[] args, Settings settings, ref int argIndex)
        {
            if (argIndex + 1 < args.Length)
            {
                settings.DirectoryPath = args[argIndex + 1];
                argIndex++;
            }

        }

        private static void HandleFormat(string[] args, Settings settings, ref int argIndex)
        {
            if (argIndex + 1 < args.Length)
            {
                string formatString = args[argIndex + 1];
                argIndex++;
                if (Enum.TryParse<ExportFormat>(formatString, true, out ExportFormat result))
                {
                    settings.Format = result;
                }
                else
                {
                    throw new InvalidExportFormatException(formatString, settings);
                }
            }
        }

        private static void HandleOutput(string[] args, Settings settings, ref int argIndex)
        {
            if (argIndex + 1 < args.Length)
            {
                settings.OutputPath = args[argIndex + 1];
                argIndex++;
            }
        }


        /// <summary>
        /// Checks user input and returns a valid choice.
        /// </summary>
        /// <param name="inputFromConsole">The user input from the console.</param>
        /// <param name="solutionCount">The number of solutions to choose from.</param>
        /// <returns>The user's choice.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="inputFromConsole"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="solutionCount"/> is less than 1.</exception>
        public static int CheckInputUserChoice(string? inputFromConsole, int solutionCount)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(solutionCount, 1);

            int choice = -1;

            string? input = inputFromConsole;

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentNullException(nameof(inputFromConsole), "No input provided. Please enter a valid number.");
            }

            if (!int.TryParse(input, out choice))
            {
                throw new InvalidNumberException();
            }

            if (choice < 1 || choice > solutionCount)
            {
                throw new InvalidSelectionException(solutionCount);
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

        public static string GetExportFileNameWithExtension(string filePath, ExportFormat format)
        {
            if (filePath == null)
            {
                filePath = "export.";
            }
            string fileName = Path.GetFileName(filePath);
            
            string newExtension = format switch
            {
                ExportFormat.CSV => ".csv",
                ExportFormat.JSON => ".json",
                _ => throw new ArgumentException($"Unsupported format: {format}", nameof(format))
            };

            string currentExtension = Path.GetExtension(filePath);

            // If file already has the desired extension, keep it, otherwise change it
            if (!currentExtension.Equals(newExtension, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(currentExtension))
                {
                    fileName = Path.ChangeExtension(fileName, newExtension);
                }
                else
                {
                    fileName += newExtension;
                }
            }
            else 
            {
                fileName = filePath;
            }

            // If an output directory is specified, combine the path
            return fileName;
        }
    }
}