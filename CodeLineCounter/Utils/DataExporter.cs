using System.IO;
using System.Collections.Generic;
using CodeLineCounter.Utils;
using CodeLineCounter.Models;

namespace CodeLineCounter.Utils
{

    public static class DataExporter
    {
        public static void Export<T>(string filePath, T data, CoreUtils.ExportFormat format) where T : class
        {
            filePath = CoreUtils.GetExportFileNameWithExtension(filePath, format);
            switch (format)
            {
                case CoreUtils.ExportFormat.CSV:
                    CsvHandler.Serialize(new List<T> { data }, filePath);
                    break;
                case CoreUtils.ExportFormat.JSON:
                    JsonHandler.Serialize(new List<T> { data }, filePath);
                    break;
            }
        }

        public static void ExportCollection<T>(string filePath, IEnumerable<T> data, CoreUtils.ExportFormat format) where T : class
        {
            filePath = CoreUtils.GetExportFileNameWithExtension(filePath, format);
            switch (format)
            {
                case CoreUtils.ExportFormat.CSV:
                    CsvHandler.Serialize(data, filePath);
                    break;
                case CoreUtils.ExportFormat.JSON:
                    JsonHandler.Serialize(data, filePath);
                    break;
            }
        }

        public static void ExportDuplications(string filePath, List<DuplicationCode> duplications, CoreUtils.ExportFormat format)
        {
            ExportCollection(filePath, duplications, format);
        }

        public static void ExportMetrics(string filePath, List<NamespaceMetrics> metrics, 
            Dictionary<string, int> projectTotals, int totalLines, 
            List<DuplicationCode> duplications, string? solutionPath, CoreUtils.ExportFormat format)
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

        private static Dictionary<string, int> GetDuplicationCounts(List<DuplicationCode> duplications)
        {
            var duplicationCounts = new Dictionary<string, int>();
            foreach (var duplication in duplications)
            {
                var normalizedPath = Path.GetFullPath(duplication.FilePath);
                if (duplicationCounts.TryGetValue(normalizedPath, out int count))
                {
                    duplicationCounts[normalizedPath] = count + 1;
                }
                else
                {
                    duplicationCounts[normalizedPath] = 1;
                }
            }
            return duplicationCounts;
        }

        private static int GetFileDuplicationsCount(Dictionary<string, int> duplicationCounts, 
            NamespaceMetrics metric, string? solutionPath)
        {
            if (metric.FilePath == null) return 0;
            
            solutionPath = solutionPath == null ? 
                Path.GetFullPath(".") : 
                Path.GetDirectoryName(solutionPath);

            if (solutionPath == null) return 0;

            var normalizedPath = !string.IsNullOrEmpty(solutionPath) ? 
                Path.GetFullPath(metric.FilePath, solutionPath) : 
                Path.GetFullPath(metric.FilePath);

            return duplicationCounts.TryGetValue(normalizedPath, out int count) ? count : 0;
        }
    }
}