using CodeLineCounter.Models;
using CodeLineCounter.Utils;
using System.Text;

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
            var memoryStream = new MemoryStream();
            
            var writer = new StreamWriter(memoryStream);
            var currentProject = "Project1";

            // Act
            CsvExporter.AppendProjectLineToCsv(projectTotals, writer, currentProject);
            writer.Flush();
            memoryStream.Position = 0;
            var result = new StreamReader(memoryStream).ReadToEnd();

            // Assert
            var expectedLine = $"{currentProject},Total,,,,{projectTotals[currentProject]},,{Environment.NewLine}";
            Assert.Contains(expectedLine, result.ToString());
        }

        [Fact]
        public void AppendProjectLineToCsv_DoesNotAppendLine_WhenProjectNameIsNull()
        {
            // Arrange
            var projectTotals = new Dictionary<string, int> { { "Project1", 100 } };
            var memoryStream = new MemoryStream();
            
            var writer = new StreamWriter(memoryStream);
            string? currentProject = null;

            // Act
            CsvExporter.AppendProjectLineToCsv(projectTotals, writer, currentProject);
            writer.Flush();
            memoryStream.Position = 0;
            var result = new StreamReader(memoryStream).ReadToEnd();

            // Assert
            Assert.Empty(result.ToString());
        }

        [Fact]
        public void GetDuplicationCounts_Should_Return_Correct_Counts()
        {
            // Arrange
            var duplications = new List<DuplicationCode>
            {
                new DuplicationCode { FilePath = "file1.cs" },
                new DuplicationCode { FilePath = "file1.cs" },
                new DuplicationCode { FilePath = "file2.cs" }
            };

            // Act
            var result = CsvExporter.GetDuplicationCounts(duplications);

            // Assert
            Assert.Equal(2, result[Path.GetFullPath("file1.cs")]);
            Assert.Equal(1, result[Path.GetFullPath("file2.cs")]);
        }

        [Fact]
        public void GetDuplicationCounts_Should_Return_Empty_Dictionary_When_No_Duplications()
        {
            // Arrange
            var duplications = new List<DuplicationCode>();

            // Act
            var result = CsvExporter.GetDuplicationCounts(duplications);

            // Assert
            Assert.Empty(result);
        }
    }
}


