using CodeLineCounter.Services;
using CodeLineCounter.Utils;

namespace CodeLineCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            string directoryPath;
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the directory path containing the solutions to analyze.");
                return;
            }
            else
            {
                directoryPath = args[0];
            }

            var solutionFiles = FileUtils.GetSolutionFiles(directoryPath);

            if (solutionFiles.Count == 0)
            {
                Console.WriteLine("No solution (.sln) found in the specified directory.");
                return;
            }

            Console.WriteLine("Available solutions:");
            for (int i = 0; i < solutionFiles.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {Path.GetFileName(solutionFiles[i])}");
            }

            Console.Write("Choose a solution to analyze (enter the number): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= solutionFiles.Count)
            {
                string solutionPath = solutionFiles[choice - 1];
                string solutionFilename = Path.GetFileName(solutionPath);
                string csvFilePath = solutionFilename + "-" + "CodeMetrics.csv";  // You can modify this path according to your needs

                var analyzer = new CodeAnalyzer();
                var (metrics, projectTotals, totalLines) = analyzer.AnalyzeSolution(solutionPath);

                foreach (var metric in metrics)
                {
                    Console.WriteLine($"Project {metric.ProjectName} ({metric.ProjectPath}) - Namespace {metric.NamespaceName} in file {metric.FileName} ({metric.FilePath}) has {metric.LineCount} lines of code and a cyclomatic complexity of {metric.CyclomaticComplexity}.");
                }

                foreach (var projectTotal in projectTotals)
                {
                    Console.WriteLine($"Project {projectTotal.Key} has {projectTotal.Value} total lines of code.");
                }

                Console.WriteLine($"Total lines of code: {totalLines}");

                // Export the data to CSV format
                CsvExporter.ExportToCsv(csvFilePath, metrics, projectTotals, totalLines);
                Console.WriteLine($"The data has been exported to {csvFilePath}");
            }
            else
            {
                Console.WriteLine("Invalid selection. Please restart the program and choose a valid option.");
            }
        }
    }
}
