using CsvHelper.Configuration.Attributes;

namespace CodeLineCounter.Models
{
    public class DuplicationCode
    {
        [Name("Code Hash")]
        public string? CodeHash { get; set; }
        [Name("FilePath")]
        public required string FilePath { get; set; }
        [Name("MethodName")]
        public string? MethodName { get; set; }
        [Name("StartLine")]
        public int StartLine { get; set; }
        [Name("NbLines")]
        public int NbLines { get; set; }  
    }
}

