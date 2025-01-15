using System.Text;
using CodeLineCounter.Models;
using DotNetGraph;
using DotNetGraph.Attributes;
using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;

namespace CodeLineCounter.Services
{
    public static class DependencyGraphGenerator
    {
        public static DotGraph GenerateGraphOnly(List<DependencyRelation> dependencies, string? filterNamespace = null, string? filterAssembly = null)
        {
            var filteredDependencies = dependencies;
            filteredDependencies = FilterNamespaceFromDependencies(dependencies, filterNamespace, filteredDependencies);
            filteredDependencies = FilterAssemblyFromDependencies(filterAssembly, filteredDependencies);

            var graph = new DotGraph();
            graph.Directed = true;

            var vertexInfo = new Dictionary<string, (int incoming, int outgoing)>();
            var namespaceGroups = new Dictionary<string, List<string>>();
            graph.WithLabel("DependencyGraph");
            graph.WithIdentifier("DependencyGraph", true);
            // Collect degree information and group by namespace
            foreach (var dep in filteredDependencies)
            {
                GroupByNamespace(vertexInfo, namespaceGroups, dep);
            }

            // Create clusters and add nodes
            foreach (var nsGroup in namespaceGroups)
            {
                DotSubgraph cluster = CreateCluster(nsGroup);

                foreach (var vertex in nsGroup.Value)
                {
                    DotNode node = CreateNode(vertexInfo, vertex);

                    cluster.Elements.Add(node);
                }

                graph.Elements.Add(cluster);
            }

            // Add edges
            foreach (var dep in filteredDependencies)
            {
                DotEdge edge = CreateEdge(dep);
                graph.Elements.Add(edge);
            }

            return graph;
        }

        private static DotEdge CreateEdge(DependencyRelation dep)
        {
            var sourceLabel = dep.SourceClass;
            var targetLabel = dep.TargetClass;

            var edge = new DotEdge();
            var dotIdentifierFrom = new DotIdentifier(sourceLabel);
            var dotIdentifierTo = new DotIdentifier(targetLabel);

            edge.From = dotIdentifierFrom;
            edge.To = dotIdentifierTo;
            edge.Style = DotEdgeStyle.Bold;
            return edge;
        }

        private static DotSubgraph CreateCluster(KeyValuePair<string, List<string>> nsGroup)
        {
            var cluster = new DotSubgraph();
            cluster.WithLabel($"cluster_{nsGroup.Key.Replace(".", "_")}");
            cluster.WithIdentifier($"cluster_{nsGroup.Key.Replace(".", "_")}", true);
            cluster.Label = nsGroup.Key;
            cluster.Style = DotSubgraphStyle.Dashed;

            return cluster;
        }

        public static DotNode CreateNode(Dictionary<string, (int incoming, int outgoing)> vertexInfo, string vertex)
        {
            var info = vertexInfo[vertex];
            var node = new DotNode();
            node.WithIdentifier(vertex, true);
            node.Label = $"{vertex}" + Environment.NewLine + $"\nIn: {info.incoming}, Out: {info.outgoing}";
            node.Shape = DotNodeShape.Oval;
            node.WithPenWidth(2);

            // Color nodes based on degrees
            if (info.incoming > info.outgoing)
            {
                node.FillColor = DotColor.MediumSeaGreen;
                node.Style = DotNodeStyle.Filled;
            }
            else if (info.incoming < info.outgoing)
            {
                node.FillColor = DotColor.Salmon;
                node.Style = DotNodeStyle.Filled;
            }

            return node;
        }

        private static void GroupByNamespace(Dictionary<string, (int incoming, int outgoing)> vertexInfo, Dictionary<string, List<string>> namespaceGroups, DependencyRelation dep)
        {
            var sourceLabel = dep.SourceClass;
            var targetLabel = dep.TargetClass;

            if (!vertexInfo.ContainsKey(sourceLabel))
            {
                vertexInfo[sourceLabel] = (dep.IncomingDegree, dep.OutgoingDegree);
            }
            if (!vertexInfo.ContainsKey(targetLabel))
            {
                vertexInfo[targetLabel] = (dep.IncomingDegree, dep.OutgoingDegree);
            }

            // Group by namespace
            if (!namespaceGroups.TryGetValue(dep.SourceNamespace, out var sourceNamespaceList))
            {
                sourceNamespaceList = [];
                namespaceGroups[dep.SourceNamespace] = sourceNamespaceList;

            }
            if (!sourceNamespaceList.Contains(dep.SourceClass))
            {
                sourceNamespaceList.Add(dep.SourceClass);
            }

            if (!namespaceGroups.TryGetValue(dep.TargetNamespace, out var targetNamespaceList))
            {
                targetNamespaceList = [];
                namespaceGroups[dep.TargetNamespace] = targetNamespaceList;
            }

            if (!targetNamespaceList.Contains(dep.TargetClass))
            {
                targetNamespaceList.Add(dep.TargetClass);
            }
        }

        public static async Task CompileGraphAndWriteToFile(string outputPath, DotGraph graph)
        {
            await using var writer = new StringWriter();
            var options = new CompilationOptions
            {
                Indented = true
            };
            var context = new CompilationContext(writer, options);
            graph.Directed = true;
            context.DirectedGraph = true;

            await graph.CompileAsync(context);
            var result = writer.GetStringBuilder().ToString();
            //using sync write for reliability with async we got some issues
            File.WriteAllText(outputPath, result);
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

    }
}