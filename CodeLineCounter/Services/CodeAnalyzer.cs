using CodeLineCounter.Models;
using CodeLineCounter.Utils;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeLineCounter.Services
{
    public class CodeAnalyzer
    {
        public (List<NamespaceMetrics>, Dictionary<string, int>, int) AnalyzeSolution(string solutionFilePath)
        {
            string solutionDirectory = Path.GetDirectoryName(solutionFilePath) ?? string.Empty;
            var projectFiles = FileUtils.GetProjectFiles(solutionFilePath);

            var namespaceMetrics = new List<NamespaceMetrics>();
            var projectTotals = new Dictionary<string, int>();
            int totalLines = 0;

            foreach (var projectFile in projectFiles)
            {
                string? projectDirectory = Path.GetDirectoryName(projectFile);
                string? projectName = Path.GetFileNameWithoutExtension(projectFile);
                string relativeProjectPath = solutionDirectory != null ? Path.GetRelativePath(solutionDirectory, projectFile) : projectFile;
                
                var files = projectDirectory != null ? FileUtils.GetAllCsFiles(projectDirectory) : new List<string>();

                int projectLineCount = 0;
                var projectNamespaceMetrics = new Dictionary<string, int>();

                foreach (var file in files)
                {
                    var lines = File.ReadAllLines(file);
                    string? currentNamespace = null;
                    int fileLineCount = 0;
                    int fileCyclomaticComplexity = 0;

                    // Analyser le fichier pour obtenir la complexité cyclomatique
                    var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
                    var compilation = CSharpCompilation.Create("CodeAnalysis", new[] { tree });
                    var model = compilation.GetSemanticModel(tree);

                    var complexityCalculator = new CyclomaticComplexityCalculator();
                    fileCyclomaticComplexity = complexityCalculator.Calculate(tree.GetRoot(), model);

                    foreach (var line in lines)
                    {
                        if (line.Trim().StartsWith("namespace"))
                        {
                            var parts = line.Trim().Split(' ');
                            if (parts.Length >= 2)
                            {
                                currentNamespace = parts[1];
                                projectNamespaceMetrics.TryAdd(currentNamespace, 0);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith("//"))
                        {
                            fileLineCount++;
                            if (currentNamespace != null)
                            {
                                projectNamespaceMetrics[currentNamespace]++;
                            }
                        }
                    }

                    // Ajouter les métriques par fichier
                    if (currentNamespace != null)
                    {
                        namespaceMetrics.Add(new NamespaceMetrics
                        {
                            ProjectName = projectName,
                            ProjectPath = relativeProjectPath,
                            NamespaceName = currentNamespace,
                            FileName = Path.GetFileName(file),
                            FilePath = solutionDirectory != null ? Path.GetRelativePath(solutionDirectory, file) :file,
                            LineCount = fileLineCount,
                            CyclomaticComplexity = fileCyclomaticComplexity
                        });
                    }
                    else
                    {
                        // Fichier sans namespace
                        namespaceMetrics.Add(new NamespaceMetrics
                        {
                            ProjectName = projectName,
                            ProjectPath = relativeProjectPath,
                            NamespaceName = "No Namespace",
                            FileName = Path.GetFileName(file),
                            FilePath = solutionDirectory != null ? Path.GetRelativePath(solutionDirectory, file) : file,
                            LineCount = fileLineCount,
                            CyclomaticComplexity = fileCyclomaticComplexity
                        });
                    }
                    projectLineCount += fileLineCount;
                }

                // Ajouter les sous-totaux par namespace dans le projet
                foreach (var kvp in projectNamespaceMetrics)
                {
                    namespaceMetrics.Add(new NamespaceMetrics
                    {
                        ProjectName = projectName,
                        ProjectPath = relativeProjectPath,
                        NamespaceName = kvp.Key,
                        FileName = "Total",
                        FilePath = projectDirectory,
                        LineCount = kvp.Value,
                        CyclomaticComplexity = 0  // Les sous-totaux ne nécessitent pas la complexité cyclomatique
                    });
                }

                // Ajouter le total du projet
                projectTotals[projectName] = projectLineCount;
                totalLines += projectLineCount;
            }

            return (namespaceMetrics, projectTotals, totalLines);
        }
    }
}
