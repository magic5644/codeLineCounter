namespace CodeLineCounter.Models
{
    public class NamespaceMetrics
    {
        public string? ProjectName { get; set; }
        public string? ProjectPath { get; set; }
        public string? NamespaceName { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public int LineCount { get; set; }
        public int CyclomaticComplexity { get; set; }  // Addition of cyclomatic complexity
    }
}
