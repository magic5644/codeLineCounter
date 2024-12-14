using CsvHelper.Configuration.Attributes;
namespace CodeLineCounter.Models
{
    public class DependencyRelation
    {
        [Name("SourceClass")]
        public required string SourceClass { get; set; }

        [Name("TargetClass")]
        public required string TargetClass { get; set; }

        [Name("FilePath")]
        public required string FilePath { get; set; }

        [Name("StartLine")]
        public int StartLine { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not DependencyRelation other)
                return false;

            return SourceClass == other.SourceClass &&
                   TargetClass == other.TargetClass &&
                   FilePath == other.FilePath &&
                   StartLine == other.StartLine;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SourceClass, TargetClass, FilePath, StartLine);
        }
    }
}
