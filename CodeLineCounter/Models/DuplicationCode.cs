namespace CodeLineCounter.Models
{
    public class DuplicationCode
    {
        public string? CodeHash { get; set; }
        public required string FilePath { get; set; }
        public string? MethodName { get; set; }
        public int StartLine { get; set; }
        public int NbLines { get; set; }  
    }
}

