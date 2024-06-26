using CodeLineCounter.Services;

namespace CodeLineCounter.Tests
{
    public class CodeAnalyzerTests
    {
        [Fact]
        public void TestAnalyzeSolution()
        {
            // Arrange
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var solutionPath = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "..", "CodeLineCounter.sln"));
            var analyzer = new CodeAnalyzer();

            // Act
            var (metrics, projectTotals, totalLines) = analyzer.AnalyzeSolution(solutionPath);

            // Assert
            Assert.NotNull(metrics);
            Assert.NotEmpty(metrics);
            Assert.Empty(projectTotals);
            Assert.NotEqual(0, totalLines);
        }
    }
}
