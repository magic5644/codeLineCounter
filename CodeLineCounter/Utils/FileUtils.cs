using System.Text.RegularExpressions;

namespace CodeLineCounter.Utils
{
    public static partial class FileUtils
    {
        public static List<string> GetAllCsFiles(string rootPath)
        {
            return Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories)
                            .Where(f => !f.Contains(@"\obj\"))
                            .ToList();
        }

        public static List<string> GetSolutionFiles(string rootPath)
        {
            if (!Directory.Exists(rootPath))
            {
                throw new UnauthorizedAccessException($"Access to the path '{rootPath}' is denied.");
            }

            return Directory.GetFiles(rootPath, "*.sln", SearchOption.TopDirectoryOnly).ToList();
        }

        public static List<string> GetProjectFiles(string solutionFilePath)
        {
            if (!File.Exists(solutionFilePath))
            {
                throw new UnauthorizedAccessException($"Access to the path '{solutionFilePath}' is denied.");
            }

            var projectFiles = new List<string>();
            var lines = File.ReadAllLines(solutionFilePath);

            foreach (var line in lines)
            {
                // Search for lines containing projects (Project("...") = "...", "...", "...")
                var match = MyRegex().Match(line);
                if (match.Success)
                {
                    var relativePath = match.Groups[1].Value;
                    var projectPath = Path.Combine(Path.GetDirectoryName(solutionFilePath) ?? string.Empty, relativePath);
                    projectFiles.Add(projectPath);
                }
            }

            return projectFiles;
        }

        public static string GetBasePath()
        {
            // Arrange
            return Path.GetDirectoryName(AppContext.BaseDirectory) ?? string.Empty;
        }

        [GeneratedRegex(@"Project\(""{.*}""\) = "".*"", ""(.*\.csproj)""")]
        private static partial Regex MyRegex();
    }
}
