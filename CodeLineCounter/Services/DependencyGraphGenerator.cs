using CodeLineCounter.Models;
using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;

namespace CodeLineCounter.Services
{
    public static class DependencyGraphGenerator
    {
        public static void GenerateGraph(List<DependencyRelation> dependencies, string outputPath, string? filterNamespace = null, string? filterAssembly = null)
        {
            var filteredDependencies = dependencies;
            filteredDependencies = FilterNamespaceFromDependencies(dependencies, filterNamespace, filteredDependencies);
            filteredDependencies = FilterAssemblyFromDependencies(filterAssembly, filteredDependencies);

            var graph = new BidirectionalGraph<string, Edge<string>>();
            var vertexInfo = new Dictionary<string, (int incoming, int outgoing)>();
            var namespaceGroups = new Dictionary<string, List<string>>();

            // Collect degree information and group by namespace
            foreach (var dep in filteredDependencies)
            {
                var sourceLabel = GetVertexLabel(dep.SourceClass, dep.SourceNamespace, dep.SourceAssembly);
                var targetLabel = GetVertexLabel(dep.TargetClass, dep.TargetNamespace, dep.TargetAssembly);

                if (!vertexInfo.ContainsKey(sourceLabel))
                {
                    vertexInfo[sourceLabel] = (dep.IncomingDegree, dep.OutgoingDegree);
                }
                if (!vertexInfo.ContainsKey(targetLabel))
                {
                    vertexInfo[targetLabel] = (dep.IncomingDegree, dep.OutgoingDegree);
                }

                if (!graph.ContainsVertex(sourceLabel))
                    graph.AddVertex(sourceLabel);
                if (!graph.ContainsVertex(targetLabel))
                    graph.AddVertex(targetLabel);

                graph.AddEdge(new Edge<string>(sourceLabel, targetLabel));

                // Group by namespace
                if (!namespaceGroups.ContainsKey(dep.SourceNamespace))
                {
                    namespaceGroups[dep.SourceNamespace] = new List<string>();
                }
                if (!namespaceGroups.ContainsKey(dep.TargetNamespace))
                {
                    namespaceGroups[dep.TargetNamespace] = new List<string>();
                }

                if (!namespaceGroups[dep.SourceNamespace].Contains(sourceLabel))
                {
                    namespaceGroups[dep.SourceNamespace].Add(sourceLabel);
                }
                if (!namespaceGroups[dep.TargetNamespace].Contains(targetLabel))
                {
                    namespaceGroups[dep.TargetNamespace].Add(targetLabel);
                }
            }


            var graphviz = new GraphvizAlgorithm<string, Edge<string>>(graph);
            graphviz.FormatVertex += (sender, args) =>
            {
                var info = vertexInfo[args.Vertex];
                args.VertexFormat.Label = $"{args.Vertex}\\nIn: {info.incoming}, Out: {info.outgoing}";
                args.VertexFormat.Shape = GraphvizVertexShape.Box;

                // Color the nodes based on their degrees
                if (info.incoming > info.outgoing)
                {
                    args.VertexFormat.Style = GraphvizVertexStyle.Filled;
                    args.VertexFormat.FillColor = GraphvizColor.LightBlue;
                }
                else if (info.incoming < info.outgoing)
                {
                    args.VertexFormat.Style = GraphvizVertexStyle.Filled;
                    args.VertexFormat.FillColor = GraphvizColor.LightCoral;
                }
            };

            graphviz.FormatCluster += (sender, args) =>
            {
                args.GraphFormat.Label = args.Cluster.ToString();
            };

            // foreach (var nsGroup in namespaceGroups)
            // {
            //     //var cluster = graphviz.Clusters.Add(nsGroup.Key);
            //     // foreach (var vertex in nsGroup.Value)
            //     // {
            //     //     graph.AddVertexToCluster(cluster, vertex);
            //     //     cluster.AddVertex(vertex);
            //     // }
            // }

            var dot = graphviz.Generate();
            File.WriteAllText(outputPath, dot);
        }

        private static List<DependencyRelation> FilterAssemblyFromDependencies(string? filterAssembly, List<DependencyRelation> filteredDependencies)
        {
            if (!string.IsNullOrEmpty(filterAssembly))
            {
                filteredDependencies = filteredDependencies.Where(d =>
                    d.SourceAssembly.Equals(filterAssembly) ||
                    d.TargetAssembly.Equals(filterAssembly)).ToList();
            }

            return filteredDependencies;
        }

        private static List<DependencyRelation> FilterNamespaceFromDependencies(List<DependencyRelation> dependencies, string? filterNamespace, List<DependencyRelation> filteredDependencies)
        {
            if (!string.IsNullOrEmpty(filterNamespace))
            {
                filteredDependencies = dependencies.Where(d =>
                    d.SourceNamespace.Contains(filterNamespace) ||
                    d.TargetNamespace.Contains(filterNamespace)).ToList();
            }

            return filteredDependencies;
        }

        private static string GetVertexLabel(string className, string namespaceName, string assemblyName)
        {
            return $"{className}\\n{namespaceName}\\n{assemblyName}";
        }

        
    }
}