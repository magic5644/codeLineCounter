using CodeLineCounter.Services;
using CodeLineCounter.Models;
using CodeLineCounter.Utils;

namespace CodeLineCounter.Tests.Services
{
    public class SolutionAnalyzerTest
    {

        [Fact]
        public void analyze_and_export_solution_succeeds_with_valid_inputs()
        {
            // Arrange
            var basePath = FileUtils.GetBasePath();
            var solutionPath = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", ".."));
            solutionPath = Path.Combine(solutionPath, "CodeLineCounter.sln");
            var sw = new StringWriter();
            Console.SetOut(sw);
            var verbose = false;
            var format = CoreUtils.ExportFormat.JSON;

            // Act & Assert
            var exception = Record.Exception(() =>
                SolutionAnalyzer.AnalyzeAndExportSolution(solutionPath, verbose, format));

            Assert.Null(exception);
        }

        [Fact]
        public void analyze_and_export_solution_throws_on_invalid_path()
        {
            // Arrange
            var invalidPath = "";
            var verbose = false;
            var format = CoreUtils.ExportFormat.JSON;

            // Act & Assert
            var exception = Assert.Throws<UnauthorizedAccessException>(() =>
                SolutionAnalyzer.AnalyzeAndExportSolution(invalidPath, verbose, format));

            Assert.Contains("Access to the path '' is denied.", exception.Message);
        }

        [Fact]
        public void PerformAnalysis_ShouldReturnCorrectAnalysisResult()
        {
            // Arrange
            var basePath = FileUtils.GetBasePath();
            var solutionPath = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", ".."));
            solutionPath = Path.Combine(solutionPath, "CodeLineCounter.sln");
            var sw = new StringWriter();
            Console.SetOut(sw);
            Console.WriteLine($"Constructed solution path: {solutionPath}");
            Assert.True(File.Exists(solutionPath), $"The solution file '{solutionPath}' does not exist.");
            Console.WriteLine($"Constructed solution path: {solutionPath}");
            Assert.True(File.Exists(solutionPath), $"The solution file '{solutionPath}' does not exist.");

            // Act
            var result = SolutionAnalyzer.PerformAnalysis(solutionPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CodeLineCounter.sln", result.SolutionFileName);
        }

        [Fact]
        public void OutputAnalysisResults_ShouldPrintCorrectOutput()
        {
            // Arrange
            var result = new AnalysisResult
            {
                Metrics = new List<NamespaceMetrics>(),
                ProjectTotals = new Dictionary<string, int>(),
                TotalLines = 1000,
                TotalFiles = 10,
                DuplicationMap = new List<DuplicationCode>(),
                DependencyList = new List<DependencyRelation>(),
                ProcessingTime = TimeSpan.FromSeconds(10),
                SolutionFileName = "CodeLineCounter.sln",
                DuplicatedLines = 100
            };
            var verbose = true;

            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                // Act
                SolutionAnalyzer.OutputAnalysisResults(result, verbose);

                // Assert
                var output = sw.ToString();
                Assert.Contains("Processing completed, number of source files processed: 10", output);
                Assert.Contains("Total lines of code: 1000", output);
                Assert.Contains("Solution CodeLineCounter.sln has 100 duplicated lines of code.", output);
                Assert.Contains("Percentage of duplicated code: 10.00 %", output);
                Assert.Contains("Time taken: 0:10.000", output);
            }
        }

        [Fact]
        public void OutputDetailedMetrics_ShouldPrintMetricsAndProjectTotals()
        {
            // Arrange
            var metrics = new List<NamespaceMetrics>
            {
                new NamespaceMetrics
                {
                    ProjectName = "Project1",
                    ProjectPath = "/path/to/project1",
                    NamespaceName = "Namespace1",
                    FileName = "File1.cs",
                    FilePath = "/path/to/project1/File1.cs",
                    LineCount = 100,
                    CyclomaticComplexity = 10
                },
                new NamespaceMetrics
                {
                    ProjectName = "Project2",
                    ProjectPath = "/path/to/project2",
                    NamespaceName = "Namespace2",
                    FileName = "File2.cs",
                    FilePath = "/path/to/project2/File2.cs",
                    LineCount = 200,
                    CyclomaticComplexity = 20
                }
            };

            var projectTotals = new Dictionary<string, int>
            {
                { "Project1", 100 },
                { "Project2", 200 }
            };

            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                // Act
                SolutionAnalyzer.OutputDetailedMetrics(metrics, projectTotals);

                // Assert
                var expectedOutput =
                    $"Project Project1 (/path/to/project1) - Namespace Namespace1 in file File1.cs (/path/to/project1/File1.cs) has 100 lines of code and a cyclomatic complexity of 10.{Environment.NewLine}" +
                    $"Project Project2 (/path/to/project2) - Namespace Namespace2 in file File2.cs (/path/to/project2/File2.cs) has 200 lines of code and a cyclomatic complexity of 20.{Environment.NewLine}" +
                    $"Project Project1 has 100 total lines of code.{Environment.NewLine}" +
                    $"Project Project2 has 200 total lines of code.{Environment.NewLine}";

                Assert.Equal(expectedOutput, sw.ToString());
            }
        }

        // Export metrics, duplications and dependencies data in parallel for valid input
        [Fact]
        public void export_results_with_valid_input_exports_all_files()
        {
            // Arrange
            var result = new AnalysisResult
            {
                SolutionFileName = "TestSolution",
                Metrics = new List<NamespaceMetrics>(),
                ProjectTotals = new Dictionary<string, int>(),
                TotalLines = 1000,
                DuplicationMap = new List<DuplicationCode>(),
                DependencyList = new List<DependencyRelation>()
            };

            var basePath = FileUtils.GetBasePath();
            var solutionPath = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", ".."));

            solutionPath = Path.Combine(solutionPath, "TestSolution.sln");
            var format = CoreUtils.ExportFormat.CSV;

            // Act
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                SolutionAnalyzer.ExportResults(result, solutionPath, format);
            }

            // Assert
            Assert.True(File.Exists("TestSolution-CodeMetrics.csv"));
            Assert.True(File.Exists("TestSolution-CodeDuplications.csv"));
            Assert.True(File.Exists("TestSolution-CodeDependencies.csv"));
        }

    }
}