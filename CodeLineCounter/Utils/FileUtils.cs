using System.Text.RegularExpressions; // Ajoutez cette ligne
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                // Rechercher les lignes contenant les projets (Project("...") = "...", "...", "...")
                var match = Regex.Match(line, @"Project\(""{.*}""\) = "".*"", ""(.*\.csproj)""");
                if (match.Success)
                {
                    var relativePath = match.Groups[1].Value;
                    var projectPath = Path.Combine(Path.GetDirectoryName(solutionFilePath), relativePath);
                    projectFiles.Add(projectPath);
                }
            }

            return projectFiles;
        }
    }
}
