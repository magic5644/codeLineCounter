using CodeLineCounter.Models;
using CodeLineCounter.Utils;

namespace CodeLineCounter.Tests
{
    public class DataExporterTests : IDisposable
    {
        private readonly string _testDirectory;
        private bool _disposed;

        public DataExporterTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "DataExporterTests");
            Directory.CreateDirectory(_testDirectory);
        }

        [Theory]
        [InlineData(CoreUtils.ExportFormat.CSV, ".csv")]
        [InlineData(CoreUtils.ExportFormat.JSON, ".json")]
        public void Export_SingleItem_CreatesFileWithCorrectExtension(CoreUtils.ExportFormat format, string expectedExtension)
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            // Arrange
            var testItem = new TestClass { Id = 1, Name = "Test" };
            var filePath = Path.Combine(_testDirectory, "test");

            // Act
            DataExporter.Export(filePath, testItem, format);

            // Assert
            Assert.True(File.Exists(filePath + expectedExtension));
        }

        [Theory]
        [InlineData(CoreUtils.ExportFormat.CSV, ".csv")]
        [InlineData(CoreUtils.ExportFormat.JSON, ".json")]
        public void ExportCollection_WithMultipleItems_CreatesFile(CoreUtils.ExportFormat format, string expectedExtension)
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            // Arrange
            var items = new List<TestClass>
            {
                new() { Id = 1, Name = "Test1" },
                new() { Id = 2, Name = "Test2" }
            };
            var filePath = Path.Combine(_testDirectory, "collection");

            // Act
            DataExporter.ExportCollection(filePath, items, format);

            // Assert
            Assert.True(File.Exists(filePath + expectedExtension));
        }

        [Fact]
        public void ExportMetrics_CreatesFileWithCorrectData()
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            // Arrange
            var metrics = new List<NamespaceMetrics>
            {
                new() { ProjectName = "Project1", LineCount = 100 },
                new() { ProjectName = "Project2", LineCount = 200 }
            };
            var projectTotals = new Dictionary<string, int>
            {
                { "Project1", 100 },
                { "Project2", 200 }
            };
            var duplications = new List<DuplicationCode>();
            var filePath = Path.Combine(_testDirectory, "metrics");

            // Act
            DataExporter.ExportMetrics(filePath, metrics, projectTotals, 300, duplications, ".", CoreUtils.ExportFormat.CSV);

            // Assert
            Assert.True(File.Exists(filePath + ".csv"));
        }

        [Fact]
        public void ExportDuplications_CreatesFileWithCorrectData()
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            // Arrange
            var duplications = new List<DuplicationCode>
            {
                new() { CodeHash = "hash1", FilePath = "file1.cs", MethodName = "method1", StartLine = 10, NbLines = 20 },
                new() { CodeHash = "hash2", FilePath = "file2.cs", MethodName = "method2", StartLine = 8, NbLines = 10 }
            };
            var filePath = Path.Combine(_testDirectory, "duplications");

            // Act
            DataExporter.ExportDuplications(filePath, duplications, CoreUtils.ExportFormat.CSV);

            // Assert
            Assert.True(File.Exists(filePath + ".csv"));
        }

        [Fact]
        public void get_duplication_counts_returns_correct_counts_for_duplicate_paths()
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            var duplications = new List<DuplicationCode>
            {
                new DuplicationCode { FilePath = "file1.cs" },
                new DuplicationCode { FilePath = "file1.cs" },
                new DuplicationCode { FilePath = "file2.cs" },
                new DuplicationCode { FilePath = "file1.cs" }
            };

            var result = DataExporter.GetDuplicationCounts(duplications);

            Assert.Equal(3u, result[Path.GetFullPath("file1.cs")]);
            Assert.Equal(1u, result[Path.GetFullPath("file2.cs")]);
        }

        [Fact]
        public void get_duplication_counts_returns_empty_dictionary_for_empty_list()
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            var duplications = new List<DuplicationCode>();

            var result = DataExporter.GetDuplicationCounts(duplications);

            Assert.Empty(result);
        }

        [Fact]
        public void get_duplication_counts_handles_absolute_paths()
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            var absolutePath = Path.GetFullPath(@"C:\test\file.cs");
            var duplications = new List<DuplicationCode>
            {
                new DuplicationCode { FilePath = absolutePath },
                new DuplicationCode { FilePath = absolutePath }
            };

            var result = DataExporter.GetDuplicationCounts(duplications);

            Assert.Equal(2u, result[absolutePath]);
        }

        [Fact]
        public void get_file_duplications_count_returns_correct_count_when_path_exists()
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            var duplicationCounts = new Dictionary<string, uint>
            {
                { Path.Combine(Path.GetFullPath("."), "test.cs"), 5 }
            };

            var metric = new NamespaceMetrics { FilePath = "test.cs" };

            var result = DataExporter.GetFileDuplicationsCount(duplicationCounts, metric, null);

            Assert.Equal(5, result);
        }

        [Fact]
        public void get_file_duplications_count_returns_zero_when_path_not_found()
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            var duplicationCounts = new Dictionary<string, uint>();

            var metric = new NamespaceMetrics { FilePath = "nonexistent.cs" };

            var result = DataExporter.GetFileDuplicationsCount(duplicationCounts, metric, null);

            Assert.Equal(0, result);
        }

        [Fact]
        public void get_file_duplications_count_returns_zero_when_filepath_null()
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            var duplicationCounts = new Dictionary<string, uint>
            {
                { Path.Combine(Path.GetFullPath("."), "test.cs"), 5 }
            };

            var metric = new NamespaceMetrics { FilePath = null };

            var result = DataExporter.GetFileDuplicationsCount(duplicationCounts, metric, null);

            Assert.Equal(0, result);
        }

        // Handles empty string solutionPath by using current directory
        [Fact]
        public void get_file_duplications_count_uses_current_dir_for_empty_solution_path()
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            var expectedPath = Path.Combine(Path.GetFullPath("."), "test.cs");
            var duplicationCounts = new Dictionary<string, uint>
            {
                { expectedPath, 3 }
            };

            var metric = new NamespaceMetrics { FilePath = "test.cs" };

            var result = DataExporter.GetFileDuplicationsCount(duplicationCounts, metric, string.Empty);

            Assert.Equal(3, result);
        }

        // Handles null solutionPath by using current directory
        [Fact]
        public void get_file_duplications_count_uses_current_dir_for_null_solution_path()
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            var expectedPath = Path.Combine(Path.GetFullPath("."), "test.cs");
            var duplicationCounts = new Dictionary<string, uint>
            {
                { expectedPath, 4 }
            };

            var metric = new NamespaceMetrics { FilePath = "test.cs" };

            var result = DataExporter.GetFileDuplicationsCount(duplicationCounts, metric, null);

            Assert.Equal(4, result);
        }

        // Successfully exports dependencies to specified format (CSV/JSON) and creates DOT file
        [Fact]
        public async Task export_dependencies_creates_files_in_correct_formats()
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            // Arrange
            var dependencies = new List<DependencyRelation>
            {
                new DependencyRelation { SourceClass = "ClassA", SourceNamespace = "NamespaceA", SourceAssembly = "AssemblyA", TargetClass = "ClassB", TargetNamespace = "NamespaceB", TargetAssembly = "AssemblyB", FilePath = "file1.cs", StartLine = 10 },
            };

            var testFilePath = Path.Combine(Path.GetFullPath("."),"test_export.dot");
            var format = CoreUtils.ExportFormat.JSON;

            // Act
            await DataExporter.ExportDependencies(testFilePath, dependencies, format);

            // Assert
            string expectedJsonPath = CoreUtils.GetExportFileNameWithExtension(testFilePath, format);
            string expectedDotPath = Path.ChangeExtension(expectedJsonPath, ".dot");

            Assert.True(File.Exists(expectedJsonPath));
            Assert.True(File.Exists(expectedDotPath));

            try
            {
                if (File.Exists(expectedJsonPath))
                {
                    File.Delete(expectedJsonPath);
                }

                if (File.Exists(expectedDotPath))
                {
                    File.Delete(expectedDotPath);
                }
            }
            catch (IOException ex)
            {
                throw new IOException($"Error deleting files: {ex.Message}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"Access denied while deleting files: {ex.Message}", ex);
            }
        }

        // Throws ArgumentException when file path is null
        [Fact]
        public void export_collection_throws_when_filepath_null()
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            // Arrange
            string? filePath = null;
            var testData = new List<TestClass> { new TestClass { Id = 1, Name = "Test" } };
            var format = CoreUtils.ExportFormat.CSV;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                DataExporter.ExportCollection(filePath, testData, format));
            Assert.Equal("File path cannot be null or empty (Parameter 'filePath')", exception.Message);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && Directory.Exists(_testDirectory))
                {
                    // Dispose managed resources
                    Directory.Delete(_testDirectory, true);
                }

                // Dispose unmanaged resources (if any)

                _disposed = true;
            }
        }

        private class TestClass
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DataExporterTests()
        {
            Dispose(false);
        }


    }
}