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

        public static void Export<T>(string baseFilename, string outputPath, T data, CoreUtils.ExportFormat format) where T : class
        {
            ArgumentNullException.ThrowIfNull(data);

            ExportCollection(baseFilename, outputPath, [data], format);
        }


        public static void ExportCollection<T>(string? filename, string outputPath, IEnumerable<T> data, CoreUtils.ExportFormat format) where T : class
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("File path cannot be null or empty", nameof(filename));
            
            ArgumentNullException.ThrowIfNull(data);

            try
            {
                filename = CoreUtils.GetExportFileNameWithExtension(filename, format);
                _exportStrategies[format].Export(filename, outputPath, data);
            }
            catch (IOException ex)
            {
                throw new IOException($"Failed to export data to {filename}", ex);
            }
        }

        public static void ExportDuplications(string baseFileName, string outputPath, List<DuplicationCode> duplications, CoreUtils.ExportFormat format)
        {
            PrepareDirectoryForOutput(outputPath);

            ExportCollection(baseFileName,outputPath, duplications, format);
        }

        public static async Task ExportDependencies(string baseFileName,string outputPath, List<DependencyRelation> dependencies, CoreUtils.ExportFormat format)
        {
            PrepareDirectoryForOutput(outputPath);
            string filename = CoreUtils.GetExportFileNameWithExtension(baseFileName, format);


            ExportCollection(filename, outputPath, dependencies, format);

            DotGraph graph = DependencyGraphGenerator.GenerateGraphOnly(dependencies);

            filename = Path.ChangeExtension(filename, ".dot");

            await DependencyGraphGenerator.CompileGraphAndWriteToFile(filename, outputPath, graph);
        }


        public static void ExportMetrics(string baseFilename, string outputPath, AnalysisResult analyzeMetrics, string solutionPath,CoreUtils.ExportFormat format)
        {
            PrepareDirectoryForOutput(outputPath);
            string TOTAL = "Total";
            var filePath = CoreUtils.GetExportFileNameWithExtension(baseFilename, format);

            try
            {
                string? currentProject = null;
                
                List<NamespaceMetrics> namespaceMetrics = [];
                var duplicationCounts = GetDuplicationCounts(analyzeMetrics.DuplicationMap);

                foreach (var metric in analyzeMetrics.Metrics)
                {
                    if (!string.Equals(currentProject, metric.ProjectName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentProject != null)
                        {
                            namespaceMetrics.Add(new NamespaceMetrics
                            {
                                ProjectName = currentProject,
                                ProjectPath = TOTAL,
                                LineCount = analyzeMetrics.ProjectTotals[currentProject]
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
                        ProjectPath = TOTAL,
                        LineCount = analyzeMetrics.ProjectTotals[currentProject]
                    });
                    namespaceMetrics.Add(new NamespaceMetrics
                    {
                        ProjectName = TOTAL,
                        ProjectPath = "",
                        LineCount = analyzeMetrics.TotalLines
                    });
                }

                ExportCollection(filePath, outputPath, namespaceMetrics, format);
            }
            catch (IOException ex)
            {
                throw new IOException($"Failed to export metrics to {outputPath}/{filePath}", ex);
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
        private static void PrepareDirectoryForOutput(string outputPath)
        {
            string? directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }

    public interface IExportStrategy
    {
        void Export<T>(string filePath, string outputPath, IEnumerable<T> data) where T : class;
    }

    public class CsvExportStrategy : IExportStrategy
    {
        public void Export<T>(string filePath, string outputPath, IEnumerable<T> data) where T : class
        {
            CsvHandler.Serialize(data, Path.Combine(outputPath, filePath));
        }
    }

    public class JsonExportStrategy : IExportStrategy
    {
        public void Export<T>(string filePath,string outputPath, IEnumerable<T> data) where T : class
        {
            JsonHandler.Serialize(data, Path.Combine(outputPath, filePath));
        }
    }
}