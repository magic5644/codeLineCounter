using CodeLineCounter.Utils;
using CodeLineCounter.Models;
using System.Diagnostics;
using System.Globalization;

namespace CodeLineCounter.Services
{
    public static partial class SolutionAnalyzer
    {

        public static void AnalyzeAndExportSolution(string solutionPath, bool verbose, CoreUtils.ExportFormat format)
        {
            try
            {
                var analysisResult = PerformAnalysis(solutionPath);
                OutputAnalysisResults(analysisResult, verbose);
                ExportResults(analysisResult, solutionPath, format);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error analyzing solution: {ex.Message}");
                throw;
            }
        }

        public static AnalysisResult PerformAnalysis(string solutionPath)
        {
            var timer = new Stopwatch();
            timer.Start();

            var (metrics, projectTotals, totalLines, totalFiles, duplicationMap, dependencyList) =
                CodeMetricsAnalyzer.AnalyzeSolution(solutionPath);

            timer.Stop();

            return new AnalysisResult
            {
                Metrics = metrics,
                ProjectTotals = projectTotals,
                TotalLines = totalLines,
                TotalFiles = totalFiles,
                DuplicationMap = duplicationMap,
                DependencyList = dependencyList,
                ProcessingTime = timer.Elapsed,
                SolutionFileName = Path.GetFileName(solutionPath),
                DuplicatedLines = duplicationMap.Sum(x => x.NbLines)
            };
        }

        public static void OutputAnalysisResults(AnalysisResult result, bool verbose)
        {
            if (verbose)
            {
                OutputDetailedMetrics(result.Metrics, result.ProjectTotals);
            }

            var percentageDuplication = (result.DuplicatedLines / (double)result.TotalLines) * 100;
            NumberFormatInfo nfi = new System.Globalization.CultureInfo( "en-US", false ).NumberFormat;

            Console.WriteLine($"Processing completed, number of source files processed: {result.TotalFiles}");
            Console.WriteLine($"Total lines of code: {result.TotalLines}");
            Console.WriteLine($"Solution {result.SolutionFileName} has {result.DuplicatedLines} duplicated lines of code.");
            Console.WriteLine($"Percentage of duplicated code: {percentageDuplication.ToString("F2", nfi)} %");
            Console.WriteLine($"Time taken: {result.ProcessingTime:m\\:ss\\.fff}");
        }

        public static void ExportResults(AnalysisResult result, string solutionPath, CoreUtils.ExportFormat format)
        {
            var metricsOutputFilePath = CoreUtils.GetExportFileNameWithExtension(
                $"{result.SolutionFileName}-CodeMetrics.xxx", format);
            var duplicationOutputFilePath = CoreUtils.GetExportFileNameWithExtension(
                $"{result.SolutionFileName}-CodeDuplications.xxx", format);
                var dependenciesOutputFilePath = CoreUtils.GetExportFileNameWithExtension(
                $"{result.SolutionFileName}-CodeDependencies.xxx", format);

            try
            {
                Parallel.Invoke(
                    () => DataExporter.ExportMetrics(
                        metricsOutputFilePath,
                        result.Metrics,
                        result.ProjectTotals,
                        result.TotalLines,
                        result.DuplicationMap,
                        solutionPath,
                        format),
                    () => DataExporter.ExportDuplications(
                        duplicationOutputFilePath,
                        result.DuplicationMap,
                        format),
                    () => DataExporter.ExportDependencies(
                        dependenciesOutputFilePath,
                        result.DependencyList,
                        format)
                );

                Console.WriteLine($"The data has been exported to {metricsOutputFilePath}");
                Console.WriteLine($"The code duplications have been exported to {duplicationOutputFilePath}");
            }
            catch (AggregateException ae)
            {
                Console.Error.WriteLine($"Error during parallel export operations: {ae.InnerException?.Message}");
                throw;
            }
            catch (IOException ioe)
            {
                Console.Error.WriteLine($"IO error during file operations: {ioe.Message}");
                throw;
            }
        }

        public static void OutputDetailedMetrics(List<NamespaceMetrics> metrics, Dictionary<string, int> projectTotals)
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

    }
}