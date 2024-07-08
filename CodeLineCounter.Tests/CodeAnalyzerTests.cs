using CodeLineCounter.Services;

namespace CodeLineCounter.Tests
{
    public class CodeAnalyzerTests
    {
        [Fact]
        public void TestAnalyzeSolution()
        {
            // Arrange
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            var solutionPath = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "..", "CodeLineCounter.sln"));
            var analyzer = new CodeAnalyzer();

            // Act
            var (metrics, projectTotals, totalLines, totalFiles, duplicationMap) = analyzer.AnalyzeSolution(solutionPath);

            // Assert
            Assert.NotNull(metrics);
            Assert.NotEmpty(metrics);
            Assert.NotEmpty(projectTotals);
            Assert.NotEqual(0, totalLines);
            Assert.NotEqual(0, totalFiles);
        }

        [Fact]
        public void AnalyzeSourceCode_Should_Set_CurrentNamespace()
        {
            // Arrange
            var projectNamespaceMetrics = new Dictionary<string, int>();
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            var file = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..",  "CodeAnalyzerTests.cs"));
            var lines = new string[]
            {
                "namespace MyNamespace",
                "{",
                "    // Code goes here",
                "}"
            };

            // Act
            CodeAnalyzer.AnalyzeSourceCode(projectNamespaceMetrics, file, lines, out string? currentNamespace, out int fileLineCount, out int fileCyclomaticComplexity);

            // Assert
            Assert.Equal("MyNamespace", currentNamespace);
        }

        [Fact]
        public void AnalyzeSourceCode_Should_Set_FileLineCount()
        {
            // Arrange
            var projectNamespaceMetrics = new Dictionary<string, int>();
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            var file = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..",  "CodeAnalyzerTests.cs"));
            var lines = new string[]
            {
                "namespace MyNamespace",
                "{",
                "    // Code goes here",
                "}"
            };

            // Act
            CodeAnalyzer.AnalyzeSourceCode(projectNamespaceMetrics, file, lines, out string? currentNamespace, out int fileLineCount, out int fileCyclomaticComplexity);

            // Assert - 3 lines only because comment lines are ignored
            Assert.Equal(3, fileLineCount);
        }

        [Fact]
        public void AnalyzeSourceCode_Should_Set_FileCyclomaticComplexity()
        {
            // Arrange
            var projectNamespaceMetrics = new Dictionary<string, int>();
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            var file = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..",  "CodeAnalyzerTests.cs"));
            var lines = new string[]
            {
                "namespace MyNamespace",
                "{",
                "    // Code goes here",
                "}"
            };

            // Act
            CodeAnalyzer.AnalyzeSourceCode(projectNamespaceMetrics, file, lines, out string? currentNamespace, out int fileLineCount, out int fileCyclomaticComplexity);

            // Assert
            Assert.Equal(1, fileCyclomaticComplexity);
        }
    }
}
