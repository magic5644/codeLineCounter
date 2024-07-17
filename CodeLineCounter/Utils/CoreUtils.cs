

namespace CodeLineCounter.Utils
{
    public static class CoreUtils
    {
        public static (bool Verbose, string? DirectoryPath, bool Help) ParseArguments(string[] args)
        {
            bool verbose = false;
            bool help = false;
            string? directoryPath = null;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-help":
                        help =true;
                        break;
                    case "-verbose":
                        verbose = true;
                        break;
                    case "-d" when i + 1 < args.Length:
                        directoryPath = args[i + 1];
                        i++;
                        break;
                }
            }
            return (verbose, directoryPath, help);
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
            List<string> listOfFilenames = new List<string>();
            for (int i = 0; i < solutionFiles.Count; i++)
            {
                if (File.Exists(solutionFiles[i])) {
                    listOfFilenames.Add(Path.GetFileName(solutionFiles[i]));
                } else 
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

        public static bool CheckSettings((bool Verbose, string? DirectoryPath, bool Help) settings)
        {
            if (settings.Help)
            {
                Console.WriteLine("Usage: CodeLineCounter.exe [-verbose] [-d <directory_path>] [-help, -h]");
                return false;
            }

            if (settings.DirectoryPath == null)
            {
                Console.WriteLine("Please provide the directory path containing the solutions to analyze using the -d switch.");
                return false;
            }

            return true;
        }
    }

}