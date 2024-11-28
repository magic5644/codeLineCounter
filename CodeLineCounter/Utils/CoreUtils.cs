

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

        public static int GetUserChoice(int solutionCount)
        {
            Console.Write("Choose a solution to analyze (enter the number): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= solutionCount)
            {
                return choice;
            }
            else
            {
                Console.WriteLine("Invalid selection. Please restart the program and choose a valid option.");
                return -1;
            }
        }

        public static List<string> GetFilenamesList(List<string> solutionFiles)
        {
            List<string> listOfFilenames = [];
            for (int i = 0; i < solutionFiles.Count; i++)
            {
                if (File.Exists(solutionFiles[i]))
                {
                    listOfFilenames.Add(Path.GetFileName(solutionFiles[i]));
                }
                else
                {
                    listOfFilenames.Add(solutionFiles[i]);
                }
            }

            return listOfFilenames;

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
            switch (format)
            {
                case CoreUtils.ExportFormat.CSV:
                    filePath = Path.ChangeExtension(filePath, ".csv");
                    break;
                case CoreUtils.ExportFormat.JSON:
                    filePath = Path.ChangeExtension(filePath, ".json");
                    break;
            }

            return filePath;
        }
    }

}