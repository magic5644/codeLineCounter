using System.Text;
using CodeLineCounter.Models;

namespace CodeLineCounter.Utils
{
    public static class CsvExporter
    {
        public static void ExportToCsv(string filePath, List<NamespaceMetrics> metrics, Dictionary<string, int> projectTotals, int totalLines, Dictionary<string, List<(string filePath, string methodName, int startLine)>> duplicationMap)
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

                var fileDuplicationCount = duplicationCounts.ContainsKey(metric.FilePath) ? duplicationCounts[metric.FilePath] : 0;

                csvBuilder.AppendLine($"{metric.ProjectName},{metric.ProjectPath},{metric.NamespaceName},{metric.FileName},{metric.FilePath},{metric.LineCount},{metric.CyclomaticComplexity},{fileDuplicationCount}");
            }

            if (currentProject != null)
            {
                csvBuilder.AppendLine($"{currentProject},Total,,,,{projectTotals[currentProject]},,");
            }

            csvBuilder.AppendLine($"Total,,,,,{totalLines},");

            File.WriteAllText(filePath, csvBuilder.ToString());
        }

        public static void ExportCodeDuplicationsToCsv(string filePath, Dictionary<string, List<(string filePath, string methodName, int startLine)>> duplicationMap)
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

        private static Dictionary<string, int> GetDuplicationCounts(Dictionary<string, List<(string filePath, string methodName, int startLine)>> duplicationMap)
        {
            var duplicationCounts = new Dictionary<string, int>();

            foreach (var entry in duplicationMap)
            {
                foreach (var detail in entry.Value)
                {
                    if (duplicationCounts.ContainsKey(detail.filePath))
                    {
                        duplicationCounts[detail.filePath]++;
                    }
                    else
                    {
                        duplicationCounts[detail.filePath] = 1;
                    }
                }
            }

            return duplicationCounts;
        }
    }
}
