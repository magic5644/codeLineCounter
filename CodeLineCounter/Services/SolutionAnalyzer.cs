using CodeLineCounter.Utils;
using CodeLineCounter.Models;
using System.Diagnostics;
using System.Globalization;

namespace CodeLineCounter.Services
{
    public static partial class SolutionAnalyzer
    {

        public static void AnalyzeAndExportSolution(string solutionPath, bool verbose, CoreUtils.ExportFormat format, string? outputPath = null)
        {
            try
            {
                var analysisResult = PerformAnalysis(solutionPath);
                OutputAnalysisResults(analysisResult, verbose);
                ExportResults(analysisResult, solutionPath, format, outputPath);

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

        public static void ExportResults(AnalysisResult result, string solutionPath, CoreUtils.ExportFormat format, string? outputPath = null)
        {
            string baseFileName = Path.GetFileNameWithoutExtension(solutionPath);
            
            // Export metrics
            string metricsFileName = $"{baseFileName}.CodeMetrics.json";
            metricsFileName = CoreUtils.GetExportFileNameWithExtension(metricsFileName, format);
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = ".";
            }   

            string metricsOutputPath = outputPath != null 
                ? Path.Combine(outputPath, metricsFileName)
                : metricsFileName;

            // Export duplications
            string duplicationsFileName = $"{baseFileName}.CodeDuplications.json";
            duplicationsFileName = CoreUtils.GetExportFileNameWithExtension(duplicationsFileName, format);
            string duplicationsOutputPath = outputPath != null 
                ? Path.Combine(outputPath, duplicationsFileName)
                : duplicationsFileName;
            // Export des duplications...

            // Export dependencies graph
            string graphFileName = $"{baseFileName}.Dependencies.dot";
            string graphOutputPath = outputPath != null 
                ? Path.Combine(outputPath, graphFileName)
                : graphFileName;

            try
            {
                Parallel.Invoke(
                    () => DataExporter.ExportMetrics(
                        metricsFileName,
                        outputPath ??".",
                        result,
                        solutionPath,
                        format),
                    () => DataExporter.ExportDuplications(
                        duplicationsFileName,
                        outputPath ?? ".",
                        result.DuplicationMap,
                        format),
                    async () => await DataExporter.ExportDependencies(
                        graphFileName,
                        outputPath ?? ".",
                        result.DependencyList,
                        format)
                );

                Console.WriteLine($"The data has been exported to {metricsOutputPath}");
                Console.WriteLine($"The code duplications have been exported to {duplicationsOutputPath}");
                Console.WriteLine($"The code dependencies have been exported to {graphOutputPath} and the graph has been generated. (dot file can be found in the same directory)");
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