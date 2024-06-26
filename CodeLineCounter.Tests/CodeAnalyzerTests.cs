using CodeLineCounter.Services;
using System.IO;
using Xunit;

namespace CodeLineCounter.Tests
{
    public class CodeAnalyzerTests
    {
        [Fact]
        public void TestAnalyzeSolution()
        {
            // Arrange
            var solutionPath = @"..\..\..\..\CodeLineCounter.sln";
            var analyzer = new CodeAnalyzer();

            // Act
            var (metrics, projectTotals, totalLines) = analyzer.AnalyzeSolution(solutionPath);

            // Assert
            Assert.NotNull(metrics);
            Assert.NotEmpty(metrics);
            Assert.NotEqual(0, totalLines);
        }
    }
}
