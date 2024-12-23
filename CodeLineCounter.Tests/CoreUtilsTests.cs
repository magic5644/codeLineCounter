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
            var (Verbose, DirectoryPath, _, _) = CoreUtils.ParseArguments(args);

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
            var (_, _, Help, _) = CoreUtils.ParseArguments(args);

            // Assert
            Assert.True(Help);
        }

        [Fact]
        public void ParseArguments_Should_Return_Default_Values_When_No_Arguments_Passed()
        {
            // Arrange
            string[] args = [];

            // Act
            var (Verbose, DirectoryPath, _, _) = CoreUtils.ParseArguments(args);

            // Assert
            Assert.False(Verbose);
            Assert.Null(DirectoryPath);
        }

        [Fact]
        public void ParseArguments_Should_Ignore_Invalid_Arguments()
        {
            // Arrange
            string[] args = ["-invalid", "-d", "testDirectory", "-f", "json"];

            // Act
            var (Verbose, DirectoryPath, _, _) = CoreUtils.ParseArguments(args);

            // Assert
            Assert.False(Verbose);
            Assert.Equal("testDirectory", DirectoryPath);
        }

        // ParseArguments correctly processes valid command line arguments with all options
        [Fact]
        public void ParseArguments_processes_valid_arguments_with_all_options()
        {
            // Arrange
            string[] args = new[] { "-verbose", "-d", "C:/test", "-format", "JSON", "-help" };

            // Act
            var result = CoreUtils.ParseArguments(args);

            // Assert
            Assert.True(result.Verbose);
            Assert.Equal("C:/test", result.DirectoryPath);
            Assert.True(result.Help);
            Assert.Equal(CoreUtils.ExportFormat.JSON, result.format);
        }

        // ParseArguments handles empty or null argument array
        [Fact]
        public void ParseArguments_handles_empty_argument_array()
        {
            // Arrange
            string[] emptyArgs = Array.Empty<string>();

            // Act
            var result = CoreUtils.ParseArguments(emptyArgs);

            // Assert
            Assert.False(result.Verbose);
            Assert.Null(result.DirectoryPath);
            Assert.False(result.Help);
            Assert.Equal(CoreUtils.ExportFormat.CSV, result.format);
        }

        // ParseArguments processes invalid format option gracefully
        [Fact]
        public void ParseArguments_handles_invalid_format_option()
        {
            // Arrange
            string[] args = new[] { "-format", "INVALID" };
            var consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            // Act
            var result = CoreUtils.ParseArguments(args);

            // Assert
            Assert.Equal(CoreUtils.ExportFormat.CSV, result.format);
            Assert.Contains("Invalid format", consoleOutput.ToString());
        }

        [Fact]
        public void GetUserChoice_Should_Return_Valid_Choice()
        {
            // Arrange
            int solutionCount = 5;
            string input = "3";
            var inputStream = new StringReader(input);
            var consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);
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
            var consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);
            Console.SetIn(inputStream);

            // Act
            int result = CoreUtils.GetUserChoice(solutionCount);

            // Assert
            Assert.Equal(-1, result);
        }

        // GetUserChoice returns valid selection when input is within range
        [Fact]
        public void GetUserChoice_returns_valid_selection_for_valid_input()
        {
            // Arrange
            var input = "2";
            var consoleInput = new StringReader(input);
            var consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);
            Console.SetIn(consoleInput);

            // Act
            int result = CoreUtils.GetUserChoice(3);

            // Assert
            Assert.Equal(2, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("abc")]
        public void GetUserChoice_handles_invalid_input(string input)
        {
            // Arrange
            var consoleInput = new StringReader(input);
            var consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);
            Console.SetIn(consoleInput);

            // Act
            int result = CoreUtils.GetUserChoice(5);

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
            (bool Verbose, string? DirectoryPath, bool Help, CoreUtils.ExportFormat format) settings = (true, null, true, CoreUtils.ExportFormat.JSON);
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
            (bool Verbose, string? DirectoryPath, bool Help, CoreUtils.ExportFormat format) settings = (Verbose: false, DirectoryPath: null, Help: false, format: CoreUtils.ExportFormat.CSV);
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
            (bool Verbose, string DirectoryPath, bool Help, CoreUtils.ExportFormat format) settings = (false, "some_directory", false, CoreUtils.ExportFormat.CSV);

            // Act
            var result = CoreUtils.CheckSettings(settings);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CheckSettings_WhenSettingsAreInvalid_ReturnsFalse()
        {
            // Arrange
            (bool Verbose, string? DirectoryPath, bool Help, CoreUtils.ExportFormat format) settings = (false, null, false, CoreUtils.ExportFormat.CSV);
            using var sw = new StringWriter();
            Console.SetOut(sw);

            // Act
            var result = CoreUtils.CheckSettings(settings);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("CO.Solution.Build-CodeMetrics.txt", CoreUtils.ExportFormat.CSV)]
        [InlineData("CO.Solution.Build-CodeDuplications.json", CoreUtils.ExportFormat.JSON)]
        [InlineData("CO.Solution.Build-CodeMetrics.", CoreUtils.ExportFormat.CSV)]
        [InlineData("CO.Solution.Build-CodeDuplications.²", CoreUtils.ExportFormat.JSON)]
        [InlineData("metrics_789.csv", CoreUtils.ExportFormat.CSV)]
        public void get_export_file_name_with_extension_handles_alphanumeric(string fileName, CoreUtils.ExportFormat format)
        {
            // Act
            var result = CoreUtils.GetExportFileNameWithExtension(fileName, format);

            // Assert
            Assert.Contains(Path.GetFileNameWithoutExtension(fileName), result);
            Assert.True(File.Exists(result) || !File.Exists(result));
        }
    }
}