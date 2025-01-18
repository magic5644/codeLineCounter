using CodeLineCounter.Utils;

namespace CodeLineCounter.Models
{
    public class Settings
    {

        public bool Verbose { get; set; }
        public string? DirectoryPath { get; set; }
        public string? OutputPath { get; set; }
        public bool Help { get; set; }
        public CoreUtils.ExportFormat Format { get; set; }

        public Settings()
        {
            Format = CoreUtils.ExportFormat.CSV;
            Help = false;
            Verbose = false;
            OutputPath = Directory.GetCurrentDirectory();
        }

        public Settings(bool verbose, string? directoryPath, string? outputPath, bool help, CoreUtils.ExportFormat format)
        {
            Verbose = verbose;
            DirectoryPath = directoryPath;
            OutputPath = outputPath;
            Help = help;
            Format = format;
        }


        public Settings(bool verbose, string? directoryPath, bool help, CoreUtils.ExportFormat format)
        {
            Verbose = verbose;
            DirectoryPath = directoryPath;
            OutputPath = OutputPath = Directory.GetCurrentDirectory();
            Help = help;
            Format = format;
        }

        public bool IsValid()
        {
            if (Help)
            {
                Console.WriteLine("Usage: CodeLineCounter.exe [-verbose] [-d <directory_path>] [-output <output_path>] [-help, -h] (-format <csv, json>)");
                return true;
            }

            if (DirectoryPath == null)
            {
                Console.WriteLine("Please provide the directory path containing the solutions to analyze using the -d switch.");
                return false;
            }

            if (OutputPath != null && !Directory.Exists(OutputPath))
            {
                try
                {
                    Directory.CreateDirectory(OutputPath);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Cannot create or access output directory: {OutputPath}");
                    return false;
                }
            }

            return true;
        }
    }
} 