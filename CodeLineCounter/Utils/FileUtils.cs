using System.Text.RegularExpressions; // Add this line

namespace CodeLineCounter.Utils
{
    public static class FileUtils
    {
        public static List<string> GetAllCsFiles(string rootPath)
        {
            return Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories)
                            .Where(f => !f.Contains(@"\obj\"))
                            .ToList();
        }

        public static List<string> GetSolutionFiles(string rootPath)
        {
            return Directory.GetFiles(rootPath, "*.sln", SearchOption.TopDirectoryOnly).ToList();
        }

        public static List<string> GetProjectFiles(string solutionFilePath)
        {
            var projectFiles = new List<string>();
            var lines = File.ReadAllLines(solutionFilePath);

            foreach (var line in lines)
            {
                // Search for lines containing projects (Project("...") = "...", "...", "...")
                var match = Regex.Match(line, @"Project\(""{.*}""\) = "".*"", ""(.*\.csproj)""");
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
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        }
    }
}
