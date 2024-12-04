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

    public class DuplicationInfo
    {
        [Name("Source File")]
        public required string SourceFile { get; set; }
        [Name("Start Line")]
        public int StartLine { get; set; }
        [Name("Nb Lines")]
        public int NbLines { get; set; }
        [Name("Duplicated Code")]
        public required string DuplicatedCode { get; set; }
        [Name("Duplicated In")]
        public required List<DuplicationLocation> Duplicates { get; set; }
    }

    public class DuplicationLocation
    {
        [Name("File Path")]
        public required string FilePath { get; set; }
        [Name("Start Line")]
        public int StartLine { get; set; }
    }
}

