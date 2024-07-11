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
        private static readonly object exportLock = new object();

        public static void ExportToCsv(string filePath, List<NamespaceMetrics> metrics, Dictionary<string, int> projectTotals, int totalLines, Dictionary<string, List<(string filePath, string methodName, int startLine)>> duplicationMap, string? solutionPath)
        {
            lock (exportLock)
            {
                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("Project,ProjectPath,Namespace,FileName,FilePath,LineCount,CyclomaticComplexity,CodeDuplications");

                string? currentProject = null;
                var duplicationCounts = GetDuplicationCounts(duplicationMap);


                foreach (var metric in metrics)
                {
                    if (currentProject != metric.ProjectName)
                    {
                        if (currentProject != null)
                        {
                            csvBuilder.AppendLine($"{currentProject},Total,,,,{projectTotals[currentProject]},,");
                        }
                        currentProject = metric.ProjectName;
                    }
                    int fileDuplicationCount = GetFileDuplicationsCount(duplicationCounts, metric, solutionPath);

                    csvBuilder.AppendLine($"{metric.ProjectName},{metric.ProjectPath},{metric.NamespaceName},{metric.FileName},{metric.FilePath},{metric.LineCount},{metric.CyclomaticComplexity},{fileDuplicationCount}");
                }

                if (currentProject != null)
                {
                    csvBuilder.AppendLine($"{currentProject},Total,,,,{projectTotals[currentProject]},,");
                }

                csvBuilder.AppendLine($"Total,,,,,{totalLines},");

                File.WriteAllText(filePath, csvBuilder.ToString());
            }
        }

        public static void ExportCodeDuplicationsToCsv(string filePath, Dictionary<string, List<(string filePath, string methodName, int startLine)>> duplicationMap, string? solutionPath)
        {
            lock (exportLock)
            {
                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("Code Hash,FilePath,MethodName,StartLine");

                foreach (var entry in duplicationMap)
                {
                    foreach (var detail in entry.Value)
                    {
                        csvBuilder.AppendLine($"{entry.Key},{detail.filePath},{detail.methodName},{detail.startLine}");
                    }
                }

                File.WriteAllText(filePath, csvBuilder.ToString());
            }
        }

        public static Dictionary<string, int> GetDuplicationCounts(Dictionary<string, List<(string filePath, string methodName, int startLine)>> duplicationMap)
        {
            var duplicationCounts = new Dictionary<string, int>();

            foreach (var entry in duplicationMap)
            {
                foreach (var detail in entry.Value)
                {
                    var normalizedPath = Path.GetFullPath(detail.filePath);
                    if (duplicationCounts.ContainsKey(normalizedPath))
                    {
                        duplicationCounts[normalizedPath]++;
                    }
                    else
                    {
                        duplicationCounts[normalizedPath] = 1;
                    }
                }
            }

            return duplicationCounts;
        }

        public static int GetFileDuplicationsCount(Dictionary<string, int> duplicationCounts, NamespaceMetrics metric, string? solutionPath)
        {
            int count = 0;
            if (solutionPath == null)  {
               solutionPath =  Path.GetFullPath(".");
            }
            else
            {
                solutionPath = Path.GetDirectoryName(solutionPath);
            }
            if (metric.FilePath !=null && solutionPath != null) 
            {
                var normalizedPath = solutionPath != string.Empty ? Path.GetFullPath(metric.FilePath, solutionPath) : Path.GetFullPath(metric.FilePath);
                count = duplicationCounts.ContainsKey(normalizedPath) ? duplicationCounts[normalizedPath] : 0;
                Console.WriteLine($"filepath : {normalizedPath}, count : {count}");
            }
            
            return count;
        }
    }
}

