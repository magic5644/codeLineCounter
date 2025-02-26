using CodeLineCounter.Utils;
using System.Text.Json;

namespace CodeLineCounter.Tests
{
    public class JsonHandlerTests : TestBase
    {

        private readonly string _testDirectory;

        public JsonHandlerTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "JsonHandlerTests");
            Directory.CreateDirectory(_testDirectory);
        }

        public class TestClass
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public TestClass(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }
        // Deserialize valid JSON file containing array of objects into IEnumerable<T>
        [Fact]
        public void deserialize_valid_json_file_returns_expected_objects()
        {
            // Arrange
            var testFilePath = Path.Combine(_testDirectory, "test.json");
            var expectedData = new[] { new TestClass(id: 1, name: "Test") };
            File.WriteAllText(testFilePath, JsonSerializer.Serialize(expectedData));

            // Act
            var result = JsonHandler.Deserialize<TestClass>(testFilePath);

            // Assert
            Assert.Equal(expectedData.Length, result.Count());
            Assert.Equal(expectedData[0].Id, result.First().Id);
            Assert.Equal(expectedData[0].Name, result.First().Name);

            File.Delete(testFilePath);
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