using CodeLineCounter.Models;
using CodeLineCounter.Utils;
using System.Collections.Generic;
using System.Text;
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
            var solutionPath = FileUtils.GetBasePath();

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

        [Fact]
        public void AppendProjectLineToCsv_AppendsCorrectLine_WhenProjectNameIsNotNull()
        {
            // Arrange
            var projectTotals = new Dictionary<string, int> { { "Project1", 100 } };
            var csvBuilder = new StringBuilder();
            var currentProject = "Project1";

            // Act
            CsvExporter.AppendProjectLineToCsv(projectTotals, csvBuilder, currentProject);

            // Assert
            var expectedLine = $"{currentProject},Total,,,,{projectTotals[currentProject]},,{Environment.NewLine}";
            Assert.Equal(expectedLine, csvBuilder.ToString());
        }

        [Fact]
        public void AppendProjectLineToCsv_DoesNotAppendLine_WhenProjectNameIsNull()
        {
            // Arrange
            var projectTotals = new Dictionary<string, int> { { "Project1", 100 } };
            var csvBuilder = new StringBuilder();
            string? currentProject = null;

            // Act
            CsvExporter.AppendProjectLineToCsv(projectTotals, csvBuilder, currentProject);

            // Assert
            Assert.Empty(csvBuilder.ToString());
        }
    }
}