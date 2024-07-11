using CodeLineCounter.Models;
using CodeLineCounter.Utils;
using Xunit;

namespace CodeLineCounter.Tests
{
    public class CsvExporterTests
    {
        [Fact]
        public void GetFileDuplicationsCount_Should_Return_Correct_Count()
        {

            // Arrange
            var file1 = "file1.cs";
            var file2 = "file2.cs";
            var file3 = "file3.cs";
            var solutionPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            File.WriteAllText(file1, "");
            File.WriteAllText(file2, "");
            File.WriteAllText(file3, "");



            // Arrange
            var duplicationCounts = new Dictionary<string, int>
            {
                { Path.GetFullPath(file1), 2 },
                { Path.GetFullPath(file2), 3 },
                { Path.GetFullPath(file3), 1 }
            };

            var metric = new NamespaceMetrics
            {
                FilePath = Path.GetFullPath("file2.cs")
            };

            // Act
            int result = CsvExporter.GetFileDuplicationsCount(duplicationCounts, metric, solutionPath);

            // Assert
            Assert.Equal(3, result);

            // Clean up
            File.Delete(file1);
            File.Delete(file2);
            File.Delete(file3);
        }
    }
}