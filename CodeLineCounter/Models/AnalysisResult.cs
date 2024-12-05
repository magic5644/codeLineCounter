using CodeLineCounter.Models;

namespace CodeLineCounter.Models
{
        public class AnalysisResult
        {
            public required List<NamespaceMetrics> Metrics { get; set; }
            public required Dictionary<string, int> ProjectTotals { get; set; }
            public int TotalLines { get; set; }
            public int TotalFiles { get; set; }
            public required List<DuplicationCode> DuplicationMap { get; set; }
            public TimeSpan ProcessingTime { get; set; }
            public required string SolutionFileName { get; set; }
            public int DuplicatedLines { get; set; }
        }

}