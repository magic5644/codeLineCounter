using CodeLineCounter.Models;
using CodeLineCounter.Services;
using DotNetGraph.Core;
using DotNetGraph.Extensions;

namespace CodeLineCounter.Tests
{
    public class DependencyGraphGeneratorTests
    {
        [Fact]
        public async Task generate_graph_with_valid_dependencies_creates_dot_file()
        {
            // Arrange
            var dependencies = new List<DependencyRelation>
            {
                new DependencyRelation { SourceClass = "ClassA", SourceNamespace = "NamespaceA", SourceAssembly = "AssemblyA", TargetClass = "ClassB" , TargetNamespace = "NamespaceB", TargetAssembly = "AssemblyB", FilePath = "path/to/file", StartLine = 1},
                new DependencyRelation { SourceClass = "ClassB", SourceNamespace = "NamespaceB", SourceAssembly = "AssemblyB", TargetClass = "ClassC", TargetNamespace = "NamespaceB", TargetAssembly = "AssemblyB",  FilePath = "path/to/file", StartLine = 1}
            };

            string outputPath = Path.Combine(Path.GetTempPath(), "test_graph.dot");

            // Act

            DotGraph graph = DependencyGraphGenerator.GenerateGraphOnly(dependencies);
            await DependencyGraphGenerator.CompileGraphAndWriteToFile(outputPath, graph);

            // Assert
            Assert.True(File.Exists(outputPath));
            string content = await File.ReadAllTextAsync(outputPath);
            Assert.Contains("ClassA", content);
            Assert.Contains("ClassB", content);
            Assert.Contains("ClassC", content);

            File.Delete(outputPath);
        }

        // Empty dependencies list
        [Fact]
        public async Task generate_graph_with_empty_dependencies_creates_empty_graph()
        {
            // Arrange
            var dependencies = new List<DependencyRelation>();
            string outputPath = Path.Combine(Path.GetTempPath(), "empty_graph.dot");

            // Act
            DotGraph graph = DependencyGraphGenerator.GenerateGraphOnly(dependencies);
            await DependencyGraphGenerator.CompileGraphAndWriteToFile(outputPath, graph);

            // Assert
            Assert.True(File.Exists(outputPath));
            string content = await File.ReadAllTextAsync(outputPath);
            Assert.Contains("digraph", content);
            Assert.DoesNotContain("->", content);

            File.Delete(outputPath);
        }

        [Fact]
        public void create_node_sets_correct_fillcolor_and_style_incoming_greater()
        {
            var vertexInfo = new Dictionary<string, (int incoming, int outgoing)>
            {
                { "TestVertex", (3, 2) }
            };

            DotNode node = DependencyGraphGenerator.CreateNode(vertexInfo, "TestVertex");

            Assert.Equal(DotColor.MediumSeaGreen.ToHexString(), node.FillColor.Value);
            Assert.Equal(DotNodeStyle.Filled.FlagsToString(), node.Style.Value);
        }

        [Fact]
        public void create_node_sets_correct_fillcolor_and_style_incoming_lower()
        {
            var vertexInfo = new Dictionary<string, (int incoming, int outgoing)>
            {
                { "TestVertex", (3, 4) }
            };

            DotNode node = DependencyGraphGenerator.CreateNode(vertexInfo, "TestVertex");

            Assert.Equal(DotColor.Salmon.ToHexString(), node.FillColor.Value);
            Assert.Equal(DotNodeStyle.Filled.FlagsToString(), node.Style.Value);
        }


    }
}
