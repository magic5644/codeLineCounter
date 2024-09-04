using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CodeLineCounter.Models;

namespace CodeLineCounter.Utils
{
    public static class CsvExporter
    {
        public static void ExportToCsv(string filePath, List<NamespaceMetrics> metrics, Dictionary<string, int> projectTotals, int totalLines, List<DuplicationCode> duplications, string? solutionPath)
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Project,ProjectPath,Namespace,FileName,FilePath,LineCount,CyclomaticComplexity,CodeDuplications");

            string? currentProject = null;
            var duplicationCounts = GetDuplicationCounts(duplications);

            foreach (var metric in metrics)
            {
                if (currentProject != metric.ProjectName)
                {
                    AppendProjectLineToCsv(projectTotals, csvBuilder, currentProject);
                    currentProject = metric.ProjectName;
                }
                int fileDuplicationCount = GetFileDuplicationsCount(duplicationCounts, metric, solutionPath);

                csvBuilder.AppendLine($"{metric.ProjectName},{metric.ProjectPath},{metric.NamespaceName},{metric.FileName},{metric.FilePath},{metric.LineCount},{metric.CyclomaticComplexity},{fileDuplicationCount}");
            }

            AppendProjectLineToCsv(projectTotals, csvBuilder, currentProject);

            csvBuilder.AppendLine($"Total,,,,,{totalLines},");

            File.WriteAllText(filePath, csvBuilder.ToString());

            csvBuilder.Clear();
        }

        public static void AppendProjectLineToCsv(Dictionary<string, int> projectTotals, StringBuilder csvBuilder, string? currentProject)
        {
            if (currentProject != null)
            {
                csvBuilder.AppendLine($"{currentProject},Total,,,,{projectTotals[currentProject]},,");
            }
        }

        public static void ExportCodeDuplicationsToCsv(string filePath, List<DuplicationCode> duplications, string? solutionPath)
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Code Hash,FilePath,MethodName,StartLine,NbLines");

            foreach (var detail in duplications)
            {
                csvBuilder.AppendLine($"{detail.CodeHash},{detail.FilePath},{detail.MethodName},{detail.StartLine},{detail.NbLines}");
            }

            File.WriteAllText(filePath, csvBuilder.ToString());

            csvBuilder.Clear();
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

