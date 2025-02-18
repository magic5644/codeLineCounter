using CodeLineCounter.Services;

namespace CodeLineCounter.Tests
{
    public class CodeDuplicationCheckerTests : TestBase
    {
        private readonly string _testDirectory;

        public CodeDuplicationCheckerTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "CodeDuplicationCheckerTests");
            Directory.CreateDirectory(_testDirectory);
        }

        [Fact]
        public void DetectCodeDuplicationInFiles_ShouldDetectDuplicates()
        {

            // Arrange
            var file1 = Path.Combine(_testDirectory, "TestFile1.cs");
            var file2 = Path.Combine(_testDirectory, "TestFile2.cs");

            var code1 = @"
                public class TestClass
                {
                    public void TestMethod()
                    {
                        // Some code
                        var i = 1;
                        var j = 2;
                        var k = i + j;
                    }
                }";

            var code2 = @"
                public class AnotherTestClass
                {
                    public void AnotherTestMethod()
                    {
                        // Some code
                        var i = 1;
                        var j = 2;
                        var k = i + j;
                    }
                }";

            File.WriteAllText(file1, code1);
            File.WriteAllText(file2, code2);

            var files = new List<string> { file1, file2 };
            var checker = new CodeDuplicationChecker();

            // Act
            checker.DetectCodeDuplicationInFiles(files);
            var result = checker.GetCodeDuplicationMap();

            // Assert
            Assert.NotEmpty(result);
            var duplicateEntry = result.First();
            Assert.Equal(2, duplicateEntry.Value.Count); // Both files should be detected as duplicates

            // Clean up
            File.Delete(file1);
            File.Delete(file2);

        }

        [Fact]
        public void DetectCodeDuplicationInSourceCode_ShouldDetectDuplicates()
        {

            // Arrange
            var checker = new CodeDuplicationChecker();

            var sourceCode1 = @"
                public class TestClass
                {
                    public void TestMethod()
                    {
                        // Some code
                        var i = 1;
                        var j = 2;
                        var k = i + j;
                    }
                }";

            var sourceCode2 = @"
                public class AnotherTestClass
                {
                    public void AnotherTestMethod()
                    {
                        // Some code
                        var i = 1;
                        var j = 2;
                        var k = i + j;
                    }
                }";

            var file1 = Path.Combine(_testDirectory, "TestFile3.cs");
            var file2 = Path.Combine(_testDirectory, "TestFile4.cs");

            // Act
            checker.DetectCodeDuplicationInSourceCode(file1, sourceCode1);
            checker.DetectCodeDuplicationInSourceCode(file2, sourceCode2);
            var result = checker.GetCodeDuplicationMap();

            // Assert
            Assert.NotEmpty(result);
            var duplicateEntry = result.First();
            Assert.Equal(2, duplicateEntry.Value.Count); // Both methods should be detected as duplicates


        }

        [Fact]
        public void DetectCodeDuplicationInSourceCode_ShouldNotDetectDuplicatesForDifferentCode()
        {

            // Arrange
            var checker = new CodeDuplicationChecker();

            var sourceCode1 = @"
                public class TestClass
                {
                    public void TestMethod()
                    {
                        // Some code
                        var i = 1;
                        var j = 2 + i;
                        var k = j * j;
                    }
                }";

            var sourceCode2 = @"
                public class AnotherTestClass
                {
                    public void AnotherTestMethod()
                    {
                        // Different code
                    }
                }";

            var file1 = Path.Combine(_testDirectory, "TestFile5.cs");
            var file2 = Path.Combine(_testDirectory, "TestFile6.cs");

            // Act
            checker.DetectCodeDuplicationInSourceCode(file1, sourceCode1);
            checker.DetectCodeDuplicationInSourceCode(file2, sourceCode2);
            var result = checker.GetCodeDuplicationMap();

            // Assert
            Assert.Empty(result); // No duplicates should be detected


        }

        protected override void Dispose(bool disposing)
        {
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
