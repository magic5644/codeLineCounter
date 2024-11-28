using Xunit;
using System;
using System.IO;
using System.Collections.Generic;
using CodeLineCounter.Models;
using CodeLineCounter.Utils;
using System.Linq;

namespace CodeLineCounter.Tests
{
    public class DataExporterTests : IDisposable
    {
        private readonly string _testDirectory;
        private bool _disposed;

        public DataExporterTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "DataExporterTests");
            Directory.CreateDirectory(_testDirectory);
        }

        [Theory]
        [InlineData(CoreUtils.ExportFormat.CSV, ".csv")]
        [InlineData(CoreUtils.ExportFormat.JSON, ".json")]
        public void Export_SingleItem_CreatesFileWithCorrectExtension(CoreUtils.ExportFormat format, string expectedExtension)
        {
            // Arrange
            var testItem = new TestClass { Id = 1, Name = "Test" };
            var filePath = Path.Combine(_testDirectory, "test");

            // Act
            DataExporter.Export(filePath, testItem, format);

            // Assert
            Assert.True(File.Exists(filePath + expectedExtension));
        }

        [Theory]
        [InlineData(CoreUtils.ExportFormat.CSV, ".csv")]
        [InlineData(CoreUtils.ExportFormat.JSON, ".json")]
        public void ExportCollection_WithMultipleItems_CreatesFile(CoreUtils.ExportFormat format, string expectedExtension)
        {
            // Arrange
            var items = new List<TestClass>
            {
                new() { Id = 1, Name = "Test1" },
                new() { Id = 2, Name = "Test2" }
            };
            var filePath = Path.Combine(_testDirectory, "collection");

            // Act
            DataExporter.ExportCollection(filePath, items, format);

            // Assert
            Assert.True(File.Exists(filePath + expectedExtension));
        }

        [Fact]
        public void ExportMetrics_CreatesFileWithCorrectData()
        {
            // Arrange
            var metrics = new List<NamespaceMetrics>
            {
                new() { ProjectName = "Project1", LineCount = 100 },
                new() { ProjectName = "Project2", LineCount = 200 }
            };
            var projectTotals = new Dictionary<string, int>
            {
                { "Project1", 100 },
                { "Project2", 200 }
            };
            var duplications = new List<DuplicationCode>();
            var filePath = Path.Combine(_testDirectory, "metrics");

            // Act
            DataExporter.ExportMetrics(filePath, metrics, projectTotals, 300, duplications, null, CoreUtils.ExportFormat.CSV);

            // Assert
            Assert.True(File.Exists(filePath + ".csv"));
        }

        [Fact]
        public void ExportDuplications_CreatesFileWithCorrectData()
        {
            // Arrange
            var duplications = new List<DuplicationCode>
            {
                new() { CodeHash = "hash1", FilePath = "file1.cs", MethodName = "method1", StartLine = 10, NbLines = 20 },
                new() { CodeHash = "hash2", FilePath = "file2.cs", MethodName = "method2", StartLine = 8, NbLines = 10 }
            };
            var filePath = Path.Combine(_testDirectory, "duplications");

            // Act
            DataExporter.ExportDuplications(filePath, duplications, CoreUtils.ExportFormat.CSV);

            // Assert
            Assert.True(File.Exists(filePath + ".csv"));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && Directory.Exists(_testDirectory))
                {
                    // Dispose managed resources
                    Directory.Delete(_testDirectory, true);
                }

                // Dispose unmanaged resources (if any)

                _disposed = true;
            }
        }

        private class TestClass
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DataExporterTests()
        {
            Dispose(false);
        }


    }
}