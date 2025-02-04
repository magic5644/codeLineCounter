using CodeLineCounter.Models;
using CodeLineCounter.Services;
using DotNetGraph.Core;
using DotNetGraph.Extensions;

namespace CodeLineCounter.Tests
{
    public class DependencyGraphGeneratorTests : IDisposable
    {

        private readonly string _testDirectory;
        private bool _disposed;

        public DependencyGraphGeneratorTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "DependencyGraphGeneratorTests");
            Directory.CreateDirectory(_testDirectory);
        }
        [Fact]
        public async Task generate_graph_with_valid_dependencies_creates_dot_file()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                // Arrange
                var dependencies = new List<DependencyRelation>
            {
                new DependencyRelation { SourceClass = "ClassA", SourceNamespace = "NamespaceA", SourceAssembly = "AssemblyA", TargetClass = "ClassB" , TargetNamespace = "NamespaceB", TargetAssembly = "AssemblyB", FilePath = "path/to/file", StartLine = 1},
                new DependencyRelation { SourceClass = "ClassB", SourceNamespace = "NamespaceB", SourceAssembly = "AssemblyB", TargetClass = "ClassC", TargetNamespace = "NamespaceB", TargetAssembly = "AssemblyB",  FilePath = "path/to/file", StartLine = 1}
            };

                string fileName = "testGraph.dot";

                string outputPath = Path.Combine(_testDirectory, fileName);

                // Act

                DotGraph graph = DependencyGraphGenerator.GenerateGraphOnly(dependencies);
                Directory.CreateDirectory(_testDirectory);
                await DependencyGraphGenerator.CompileGraphAndWriteToFile(fileName, _testDirectory, graph);

                // Assert
                Assert.True(File.Exists(outputPath));
                string content = await File.ReadAllTextAsync(outputPath);
                Assert.Contains("ClassA", content);
                Assert.Contains("ClassB", content);
                Assert.Contains("ClassC", content);

                File.Delete(outputPath);
            }

        }

        // Empty dependencies list
        [Fact]
        public async Task generate_graph_with_empty_dependencies_creates_empty_graph()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                // Arrange
                var dependencies = new List<DependencyRelation>();
                string filename = "empty_graph.dot";
                string outputPath = Path.Combine(_testDirectory, filename);

                // Act
                DotGraph graph = DependencyGraphGenerator.GenerateGraphOnly(dependencies);
                await DependencyGraphGenerator.CompileGraphAndWriteToFile(filename, _testDirectory, graph);

                // Assert
                Assert.True(File.Exists(outputPath));
                string content = await File.ReadAllTextAsync(outputPath);
                Assert.Contains("digraph", content);
                Assert.DoesNotContain("->", content);

                File.Delete(outputPath);
            }

        }

        [Fact]
        public void create_node_sets_correct_fillcolor_and_style_incoming_greater()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                var vertexInfo = new Dictionary<string, (int incoming, int outgoing)>
            {
                { "TestVertex", (3, 2) }
            };

                DotNode node = DependencyGraphGenerator.CreateNode(vertexInfo, "TestVertex");

                Assert.Equal(DotColor.MediumSeaGreen.ToHexString(), node.FillColor.Value);
                Assert.Equal(DotNodeStyle.Filled.FlagsToString(), node.Style.Value);
            }

        }

        [Fact]
        public void create_node_sets_correct_fillcolor_and_style_incoming_lower()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                var vertexInfo = new Dictionary<string, (int incoming, int outgoing)>
            {
                { "TestVertex", (3, 4) }
            };

                DotNode node = DependencyGraphGenerator.CreateNode(vertexInfo, "TestVertex");

                Assert.Equal(DotColor.Salmon.ToHexString(), node.FillColor.Value);
                Assert.Equal(DotNodeStyle.Filled.FlagsToString(), node.Style.Value);
            }

        }

        // Returns empty quoted string '""' for non-empty input string
        [Fact]
        public void enclose_string_in_quotes_returns_empty_quoted_string_for_nonempty_input()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                // Arrange
                var input = "test string";

                // Act 
                var result = DependencyGraphGenerator.EncloseNotEmptyOrNullStringInQuotes(input);

                // Assert
                Assert.Equal("\"test string\"", result);
            }

        }

        // Returns quoted string with null value for null input
        [Fact]
        public void enclose_string_in_quotes_returns_quoted_null_for_null_input()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                // Arrange
                string? input = null;

                // Act
                var result = DependencyGraphGenerator.EncloseNotEmptyOrNullStringInQuotes(input);

                // Assert
                Assert.Equal(string.Empty, result);
            }

        }

        // Graph initialization with empty values
        [Fact]
        public void initialize_graph_sets_default_label_and_identifier()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                var graph = DependencyGraphGenerator.InitializeGraph();

                Assert.Equal("DependencyGraph", graph.Label.Value);
                Assert.Equal("DependencyGraph", graph.Identifier.Value);
            }

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DependencyGraphGeneratorTests()
        {
            Dispose(false);
        }
    }
}
