﻿using CodeLineCounter.Services;
using CodeLineCounter.Utils;

namespace CodeLineCounter
{
    static class Program
    {
        static void Main(string[] args)
        {
            var settings = CoreUtils.ParseArguments(args);
            if (!settings.IsValid() || settings.DirectoryPath == null)
                return;

            List <string> solutionFiles = FileUtils.GetSolutionFiles(settings.DirectoryPath);
            if (solutionFiles.Count == 0)
            {
                Console.WriteLine("No solution (.sln) found in the specified directory.");
                return;
            }

            // If only one solution found in directory, select it automatically
            // if more than one waiting for user selection
            if (solutionFiles.Count == 1)
            {
                SolutionAnalyzer.AnalyzeAndExportSolution(solutionFiles[0], settings.Verbose, settings.Format, settings.OutputPath);
                return;
            }

            var solutionFilenameList = CoreUtils.GetFilenamesList(solutionFiles);
            CoreUtils.DisplaySolutions(solutionFilenameList);

            var choice = CoreUtils.GetUserChoice(solutionFiles.Count);
            if (choice == -1)
                return;

            var solutionPath = Path.GetFullPath(solutionFiles[choice - 1]);
            SolutionAnalyzer.AnalyzeAndExportSolution(solutionPath, settings.Verbose, settings.Format, settings.OutputPath);
        }
    }
}