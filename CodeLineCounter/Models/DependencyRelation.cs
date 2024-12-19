using CsvHelper.Configuration.Attributes;

namespace CodeLineCounter.Models
{
    public class DependencyRelation
    {
        [Name("SourceClass")]
        public required string SourceClass { get; set; }

        [Name("SourceNamespace")]
        public required string SourceNamespace { get; set; }

        [Name("SourceAssembly")]
        public required string SourceAssembly { get; set; }

        [Name("TargetClass")]
        public required string TargetClass { get; set; }

        [Name("TargetNamespace")]
        public required string TargetNamespace { get; set; }

        [Name("TargetAssembly")]
        public required string TargetAssembly { get; set; }

        [Name("FilePath")]
        public required string FilePath { get; set; }

        [Name("StartLine")]
        public int StartLine { get; set; }

        [Name("IncomingDegree")]
        public int IncomingDegree { get; set; }

        [Name("OutgoingDegree")]
        public int OutgoingDegree { get; set; }


        public override bool Equals(object? obj)
        {
            if (obj is not DependencyRelation other)
                return false;

            return SourceClass == other.SourceClass &&
                   SourceNamespace == other.SourceNamespace &&
                   SourceAssembly == other.SourceAssembly &&
                   TargetClass == other.TargetClass &&
                   TargetNamespace == other.TargetNamespace &&
                   TargetAssembly == other.TargetAssembly &&
                   FilePath == other.FilePath &&
                   StartLine == other.StartLine;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                SourceClass,
                SourceNamespace,
                SourceAssembly,
                TargetClass,
                TargetNamespace,
                TargetAssembly,
                FilePath,
                StartLine);
        }
    }
}