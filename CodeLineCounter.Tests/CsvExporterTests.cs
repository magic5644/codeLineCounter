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
            Assert.Equal(3, lines.Length); // Header + 2 records

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
            List<NamespaceMetrics> namespaceMetrics = null;//new List<NamespaceMetrics>();
            Dictionary<string, int> projectTotals = null;
            int totalLines = 0;
            List<DuplicationCode> duplicationCodes = null;
            string? additionalInfo = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => CsvExporter.ExportToCsv(filePath, namespaceMetrics, projectTotals, totalLines, duplicationCodes, additionalInfo));
        }

        private List<NamespaceMetrics> GetSampleNamespaceMetrics()
        {
            return new List<NamespaceMetrics>
            {
                new NamespaceMetrics { NamespaceName = "Namespace1", LineCount = 100 },
                new NamespaceMetrics { NamespaceName = "Namespace2", LineCount = 200 }
            };
        }

        private Dictionary<string, int> GetSampleProjectTotals()
        {
            return new Dictionary<string, int>
            {
                {"Project1", 100},
                {"Project2", 200}
            };
        }

        private List<DuplicationCode> GetSampleDuplicationCodes()
        {
            return new List<DuplicationCode>
            {
                new DuplicationCode { CodeHash = "Code1", FilePath = ".", MethodName = "Method1", StartLine = 10 , NbLines = 20 },
                new DuplicationCode { CodeHash = "Code2", FilePath = ".", MethodName = "Method2", StartLine = 15 , NbLines = 25  }
            };
        }
    }
}