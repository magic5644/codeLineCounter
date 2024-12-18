using CodeLineCounter.Models;
using CodeLineCounter.Services;

namespace CodeLineCounter.Tests
{
    public class DependencyGraphGeneratorTests
    {
        [Fact]
        public void generate_graph_with_valid_dependencies_creates_dot_file()
        {
            // Arrange
            var dependencies = new List<DependencyRelation>
            {
                new DependencyRelation { SourceClass = "ClassA", TargetClass = "ClassB" , FilePath = "path/to/file", StartLine = 1},
                new DependencyRelation { SourceClass = "ClassB", TargetClass = "ClassC",  FilePath = "path/to/file", StartLine = 1}
            };

            string outputPath = Path.Combine(Path.GetTempPath(), "test_graph.dot");

            // Act
            DependencyGraphGenerator.GenerateGraph(dependencies, outputPath);

            // Assert
            Assert.True(File.Exists(outputPath));
            string content = File.ReadAllText(outputPath);
            Assert.Contains("ClassA", content);
            Assert.Contains("ClassB", content);
            Assert.Contains("ClassC", content);

            File.Delete(outputPath);
        }

        // Empty dependencies list
        [Fact]
        public void generate_graph_with_empty_dependencies_creates_empty_graph()
        {
            // Arrange
            var dependencies = new List<DependencyRelation>();
            string outputPath = Path.Combine(Path.GetTempPath(), "empty_graph.dot");

            // Act
            DependencyGraphGenerator.GenerateGraph(dependencies, outputPath);

            // Assert
            Assert.True(File.Exists(outputPath));
            string content = File.ReadAllText(outputPath);
            Assert.Contains("digraph", content);
            Assert.DoesNotContain("->", content);

            File.Delete(outputPath);
        }


    }
}
