using Xunit;
using System.IO;
using System.Collections.Generic;
using CodeLineCounter.Models;
using CodeLineCounter.Utils;

namespace CodeLineCounter.Tests
{
    public class CsvExporterTests
    {
        [Fact]
        public void ExportToCsv_ValidData_WritesToFile()
        {
            // Arrange
            string filePath = "test1.csv";
            var namespaceMetrics = GetSampleNamespaceMetrics();
            var projectTotals = GetSampleProjectTotals();
            int totalLines = 500;
            var duplicationCodes = GetSampleDuplicationCodes();
            string? additionalInfo = "Some additional info";

            // Act
            CsvExporter.ExportToCsv(filePath, namespaceMetrics, projectTotals, totalLines, duplicationCodes, additionalInfo);

            // Assert
            Assert.True(File.Exists(filePath));
            var lines = File.ReadAllLines(filePath);
            Assert.Equal(6, lines.Length); // Header + 2 records + 2 sub-totals + 1 total

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public void ExportToCsv_EmptyData_WritesEmptyFile()
        {
            // Arrange
            string filePath = "test2.csv";
            var namespaceMetrics = new List<NamespaceMetrics>();
            var projectTotals = new Dictionary<string, int>();
            int totalLines = 0;
            var duplicationCodes = new List<DuplicationCode>();
            string? additionalInfo = null;

            // Act
            CsvExporter.ExportToCsv(filePath, namespaceMetrics, projectTotals, totalLines, duplicationCodes, additionalInfo);

            // Assert
            Assert.True(File.Exists(filePath));
            var lines = File.ReadAllLines(filePath);
            Assert.Single(lines); // Only header

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public void ExportToCsv_NullData_ThrowsNullReferenceException()
        {
            // Arrange
            string filePath = "test3.csv";
            int totalLines = 0;

            // Act & Assert
            #pragma warning disable CS8625 
            Assert.Throws<NullReferenceException>(() => CsvExporter.ExportToCsv(filePath, null, null, totalLines, null, null));
        }

        [Fact]
        public void ExportCodeDuplicationsToCsv_ShouldCallSerializeWithCorrectParameters()
        {
            // Arrange
            var filePath = "test4.csv";
            var duplications = new List<DuplicationCode>
            {
                new() { CodeHash = "hash1", FilePath = "file1.cs", MethodName = "method1", StartLine = 10, NbLines = 20 },
                new() { CodeHash = "hash2", FilePath = "file2.cs", MethodName = "method2", StartLine = 8, NbLines = 10 }
            };


            // Act
            CsvExporter.ExportCodeDuplicationsToCsv(filePath, duplications);

            // Assert
            Assert.True(File.Exists(filePath));
            var lines = File.ReadAllLines(filePath);
            Assert.Equal(3, lines.Length); 

            // Cleanup
            File.Delete(filePath);

        }

        private static List<NamespaceMetrics> GetSampleNamespaceMetrics()
        {
            return
            [
                new NamespaceMetrics { ProjectName="Project1", ProjectPath = ".", NamespaceName = "Namespace1", FileName = "File1", FilePath = ".", LineCount = 100, CyclomaticComplexity = 0, CodeDuplications = 0 },
                new NamespaceMetrics { ProjectName="Project2", ProjectPath = ".", NamespaceName = "Namespace2", FileName = "File2", FilePath = ".", LineCount = 200, CyclomaticComplexity = 5, CodeDuplications = 2 }
            ];
        }

        private static Dictionary<string, int> GetSampleProjectTotals()
        {
            return new Dictionary<string, int>
            {
                {"Project1", 100},
                {"Project2", 200}
            };
        }

        private static List<DuplicationCode> GetSampleDuplicationCodes()
        {
            return
            [
                new DuplicationCode { CodeHash = "Code1", FilePath = ".", MethodName = "Method1", StartLine = 10 , NbLines = 20 },
                new DuplicationCode { CodeHash = "Code2", FilePath = ".", MethodName = "Method2", StartLine = 15 , NbLines = 25  }
            ];
        }
    }
}