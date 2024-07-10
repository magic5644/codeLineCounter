using Xunit;

namespace CodeLineCounter.Utils.Tests
{
    public class FileUtilsTests
    {
        [Fact]
        public void GetSolutionFiles_Should_Return_List_Of_Solution_Files()
        {
            // Arrange
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            var solutionPath = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", ".."));
            var rootPath = solutionPath;

            // Act
            var result = FileUtils.GetSolutionFiles(rootPath);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, file => Assert.EndsWith(".sln", file));
        }
    }
}