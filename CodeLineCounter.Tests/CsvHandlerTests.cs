using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using System.Linq;
using Xunit;

namespace CodeLineCounter.Utils.Tests
{
    public class CsvHandlerTests
    {
        private class TestRecord
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        [Fact]
        public void Serialize_ValidData_WritesToFile()
        {
            // Arrange
            var data = new List<TestRecord>
            {
                new TestRecord { Id = 1, Name = "Alice" },
                new TestRecord { Id = 2, Name = "Bob" }
            };
            string filePath = "test.csv";

            // Act
            CsvHandler.Serialize(data, filePath);

            // Assert
            var lines = File.ReadAllLines(filePath);
            Assert.Equal(3, lines.Length); // Header + 2 records
            Assert.Contains("Alice", lines[1]);
            Assert.Contains("Bob", lines[2]);

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public void Deserialize_ValidFile_ReturnsData()
        {
            // Arrange
            string filePath = "test.csv";
            var data = new List<string>
            {
                "Id,Name",
                "1,Alice",
                "2,Bob"
            };
            File.WriteAllLines(filePath, data);

            // Act
            var result = CsvHandler.Deserialize<TestRecord>(filePath).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Alice", result[0].Name);
            Assert.Equal("Bob", result[1].Name);

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public void Serialize_EmptyData_WritesEmptyFile()
        {
            // Arrange
            var data = new List<TestRecord>();
            string filePath = "test.csv";

            // Act
            CsvHandler.Serialize(data, filePath);

            // Assert
            var lines = File.ReadAllLines(filePath);
            Assert.Single(lines); // Only header

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public void Deserialize_EmptyFile_ReturnsEmptyList()
        {
            // Arrange
            string filePath = "test.csv";
            File.WriteAllText(filePath, "Id,Name");

            // Act
            var result = CsvHandler.Deserialize<TestRecord>(filePath).ToList();

            // Assert
            Assert.Empty(result);

            // Cleanup
            File.Delete(filePath);
        }
    }
}