using CodeLineCounter.Utils;

namespace CodeLineCounter.Tests
{
    public class CsvHandlerTests : TestBase
    {

        private readonly string _testDirectory;

        private class TestRecord
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        public CsvHandlerTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "CsvHandlerTests");
            Directory.CreateDirectory(_testDirectory);
        }

        [Fact]
        public void Serialize_ValidData_WritesToFile()
        {

            // Arrange
            var data = new List<TestRecord>
            {
                new() { Id = 1, Name = "Alice" },
                new() { Id = 2, Name = "Bob" }
            };
            string filePath = Path.Combine(_testDirectory, "test_1.csv");

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
            string filePath = Path.Combine(_testDirectory, "test_2.csv");
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
            string filePath = Path.Combine(_testDirectory, "test_3.csv");

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
            string filePath = Path.Combine(_testDirectory, "test_4.csv");
            File.WriteAllText(filePath, "Id,Name");

            // Act
            var result = CsvHandler.Deserialize<TestRecord>(filePath).ToList();

            // Assert
            Assert.Empty(result);

            // Cleanup
            File.Delete(filePath);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Directory.Exists(_testDirectory))
            {
                // Dispose managed resources
                Directory.Delete(_testDirectory, true);
            }

            // Dispose unmanaged resources (if any)
            base.Dispose(disposing);
        }
    }
}