using CodeLineCounter.Services;
using CodeLineCounter.Utils;

namespace CodeLineCounter.Tests
{
    public class CodeAnalyzerTests
    {
        [Fact]
        public void TestAnalyzeSolution()
        {
            string basePath = FileUtils.GetBasePath();
            var solutionPath = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "..", "CodeLineCounter.sln"));

            // Act
            var (metrics, projectTotals, totalLines, totalFiles, duplicationMap) = CodeAnalyzer.AnalyzeSolution(solutionPath);

            // Assert
            Assert.NotNull(metrics);
            Assert.NotEmpty(metrics);
            Assert.NotEmpty(projectTotals);
            Assert.NotEqual(0, totalLines);
            Assert.NotEqual(0, totalFiles);
            Assert.NotNull(duplicationMap);
        }

        [Fact]
        public void AnalyzeSourceCode_Should_Set_CurrentNamespace()
        {
            // Arrange
            var projectNamespaceMetrics = new Dictionary<string, int>();
            var basePath = FileUtils.GetBasePath();
            var file = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..",  "CodeAnalyzerTests.cs"));
            var lines = new string[]
            {
                "namespace MyNamespace",
                "{",
                "    // Code goes here",
                "}"
            };

            // Act
            CodeAnalyzer.AnalyzeSourceCode(projectNamespaceMetrics, file, lines, out string? currentNamespace, out _, out _);

            // Assert
            Assert.Equal("MyNamespace", currentNamespace);
        }

        [Fact]
        public void AnalyzeSourceCode_Should_Set_FileLineCount()
        {
            // Arrange
            var projectNamespaceMetrics = new Dictionary<string, int>();
            var basePath = FileUtils.GetBasePath();
            var file = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..",  "CodeAnalyzerTests.cs"));
            var lines = new string[]
            {
                "namespace MyNamespace",
                "{",
                "    // Code goes here",
                "}"
            };

            // Act
            CodeAnalyzer.AnalyzeSourceCode(projectNamespaceMetrics, file, lines, out _, out int fileLineCount, out _);

            // Assert - 3 lines only because comment lines are ignored
            Assert.Equal(3, fileLineCount);
        }

        [Fact]
        public void AnalyzeSourceCode_Should_Set_FileCyclomaticComplexity()
        {
            // Arrange
            var projectNamespaceMetrics = new Dictionary<string, int>();
            var basePath = FileUtils.GetBasePath();
            var file = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..",  "CodeAnalyzerTests.cs"));
            var lines = new string[]
            {
                "namespace MyNamespace",
                "{",
                "    // Code goes here",
                "}"
            };

            // Act
            CodeAnalyzer.AnalyzeSourceCode(projectNamespaceMetrics, file, lines, out _, out _, out int fileCyclomaticComplexity);

            // Assert
            Assert.Equal(1, fileCyclomaticComplexity);
        }
    }
}
