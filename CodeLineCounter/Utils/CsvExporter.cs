using System.IO;
using System.Collections.Generic;
using CodeLineCounter.Models;

namespace CodeLineCounter.Utils
{
    public static class CsvExporter
    {
        public static void ExportToCsv(string filePath, List<NamespaceMetrics> metrics, Dictionary<string, int> projectTotals, int totalLines, List<DuplicationCode> duplications, string? solutionPath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Project,ProjectPath,Namespace,FileName,FilePath,LineCount,CyclomaticComplexity,CodeDuplications");

                string? currentProject = null;
                var duplicationCounts = GetDuplicationCounts(duplications);

                foreach (var metric in metrics)
                {
                    if (currentProject != metric.ProjectName)
                    {
                        AppendProjectLineToCsv(projectTotals, writer, currentProject);
                        currentProject = metric.ProjectName;
                    }
                    int fileDuplicationCount = GetFileDuplicationsCount(duplicationCounts, metric, solutionPath);

                    writer.WriteLine($"{metric.ProjectName},{metric.ProjectPath},{metric.NamespaceName},{metric.FileName},{metric.FilePath},{metric.LineCount},{metric.CyclomaticComplexity},{fileDuplicationCount}");
                }

                AppendProjectLineToCsv(projectTotals, writer, currentProject);

                writer.WriteLine($"Total,,,,,{totalLines},");
            }
        }

        public static void AppendProjectLineToCsv(Dictionary<string, int> projectTotals, StreamWriter writer, string? currentProject)
        {
            if (currentProject != null)
            {
                writer.WriteLine($"{currentProject},Total,,,,{projectTotals[currentProject]},,");
            }
        }

        public static void ExportCodeDuplicationsToCsv(string filePath, List<DuplicationCode> duplications, string? solutionPath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Code Hash,FilePath,MethodName,StartLine,NbLines");

                foreach (var detail in duplications)
                {
                    writer.WriteLine($"{detail.CodeHash},{detail.FilePath},{detail.MethodName},{detail.StartLine},{detail.NbLines}");
                }
            }
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
                var normalizedPath = solutionPath != string.Empty ? Path.GetFullPath(metric.FilePath, solutionPath) : Path.GetFullPath(metric.FilePath);
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

