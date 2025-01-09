using CodeLineCounter.Models;
using CodeLineCounter.Services;
using DotNetGraph.Core;


namespace CodeLineCounter.Utils
{

    public static class DataExporter
    {
        private static readonly Dictionary<CoreUtils.ExportFormat, IExportStrategy> _exportStrategies = new()
        {
            { CoreUtils.ExportFormat.CSV, new CsvExportStrategy() },
            { CoreUtils.ExportFormat.JSON, new JsonExportStrategy() }
        };

        public static void Export<T>(string filePath, T data, CoreUtils.ExportFormat format) where T : class
        {
            ArgumentNullException.ThrowIfNull(data);

            ExportCollection(filePath, [data], format);
        }

        public static void ExportCollection<T>(string? filePath, IEnumerable<T> data, CoreUtils.ExportFormat format) where T : class
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            
            ArgumentNullException.ThrowIfNull(data);

            try
            {
                filePath = CoreUtils.GetExportFileNameWithExtension(filePath, format);
                _exportStrategies[format].Export(filePath, data);
            }
            catch (IOException ex)
            {
                throw new IOException($"Failed to export data to {filePath}", ex);
            }
        }

        public static void ExportDuplications(string filePath, List<DuplicationCode> duplications, CoreUtils.ExportFormat format)
        {
            ExportCollection(filePath, duplications, format);
        }

        public static async Task ExportDependencies(string filePath, List<DependencyRelation> dependencies,CoreUtils.ExportFormat format)
        {
            string outputFilePath = CoreUtils.GetExportFileNameWithExtension(filePath, format);
            ExportCollection(outputFilePath, dependencies, format);

            DotGraph graph =  DependencyGraphGenerator.GenerateGraphOnly(dependencies);
            await DependencyGraphGenerator.CompileGraphAndWriteToFile(Path.ChangeExtension(outputFilePath, ".dot"), graph);
        }

        public static void ExportMetrics(string filePath, List<NamespaceMetrics> metrics,
            Dictionary<string, int> projectTotals, int totalLines,
            List<DuplicationCode> duplications, string? solutionPath, CoreUtils.ExportFormat format)
        {
            try
            {
                string? currentProject = null;
                filePath = CoreUtils.GetExportFileNameWithExtension(filePath, format);
                List<NamespaceMetrics> namespaceMetrics = [];
                var duplicationCounts = GetDuplicationCounts(duplications);

                foreach (var metric in metrics)
                {
                    if (!string.Equals(currentProject, metric.ProjectName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentProject != null)
                        {
                            namespaceMetrics.Add(new NamespaceMetrics
                            {
                                ProjectName = currentProject,
                                ProjectPath = "Total",
                                LineCount = projectTotals[currentProject]
                            });
                        }
                        currentProject = metric.ProjectName;
                    }
                    metric.CodeDuplications = GetFileDuplicationsCount(duplicationCounts, metric, solutionPath);
                    namespaceMetrics.Add(metric);
                }

                if (currentProject != null)
                {
                    namespaceMetrics.Add(new NamespaceMetrics
                    {
                        ProjectName = currentProject,
                        ProjectPath = "Total",
                        LineCount = projectTotals[currentProject]
                    });
                    namespaceMetrics.Add(new NamespaceMetrics
                    {
                        ProjectName = "Total",
                        ProjectPath = "",
                        LineCount = totalLines
                    });
                }

                ExportCollection(filePath, namespaceMetrics, format);
            }
            catch (IOException ex)
            {
                throw new IOException($"Failed to export metrics to {filePath}", ex);
            }
        }

        public static Dictionary<string, uint> GetDuplicationCounts(List<DuplicationCode> duplications)
        {
            var duplicationCounts = new Dictionary<string, uint>(duplications.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var duplication in duplications)
            {
                var normalizedPath = Path.GetFullPath(duplication.FilePath, Path.GetFullPath("."));
                duplicationCounts[normalizedPath] = duplicationCounts.GetValueOrDefault(normalizedPath) + 1;
            }
            return duplicationCounts;
        }

        public static int GetFileDuplicationsCount(Dictionary<string, uint> duplicationCounts,
            NamespaceMetrics metric, string? solutionPath)
        {
            if (metric.FilePath == null) return 0;

            var normalizedPath = Path.Combine(
                string.IsNullOrEmpty(solutionPath) ? Path.GetFullPath(".") : solutionPath,
                metric.FilePath ?? string.Empty);

            return (int)duplicationCounts.GetValueOrDefault(normalizedPath);
        }
    }

    public interface IExportStrategy
    {
        void Export<T>(string filePath, IEnumerable<T> data) where T : class;
    }

    public class CsvExportStrategy : IExportStrategy
    {
        public void Export<T>(string filePath, IEnumerable<T> data) where T : class
        {
            CsvHandler.Serialize(data, filePath);
        }
    }

    public class JsonExportStrategy : IExportStrategy
    {
        public void Export<T>(string filePath, IEnumerable<T> data) where T : class
        {
            JsonHandler.Serialize(data, filePath);
        }
    }
}