using CodeLineCounter.Utils;

namespace CodeLineCounter.Tests
{
    public class FileUtilsTests
    {
        [Fact]
        public void GetSolutionFiles_Should_Return_List_Of_Solution_Files()
        {
            // Arrange
            var basePath = FileUtils.GetBasePath();
            var solutionPath = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", ".."));
            var rootPath = solutionPath;

            // Act
            var result = FileUtils.GetSolutionFiles(rootPath);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, file => Assert.EndsWith(".sln", file));
        }

        [Fact]
        public void GetBasePath_Should_Return_NonEmptyString()
        {
            // Act
            string basePath = FileUtils.GetBasePath();

            // Assert
            Assert.False(string.IsNullOrEmpty(basePath), "Base path should not be null or empty.");
            Assert.True(Directory.Exists(basePath), "Base path should be a valid directory.");
        }

        [Fact]
        public void get_solution_files_throws_exception_for_nonexistent_directory()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() => FileUtils.GetSolutionFiles(nonExistentPath));
        }

        // Throws UnauthorizedAccessException when solution file does not exist
        [Fact]
        public void get_project_files_throws_when_file_not_exists()
        {
            // Arrange
            var nonExistentPath = "nonexistent.sln";

            // Act & Assert
            var exception = Assert.Throws<UnauthorizedAccessException>(() =>
                FileUtils.GetProjectFiles(nonExistentPath));

            Assert.Contains(nonExistentPath, exception.Message);
        }
    }
}