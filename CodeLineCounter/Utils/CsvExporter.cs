using System.Text;
using CodeLineCounter.Models;

namespace CodeLineCounter.Utils
{
    public static class CsvExporter
    {
        public static void ExportToCsv(string filePath, List<NamespaceMetrics> metrics, Dictionary<string, int> projectTotals, int totalLines)
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Project,ProjectPath,Namespace,FileName,FilePath,LineCount,CyclomaticComplexity");

            string? currentProject = null;
            foreach (var metric in metrics)
            {
                if (currentProject != metric.ProjectName)
                {
                    if (currentProject != null)
                    {
                        csvBuilder.AppendLine($"{currentProject},,,,,,Total,{projectTotals[currentProject]},");
                    }
                    currentProject = metric.ProjectName;
                }
                csvBuilder.AppendLine($"{metric.ProjectName},{metric.ProjectPath},{metric.NamespaceName},{metric.FileName},{metric.FilePath},{metric.LineCount},{metric.CyclomaticComplexity}");
            }

            if (currentProject != null)
            {
                csvBuilder.AppendLine($"{currentProject},,,,,Total,{projectTotals[currentProject]},");
            }

            csvBuilder.AppendLine($",,,,,Total,{totalLines},");

            File.WriteAllText(filePath, csvBuilder.ToString());
        }
    }
}
