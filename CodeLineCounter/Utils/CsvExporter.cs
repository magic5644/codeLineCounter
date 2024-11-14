using System.IO;
using System.Collections.Generic;
using CodeLineCounter.Utils;
using CodeLineCounter.Models;

namespace CodeLineCounter.Utils
{
    public static class CsvExporter
    {
        public static void ExportToCsv(string filePath, List<NamespaceMetrics> metrics, Dictionary<string, int> projectTotals, int totalLines, List<DuplicationCode> duplications, string? solutionPath)
        {
            string? currentProject = null;
            List<NamespaceMetrics> namespaceMetrics = [];
            var duplicationCounts = GetDuplicationCounts(duplications);

            foreach (var metric in metrics)
            {
                if (!string.Equals(currentProject, metric.ProjectName, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (currentProject != null)
                    {
                        var total = new NamespaceMetrics
                        {
                            ProjectName = currentProject,
                            ProjectPath = "Total",
                            LineCount = projectTotals[currentProject]
                        };
                        namespaceMetrics.Add(total);
                    }
                    currentProject = metric.ProjectName;
                }
                int fileDuplicationCount = GetFileDuplicationsCount(duplicationCounts, metric, solutionPath);
                metric.CodeDuplications = fileDuplicationCount;
                namespaceMetrics.Add(metric);

            }
            if (currentProject != null)
            {
                var total = new NamespaceMetrics
                {
                    ProjectName = currentProject,
                    ProjectPath = "Total",
                    LineCount = projectTotals[currentProject]
                };
                namespaceMetrics.Add(total);
                var totalGeneral = new NamespaceMetrics
                {
                    ProjectName = "Total",
                    ProjectPath = "",
                    LineCount = totalLines
                };
                namespaceMetrics.Add(totalGeneral);

            }
            CsvHandler.Serialize(namespaceMetrics, filePath);
        }


        public static void ExportCodeDuplicationsToCsv(string filePath, List<DuplicationCode> duplications)
        {
            CsvHandler.Serialize(duplications, filePath);
        }

        public static Dictionary<string, int> GetDuplicationCounts(List<DuplicationCode> duplications)
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

        public static int GetFileDuplicationsCount(Dictionary<string, int> duplicationCounts, NamespaceMetrics metric, string? solutionPath)
        {
            int count = 0;
            if (solutionPath == null)
            {
                solutionPath = Path.GetFullPath(".");
            }
            else
            {
                solutionPath = Path.GetDirectoryName(solutionPath);
            }
            if (metric.FilePath != null && solutionPath != null)
            {
                var normalizedPath = !string.Equals(solutionPath, string.Empty, System.StringComparison.OrdinalIgnoreCase) ? Path.GetFullPath(metric.FilePath, solutionPath) : Path.GetFullPath(metric.FilePath);
                if (duplicationCounts.TryGetValue(normalizedPath, out int countValue))
                {
                    count = countValue;
                }
                else
                {
                    count = 0;
                }
            }

            return count;
        }
    }
}

