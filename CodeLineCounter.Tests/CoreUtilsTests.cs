using CodeLineCounter.Utils;

namespace CodeLineCounter.Tests
{
    public class CoreUtilsTests
    {
        [Fact]
        public void ParseArguments_Should_Return_Correct_Values()
        {
            // Arrange
            string[] args = ["-verbose", "-d", "testDirectory"];

            // Act
            var (Verbose, DirectoryPath, _) = CoreUtils.ParseArguments(args);

            // Assert
            Assert.True(Verbose);
            Assert.Equal("testDirectory", DirectoryPath);
        }

        [Fact]
        public void ParseArguments_help_Should_Return_Correct_Values()
        {
            // Arrange
            string[] args = ["-help"];

            // Act
            var (_, _, Help) = CoreUtils.ParseArguments(args);

            // Assert
            Assert.True(Help);
        }

        [Fact]
        public void ParseArguments_Should_Return_Default_Values_When_No_Arguments_Passed()
        {
            // Arrange
            string[] args = [];

            // Act
            var (Verbose, DirectoryPath, _) = CoreUtils.ParseArguments(args);

            // Assert
            Assert.False(Verbose);
            Assert.Null(DirectoryPath);
        }

        [Fact]
        public void ParseArguments_Should_Ignore_Invalid_Arguments()
        {
            // Arrange
            string[] args = ["-invalid", "-d", "testDirectory"];

            // Act
            var (Verbose, DirectoryPath, _) = CoreUtils.ParseArguments(args);

            // Assert
            Assert.False(Verbose);
            Assert.Equal("testDirectory", DirectoryPath);
        }

        [Fact]
        public void GetUserChoice_Should_Return_Valid_Choice()
        {
            // Arrange
            int solutionCount = 5;
            string input = "3";
            var inputStream = new StringReader(input);
            Console.SetIn(inputStream);

            // Act
            int result = CoreUtils.GetUserChoice(solutionCount);

            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public void GetUserChoice_Should_Return_Invalid_Choice()
        {
            // Arrange
            int solutionCount = 5;
            string input = "invalid";
            var inputStream = new StringReader(input);
            Console.SetIn(inputStream);

            // Act
            int result = CoreUtils.GetUserChoice(solutionCount);

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void DisplaySolutions_Should_Write_Solutions_To_Console()
        {

            var envNewLine = Environment.NewLine;
            // Arrange
            List<string> solutionFiles =
            [
                "Solution1.sln",
                "Solution2.sln",
                "Solution3.sln"
            ];

            // Redirect console output to a StringWriter
            using StringWriter sw = new();
            Console.SetOut(sw);

            // Act
            CoreUtils.DisplaySolutions(solutionFiles);

            // Assert
            string expectedOutput = $"Available solutions:{envNewLine}";
            for (int i = 0; i < solutionFiles.Count; i++)
            {
                expectedOutput += $"{i + 1}. {solutionFiles[i]}{envNewLine}";
            }
            Assert.Equal(expectedOutput, sw.ToString());
        }

        [Fact]
        public void GetFilenamesList_Should_Return_List_Of_Filenames()
        {
            // Arrange
            List<string> solutionFiles =
            [
                "Solution1.sln",
                "Solution2.sln",
                "Solution3.sln"
            ];

            // Act
            List<string> result = CoreUtils.GetFilenamesList(solutionFiles);

            // Assert
            List<string> expectedFilenames =
            [
                "Solution1.sln",
                "Solution2.sln",
                "Solution3.sln"
            ];
            Assert.Equal(expectedFilenames, result);
        }


        [Fact]
        public void CheckSettings_WhenHelpIsTrue_ReturnsFalse()
        {
            // Arrange
            (bool Verbose, string? DirectoryPath, bool Help) settings = (true, null, true);
            using var sw = new StringWriter();
            Console.SetOut(sw);

            // Act
            var result = CoreUtils.CheckSettings(settings);

            // Assert
            Assert.False(result);
            Assert.Contains("Usage:", sw.ToString());
        }

        [Fact]
        public void CheckSettings_WhenDirectoryPathIsNull_ReturnsFalse()
        {
            // Arrange
            (bool Verbose, string? DirectoryPath, bool Help) settings = (Verbose: false, DirectoryPath: null, Help: false);
            using var sw = new StringWriter();
            Console.SetOut(sw);

            // Act
            var result = CoreUtils.CheckSettings(settings);

            // Assert
            Assert.False(result);
            Assert.Contains("Please provide the directory path", sw.ToString());
        }

        [Fact]
        public void CheckSettings_WhenSettingsAreValid_ReturnsTrue()
        {
            // Arrange
            (bool Verbose, string DirectoryPath, bool Help) settings = (false, "some_directory", false);

            // Act
            var result = CoreUtils.CheckSettings(settings);

            // Assert
            Assert.True(result);
        }
    }
}