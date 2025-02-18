using System.Text;
using CodeLineCounter.Models;
using DotNetGraph;
using DotNetGraph.Attributes;
using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using Microsoft.IO;

namespace CodeLineCounter.Services
{
    public class GraphvizUnflattenOptions
    {
        public int ChainLimit { get; set; } = 0;
        public int MaxMinlen { get; set; } = 0;
        public bool DoFans { get; set; } = false;
    }

    public static class DependencyGraphGenerator
    {

        private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();
        public static DotGraph GenerateGraphOnly(List<DependencyRelation> dependencies, string? filterNamespace = null, string? filterAssembly = null)
        {
            var filteredDependencies = dependencies
                .Where(d => (string.IsNullOrEmpty(filterNamespace) || d.SourceNamespace.Contains(filterNamespace) || d.TargetNamespace.Contains(filterNamespace)) &&
                             (string.IsNullOrEmpty(filterAssembly) || d.SourceAssembly.Equals(filterAssembly) || d.TargetAssembly.Equals(filterAssembly)))
                .ToList();

            var vertexInfo = new Dictionary<string, (int incoming, int outgoing)>();
            var namespaceGroups = new Dictionary<string, List<string>>();
            DotGraph graph = InitializeGraph();

            // Collect degree information and group by namespace
            CollectDegreeInformationAndGroupByNamespace(filteredDependencies, vertexInfo, namespaceGroups);

            // Create clusters and add nodes
            CreateClustersAndNodes(vertexInfo, namespaceGroups, graph);

            // Add edges
            AddEdgesBetweenDependencies(filteredDependencies, graph);

            return graph;
        }

        public static void AddEdgesBetweenDependencies(List<DependencyRelation> filteredDependencies, DotGraph graph)
        {
            foreach (var dep in filteredDependencies)
            {
                DotEdge edge = CreateEdge(dep);
                graph.Elements.Add(edge);
            }
        }

        public static void CreateClustersAndNodes(Dictionary<string, (int incoming, int outgoing)> vertexInfo, Dictionary<string, List<string>> namespaceGroups, DotGraph graph)
        {
            foreach (var nsGroup in namespaceGroups)
            {
                DotSubgraph cluster = CreateCluster(nsGroup);

                foreach (var vertex in nsGroup.Value)
                {
                    DotNode node = CreateNode(vertexInfo, EncloseNotEmptyOrNullStringInQuotes(vertex));
                    cluster.Elements.Add(node);
                }

                graph.Elements.Add(cluster);
            }
        }

        public static void CollectDegreeInformationAndGroupByNamespace(List<DependencyRelation> filteredDependencies, Dictionary<string, (int incoming, int outgoing)> vertexInfo, Dictionary<string, List<string>> namespaceGroups)
        {
            foreach (var dep in filteredDependencies)
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
                    sourceNamespaceList = new List<string>();
                    namespaceGroups[dep.SourceNamespace] = sourceNamespaceList;
                }
                if (!sourceNamespaceList.Contains(dep.SourceClass))
                {
                    sourceNamespaceList.Add(dep.SourceClass);
                }

                if (!namespaceGroups.TryGetValue(dep.TargetNamespace, out var targetNamespaceList))
                {
                    targetNamespaceList = new List<string>();
                    namespaceGroups[dep.TargetNamespace] = targetNamespaceList;
                }

                if (!targetNamespaceList.Contains(dep.TargetClass))
                {
                    targetNamespaceList.Add(dep.TargetClass);
                }
            }
        }

        public static DotGraph InitializeGraph()
        {
            var graph = new DotGraph();
            graph.Directed = true;
            graph.WithLabel("DependencyGraph");
            graph.WithIdentifier("DependencyGraph", true);
            return graph;
        }

        private static DotEdge CreateEdge(DependencyRelation dep)
        {
            var sourceLabel = dep.SourceClass;
            var targetLabel = dep.TargetClass;

            var edge = new DotEdge();
            var dotIdentifierFrom = new DotIdentifier(EncloseNotEmptyOrNullStringInQuotes(sourceLabel));
            var dotIdentifierTo = new DotIdentifier(EncloseNotEmptyOrNullStringInQuotes(targetLabel));

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
            var info = vertexInfo[RemoveQuotes(vertex) ?? vertex];
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

        public static async Task CompileGraphAndWriteToFile(string fileName, string outputPath, DotGraph graph)
        {
            // Use memory buffer
            using var memoryStream = MemoryStreamManager.GetStream();
            using var writer = new StreamWriter(memoryStream);

            var options = new CompilationOptions { Indented = true };
            var context = new CompilationContext(writer, options);
            graph.Directed = true;
            context.DirectedGraph = true;

            await graph.CompileAsync(context);
            await writer.FlushAsync(); // Ensure all data is written to memory

            memoryStream.Position = 0; // Reset position to start
            using var fileStream = File.Create(Path.Combine(outputPath, fileName));
            await memoryStream.CopyToAsync(fileStream); // Write complete buffer to file
        }

        public static string EncloseNotEmptyOrNullStringInQuotes(string? str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return $"\"{str}\"";
            }
            else
            {
                return string.Empty;
            }
        }

        public static string RemoveQuotes(string str)
        {
            return str.Replace("\"", "");
        }
    }
}
