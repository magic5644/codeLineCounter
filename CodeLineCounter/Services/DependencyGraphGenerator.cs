using CodeLineCounter.Models;
using QuikGraph;
using QuikGraph.Graphviz;

namespace CodeLineCounter.Services
{

    public static class DependencyGraphGenerator
    {
        public static void GenerateGraph(List<DependencyRelation> dependencies, string outputPath)
        {
            var graph = new AdjacencyGraph<string, Edge<string>>();

            foreach (var dependency in dependencies)
            {
                if (!graph.ContainsVertex(dependency.SourceClass))
                    graph.AddVertex(dependency.SourceClass);
                if (!graph.ContainsVertex(dependency.TargetClass))
                    graph.AddVertex(dependency.TargetClass);

                graph.AddEdge(new Edge<string>(dependency.SourceClass, dependency.TargetClass));
            }

            var graphviz = new GraphvizAlgorithm<string, Edge<string>>(graph);
            graphviz.FormatVertex += (sender, args) =>
            {
                args.VertexFormat.Label = args.Vertex;
                args.VertexFormat.Shape = QuikGraph.Graphviz.Dot.GraphvizVertexShape.Box;
            };

            string dot = graphviz.Generate();
            File.WriteAllText(outputPath, dot);
            
        }
    }
}