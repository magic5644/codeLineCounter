using CodeLineCounter.Utils;

namespace CodeLineCounter.Tests
{
    public class FileUtilsTests : TestBase
    {
        private readonly string _testDirectory;

        public FileUtilsTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "FileUtilsTests");
            Directory.CreateDirectory(_testDirectory);
        }
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
            // Reset console output
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
            var nonExistentPath = Path.Combine(_testDirectory, Guid.NewGuid().ToString());

            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(() => FileUtils.GetSolutionFiles(nonExistentPath));
        }

        // Throws UnauthorizedAccessException when solution file does not exist
        [Fact]
        public void get_project_files_throws_when_file_not_exists()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.sln");

            // Act & Assert
            var exception = Assert.Throws<FileNotFoundException>(() =>
                FileUtils.GetProjectFiles(nonExistentPath));

            Assert.Contains(nonExistentPath, exception.Message);
        }

        protected override void Dispose(bool disposing)
        {
            // Ensure the file is closed before attempting to delete it
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
