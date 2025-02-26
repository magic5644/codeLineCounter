using System.Text.RegularExpressions;

namespace CodeLineCounter.Utils
{
    public static partial class FileUtils
    {
        // Default file extensions
        private const string DEFAULT_CODE_EXTENSION = "*.cs";
        private const string DEFAULT_SOLUTION_EXTENSION = "*.sln";
        private const string DEFAULT_PROJECT_EXTENSION = ".csproj";

        public static List<string> GetAllCodeFiles(string rootPath, string fileExtension = DEFAULT_CODE_EXTENSION)
        {
            var excludeFolders = new[] { @"\obj\", @"\bin\", @"\.*" };
            return Directory.GetFiles(rootPath, fileExtension, SearchOption.AllDirectories)
                           .Where(f => !excludeFolders.Any(ef => f.Contains(ef)))
                           .ToList();
        }

        // For backward compatibility
        public static List<string> GetAllCsFiles(string rootPath) => GetAllCodeFiles(rootPath);

        public static List<string> GetSolutionFiles(string rootPath, string fileExtension = DEFAULT_SOLUTION_EXTENSION)
        {
            if (!Directory.Exists(rootPath))
            {
                throw new DirectoryNotFoundException($"Directory '{rootPath}' not found.");
            }

            return Directory.GetFiles(rootPath, fileExtension, SearchOption.TopDirectoryOnly).ToList();
        }

        public static List<string> GetProjectFiles(string solutionFilePath, string projectExtension = DEFAULT_PROJECT_EXTENSION)
        {
            if (!File.Exists(solutionFilePath))
            {
                throw new FileNotFoundException($"File '{solutionFilePath}' not found.");
            }

            var projectFiles = new List<string>();
            var lines = File.ReadAllLines(solutionFilePath);

            foreach (var line in lines)
            {
                // Search for lines containing projects with the specified extension
                var match = GenerateProjectRegex(projectExtension).Match(line);
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
            return Path.GetDirectoryName(AppContext.BaseDirectory) ?? string.Empty;
        }

        private static Regex GenerateProjectRegex(string projectExtension)
        {
            return new Regex($@"Project\(""{{.*}}""\) = "".*"", ""(.*{Regex.Escape(projectExtension)})""");
        }

        [GeneratedRegex(@"Project\(""{.*}""\) = "".*"", ""(.*\.csproj)""")]
        private static partial Regex MyRegex();
    }
}