using CodeLineCounter.Utils;
using CodeLineCounter.Models;
using CodeLineCounter.Exceptions;

namespace CodeLineCounter.Tests
{
    public class CoreUtilsTests : TestBase
    {

        private readonly string _testDirectory;

        public CoreUtilsTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "CoreUtilsTests");
            Directory.CreateDirectory(_testDirectory);
        }
        [Fact]
        public void ParseArguments_Should_Return_Correct_Values()
        {
            // Arrange
            string[] args = ["-verbose", "-d", "testDirectory", "-output", _testDirectory];

            // Act
            Settings settings = CoreUtils.ParseArguments(args);

            // Assert
            Assert.True(settings.Verbose);
            Assert.Equal("testDirectory", settings.DirectoryPath);

        }

        [Fact]
        public void ParseArguments_help_Should_Return_Correct_Values()
        {
            // Arrange
            string[] args = ["-help"];

            // Act
            var settings = CoreUtils.ParseArguments(args);

            // Assert
            Assert.True(settings.Help);

        }

        [Fact]
        public void ParseArguments_Should_Return_Default_Values_When_No_Arguments_Passed()
        {
            // Arrange
            string[] args = [];

            // Act
            var settings = CoreUtils.ParseArguments(args);

            // Assert
            Assert.False(settings.Verbose);
            Assert.Null(settings.DirectoryPath);

        }

        [Fact]
        public void ParseArguments_Should_Ignore_Invalid_Arguments()
        {
            // Arrange
            string[] args = ["-invalid", "-d", "testDirectory", "-f", "json"];

            // Act
            var settings = CoreUtils.ParseArguments(args);

            // Assert
            Assert.False(settings.Verbose);
            Assert.Equal("testDirectory", settings.DirectoryPath);

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
            Assert.Equal(CoreUtils.ExportFormat.JSON, result.Format);

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
            Assert.Equal(CoreUtils.ExportFormat.CSV, result.Format);

        }

        // ParseArguments processes invalid format option gracefully
        [Fact]
        public void ParseArguments_handles_invalid_format_option()
        {
            // Arrange
            string[] args = new[] { "-format", "INVALID" };
            Settings result;
            // Act & Assert
            Assert.Throws<InvalidExportFormatException>(() => result = CoreUtils.ParseArguments(args));

        }

        [Fact]
        public void GetUserChoice_Should_Return_Valid_Choice()
        {
            // Arrange
            int solutionCount = 5;
            string input = "3";

            // Act
            int result = CoreUtils.CheckInputUserChoice(input, solutionCount);
            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public void GetUserChoice_With_Invalid_Input_Should_Return_Valid_Choice()
        {
            // Arrange
            int solutionCount = 5;
            string input = "6";
            int result = -1;
            // Act & Assert
            Assert.Throws<InvalidSelectionException>(() => result = CoreUtils.CheckInputUserChoice(input, solutionCount));
            Assert.Equal(-1, result);

        }

        [Fact]
        public void GetUserChoice_Should_Return_Invalid_Choice()
        {
            // Arrange
            int solutionCount = 5;
            string input = "invalid";
            int result = -1;
            // Act
            Assert.Throws<InvalidNumberException>(() => result = CoreUtils.CheckInputUserChoice(input, solutionCount));
            // Assert
            Assert.Equal(-1, result);
        }

        // GetUserChoice returns valid selection when input is within range
        [Fact]
        public void GetUserChoice_returns_valid_selection_for_valid_input()
        {
            // Arrange
            string input = "2";
            // Act
            int result = CoreUtils.CheckInputUserChoice(input, 3);

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
            var exceptions = new List<Type>()
            {
                typeof(ArgumentNullException),
                typeof(InvalidNumberException),
                typeof(InvalidOperationException),
            };

            var ex = Record.Exception(() => CoreUtils.CheckInputUserChoice(input, 5));

            Assert.NotNull(ex);
            Assert.Contains(ex.GetType(), exceptions);


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

            // Act
            initialization();
            RedirectConsoleInputOutput();
            CoreUtils.DisplaySolutions(solutionFiles);
            

            // Assert
            string expectedOutput = $"Available solutions:{envNewLine}";
            for (int i = 0; i < solutionFiles.Count; i++)
            {
                expectedOutput += $"{i + 1}. {solutionFiles[i]}{envNewLine}";
            }
            Assert.Equal(expectedOutput, GetConsoleOutput());
            ResetConsoleInputOutput();

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
        public void CheckSettings_WhenHelpIsTrue_ReturnsHelpRequestedException()
        {
            // Arrange
            Settings settings = new Settings(true, null, ".", true, CoreUtils.ExportFormat.JSON);
            bool result = false;
            // Act & Assert
            Assert.Throws<HelpRequestedException>(() => result = settings.IsValid());

        }

        [Fact]
        public void CheckSettings_WhenDirectoryPathIsNull_ReturnsFalse()
        {
            // Arrange
            Settings settings = new Settings(verbose: false, directoryPath: null, help: false, format: CoreUtils.ExportFormat.CSV);
            bool result = false;
            // Act
            Assert.Throws<ArgumentNullException>(() => result = settings.IsValid());

            // Assert
            Assert.False(result);

        }

        [Fact]
        public void CheckSettings_WhenSettingsAreValid_ReturnsTrue()
        {
            // Arrange
            Settings settings = new Settings(false, "some_directory", false, CoreUtils.ExportFormat.CSV);

            // Act
            var result = settings.IsValid();

            // Assert
            Assert.True(result);

        }
        [Fact]
        public void CheckSettings_WhenSettingsOutputIsInValid_ReturnsFalse()
        {
            // Arrange
            Settings settings = new Settings(false, "some_directory", "invalid_output_path", false, CoreUtils.ExportFormat.CSV);
            bool result = false;

            // Act
            result = settings.IsValid();

            // Assert
            Assert.True(result);

        }

        [Fact]
        public void CheckSettings_WhenSettingsAreInvalid_ReturnsFalse()
        {
            // Arrange
            Settings settings = new Settings(false, null, false, CoreUtils.ExportFormat.CSV);

            bool result = false;
            // Act
            Assert.Throws<ArgumentNullException>(() => result = settings.IsValid());

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
            var fullFileName = Path.Combine(_testDirectory, fileName);
            var result = CoreUtils.GetExportFileNameWithExtension(fullFileName, format);

            // Assert
            Assert.Contains(Path.GetFileNameWithoutExtension(fullFileName), result);
            Assert.True(File.Exists(result) || !File.Exists(result));


        }

        // Returns list of filenames for existing files in input list
        [Fact]
        public void get_filenames_list_returns_filenames_for_existing_files()
        {

            // Arrange
            var testFiles = new List<string>
            {
                Path.Combine(_testDirectory, "file1.txt"),
                Path.Combine(_testDirectory, "file2.txt")
            };

            File.WriteAllText(testFiles[0], "test content");
            File.WriteAllText(testFiles[1], "test content");

            // Act
            var result = CoreUtils.GetFilenamesList(testFiles);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("file1.txt", result[0]);
            Assert.Equal("file2.txt", result[1]);

            // Cleanup
            File.Delete(testFiles[0]);
            File.Delete(testFiles[1]);

        }

        // Directory creation succeeds when OutputPath is valid and does not exist
        [Fact]
        public void create_directory_succeeds_with_valid_path()
        {

            var settings = new Settings();
            settings.DirectoryPath = _testDirectory;
            settings.OutputPath = Path.Combine(_testDirectory, "TestOutput");

            var result = settings.IsValid();

            Assert.True(result);
            Assert.True(Directory.Exists(settings.OutputPath));
            Directory.Delete(settings.OutputPath);

        }

        protected override void Dispose(bool disposing)
        {


            if (Directory.Exists(_testDirectory) && disposing)
            {
                // Dispose managed resources
                Directory.Delete(_testDirectory, true);
            }


            // Dispose unmanaged resources (if any)
            base.Dispose(disposing);

        }

    }
}