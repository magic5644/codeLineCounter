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
            // Arrange
            List<string> solutionFiles = new List<string>
            {
                "Solution1.sln",
                "Solution2.sln",
                "Solution3.sln"
            };

            // Redirect console output to a StringWriter
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                // Act
                CoreUtils.DisplaySolutions(solutionFiles);

                // Assert
                string expectedOutput = "Available solutions:\r\n1. Solution1.sln\r\n2. Solution2.sln\r\n3. Solution3.sln\r\n";
                Assert.Equal(expectedOutput, sw.ToString());
            }
        }

        [Fact]
        public void GetFilenamesList_Should_Return_List_Of_Filenames()
        {
            // Arrange
            List<string> solutionFiles = new List<string>
            {
                "Solution1.sln",
                "Solution2.sln",
                "Solution3.sln"
            };

            // Act
            List<string> result = CoreUtils.GetFilenamesList(solutionFiles);

            // Assert
            List<string> expectedFilenames = new List<string>
            {
                "Solution1.sln",
                "Solution2.sln",
                "Solution3.sln"
            };
            Assert.Equal(expectedFilenames, result);
        }
    }
}