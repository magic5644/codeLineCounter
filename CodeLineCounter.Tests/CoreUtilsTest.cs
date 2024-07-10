using Xunit;

namespace CodeLineCounter.Utils.Tests
{
    public class CoreUtilsTests
    {
        [Fact]
        public void ParseArguments_Should_Return_Correct_Values()
        {
            // Arrange
            string[] args = new string[] { "-verbose", "-d", "testDirectory" };

            // Act
            var result = CoreUtils.ParseArguments(args);

            // Assert
            Assert.True(result.Verbose);
            Assert.Equal("testDirectory", result.DirectoryPath);
        }

        [Fact]
        public void ParseArguments_Should_Return_Default_Values_When_No_Arguments_Passed()
        {
            // Arrange
            string[] args = new string[0];

            // Act
            var result = CoreUtils.ParseArguments(args);

            // Assert
            Assert.False(result.Verbose);
            Assert.Null(result.DirectoryPath);
        }

        [Fact]
        public void ParseArguments_Should_Ignore_Invalid_Arguments()
        {
            // Arrange
            string[] args = new string[] { "-invalid", "-d", "testDirectory" };

            // Act
            var result = CoreUtils.ParseArguments(args);

            // Assert
            Assert.False(result.Verbose);
            Assert.Equal("testDirectory", result.DirectoryPath);
        }
    }
}