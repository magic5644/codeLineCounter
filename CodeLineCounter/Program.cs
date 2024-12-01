﻿using CodeLineCounter.Services;
using CodeLineCounter.Utils;
using CodeLineCounter.Models;
using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace CodeLineCounter
{
    static class Program
    {
        static void Main(string[] args)
        {
            var settings = CoreUtils.ParseArguments(args);
            if (CoreUtils.CheckSettings(settings) && settings.DirectoryPath != null)
            {
                // file deepcode ignore PT: Not a web server. This software is a console application.
                var solutionFiles = FileUtils.GetSolutionFiles(settings.DirectoryPath);
                if (solutionFiles.Count == 0)
                {
                    Console.WriteLine("No solution (.sln) found in the specified directory.");
                    return;
                }

                var solutionFilenameList = CoreUtils.GetFilenamesList(solutionFiles);

                CoreUtils.DisplaySolutions(solutionFilenameList);
                int choice = CoreUtils.GetUserChoice(solutionFiles.Count);
                if (choice == -1) return;

                var solutionPath = Path.GetFullPath(solutionFiles[choice - 1]);
                AnalyzeAndExportSolution(solutionPath, settings.Verbose, settings.format);
            }
        }

        private static void AnalyzeAndExportSolution(string solutionPath, bool verbose, CoreUtils.ExportFormat format )
        {
            var timer = new Stopwatch();
            timer.Start();
            string solutionFilename = Path.GetFileName(solutionPath);
            string csvFilePath = $"{solutionFilename}-CodeMetrics.csv";
            csvFilePath = CoreUtils.GetExportFileNameWithExtension(csvFilePath, format);
            string duplicationCsvFilePath = $"{solutionFilename}-CodeDuplications.csv";
            duplicationCsvFilePath = CoreUtils.GetExportFileNameWithExtension(duplicationCsvFilePath, format);

            var (metrics, projectTotals, totalLines, totalFiles, duplicationMap) = CodeAnalyzer.AnalyzeSolution(solutionPath);
            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed;
            string processingTime = $"Time taken: {timeTaken:m\\:ss\\.fff}";
            var nbLinesDuplicated = duplicationMap.Sum(x => x.NbLines);
            var percentageDuplication = (nbLinesDuplicated / (double)totalLines) * 100;

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
            Console.WriteLine($"Solution {solutionFilename} has {nbLinesDuplicated} duplicated lines of code.");
            Console.WriteLine($"Percentage of duplicated code: {percentageDuplication:F2} %");

            Parallel.Invoke(
                //() => CsvExporter.ExportToCsv(csvFilePath, metrics, projectTotals, totalLines, duplicationMap, solutionPath),
                //() => CsvExporter.ExportCodeDuplicationsToCsv(duplicationCsvFilePath, duplicationMap),
                () => DataExporter.ExportMetrics(csvFilePath, metrics, projectTotals, totalLines, duplicationMap, solutionPath, format),
                () => DataExporter.ExportDuplications(duplicationCsvFilePath, duplicationMap, format)
            );

            Console.WriteLine($"The data has been exported to {csvFilePath}");
            Console.WriteLine($"The code duplications have been exported to {duplicationCsvFilePath}");
            Console.WriteLine(processingTime);
        }
    }
}