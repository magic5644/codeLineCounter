using CsvHelper.Configuration.Attributes;

namespace CodeLineCounter.Models
{
    public class NamespaceMetrics
    {
        [Name("ProjectName")]
        public string? ProjectName { get; set; }
        [Name("ProjectPath")]
        public string? ProjectPath { get; set; }
        [Name("NamespaceName")]
        public string? NamespaceName { get; set; }
        [Name("FileName")]
        public string? FileName { get; set; }
        [Name("FilePath")]
        public string? FilePath { get; set; }
        [Name("LineCount")]
        public int LineCount { get; set; }
        [Name("CyclomaticComplexity")]
        public int CyclomaticComplexity { get; set; }  // Addition of cyclomatic complexity
        [Name("CodeDuplications")]
        public int CodeDuplications { get; set; }
    }
}
