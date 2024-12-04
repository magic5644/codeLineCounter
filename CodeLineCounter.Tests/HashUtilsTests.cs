using CodeLineCounter.Utils;

namespace CodeLineCounter.Tests
{
    public class HashUtilsTests
    {
        [Fact]
        public void ComputeHash_EmptyString_ReturnsEmptyString()
        {
            // Arrange
            string input = "";

            // Act
            string result = HashUtils.ComputeHash(input);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void ComputeHash_NullString_ReturnsEmptyString()
        {
            // Arrange
            string? input = null;

            // Act
            string result = HashUtils.ComputeHash(input);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void ComputeHash_ValidString_ReturnsHash()
        {
            // Arrange
            string input = "Hello, World!";

            // Act
            string result = HashUtils.ComputeHash(input);

            // Assert
            Assert.NotEmpty(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void ComputeHash_DuplicateStrings_ReturnSameHash()
        {
            // Arrange
            string input1 = "Hello, World!";
            string input2 = "Hello, World!";

            // Act
            string result1 = HashUtils.ComputeHash(input1);
            string result2 = HashUtils.ComputeHash(input2);

            // Assert
            Assert.Equal(result1, result2);
        }
    }
}