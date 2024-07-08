using CodeLineCounter.Services;
using CodeLineCounter.Utils;
using System.Diagnostics;
using System;
using System.IO;
using System.Linq;

namespace CodeLineCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = ParseArguments(args);
            if (settings.DirectoryPath == null)
            {
                Console.WriteLine("Please provide the directory path containing the solutions to analyze using the -d switch.");
                return;
            }

            var solutionFiles = FileUtils.GetSolutionFiles(settings.DirectoryPath);
            if (solutionFiles.Count == 0)
            {
                Console.WriteLine("No solution (.sln) found in the specified directory.");
                return;
            }

            DisplaySolutions(solutionFiles);
            int choice = GetUserChoice(solutionFiles.Count);
            if (choice == -1) return;

            AnalyzeAndExportSolution(solutionFiles[choice - 1], settings.Verbose);
        }

        private static (bool Verbose, string? DirectoryPath) ParseArguments(string[] args)
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

        private static void DisplaySolutions(System.Collections.Generic.List<string> solutionFiles)
        {
            Console.WriteLine("Available solutions:");
            for (int i = 0; i < solutionFiles.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {Path.GetFileName(solutionFiles[i])}");
            }
        }

        private static int GetUserChoice(int solutionCount)
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

        private static void AnalyzeAndExportSolution(string solutionPath, bool verbose)
        {
            var timer = new Stopwatch();
            timer.Start();
            string solutionFilename = Path.GetFileName(solutionPath);
            string csvFilePath = $"{solutionFilename}-CodeMetrics.csv";

            var analyzer = new CodeAnalyzer();
            var (metrics, projectTotals, totalLines, totalFiles, duplicationMap) = analyzer.AnalyzeSolution(solutionPath);
            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed;
            string processingTime = $"Time taken: {timeTaken:m\\:ss\\.fff}";

            if (verbose)
            {
                foreach (var metric in metrics)
                {
                    Console.WriteLine($"Project {metric.ProjectName} ({metric.ProjectPath}) - Namespace {metric.NamespaceName} in file {metric.FileName} ({metric.FilePath}) has {metric.LineCount} lines of code and a cyclomatic complexity of {metric.CyclomaticComplexity}.");
                }

                foreach (var projectTotal in projectTotals)
                {
                    Console.WriteLine($"Project {projectTotal.Key} has {projectTotal.Value} total lines of code.");
                }
            }
            Console.WriteLine($"Processing completed, number of source files processed: {totalFiles}");
            Console.WriteLine($"Total lines of code: {totalLines}");

            CsvExporter.ExportToCsv(csvFilePath, metrics.ToList(), projectTotals, totalLines);
            Console.WriteLine($"The data has been exported to {csvFilePath}");
            Console.WriteLine(processingTime);
        }
    }
}