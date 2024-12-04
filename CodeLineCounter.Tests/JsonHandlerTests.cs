using CodeLineCounter.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Sdk;

namespace CodeLineCounter.Tests
{
    public class JsonHandlerTests
    {
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
            var testFilePath = "test.json";
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

    }
}