using CodeLineCounter.Models;
using CodeLineCounter.Utils;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeLineCounter.Services
{
    public class CodeAnalyzer
    {
        public (List<NamespaceMetrics>, Dictionary<string, int>, int, int) AnalyzeSolution(string solutionFilePath)
        {
            string solutionDirectory = Path.GetDirectoryName(solutionFilePath) ?? string.Empty;
            var projectFiles = FileUtils.GetProjectFiles(solutionFilePath);

            var namespaceMetrics = new List<NamespaceMetrics>();
            var projectTotals = new Dictionary<string, int>();
            int totalLines = 0;
            int totalFilesanalyzed = 0;

            foreach (var projectFile in projectFiles)
            {
                string? projectDirectory = Path.GetDirectoryName(projectFile);
                string projectName = Path.GetFileNameWithoutExtension(projectFile);
                string relativeProjectPath = solutionDirectory != null ? Path.GetRelativePath(solutionDirectory, projectFile) : projectFile;
                
                var files = projectDirectory != null ? FileUtils.GetAllCsFiles(projectDirectory) : [];

                int projectLineCount = 0;
                var projectNamespaceMetrics = new Dictionary<string, int>();

                foreach (var file in files)
                {
                    totalFilesanalyzed += 1;
                    projectLineCount = AnalyzeSourceFile(solutionDirectory, namespaceMetrics, projectName, relativeProjectPath, projectLineCount, projectNamespaceMetrics, file);

                }

                // Add subtotals by namespace in the project
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
                        CyclomaticComplexity = 0  // Subtotals do not require cyclomatic complexity
                    });
                }

                // Add project total
                projectTotals[projectName] = projectLineCount;
                totalLines += projectLineCount;
            }

            return (namespaceMetrics, projectTotals, totalLines, totalFilesanalyzed );
        }

        // Analyzes a source file to obtain metrics such as line count and cyclomatic complexity.
        public static int AnalyzeSourceFile(string? solutionDirectory, List<NamespaceMetrics> namespaceMetrics, string projectName, string relativeProjectPath, int projectLineCount, Dictionary<string, int> projectNamespaceMetrics, string file)
        {
            var lines = File.ReadAllLines(file);
            string? currentNamespace;
            int fileLineCount, fileCyclomaticComplexity;
            AnalyzeSourceCode(projectNamespaceMetrics, file, lines, out currentNamespace, out fileLineCount, out fileCyclomaticComplexity);

            // Add file metrics
            if (currentNamespace != null)
            {
                namespaceMetrics.Add(new NamespaceMetrics
                {
                    ProjectName = projectName,
                    ProjectPath = relativeProjectPath,
                    NamespaceName = currentNamespace,
                    FileName = Path.GetFileName(file),
                    FilePath = solutionDirectory != null ? Path.GetRelativePath(solutionDirectory, file) : file,
                    LineCount = fileLineCount,
                    CyclomaticComplexity = fileCyclomaticComplexity
                });
            }
            else
            {
                // File without namespace
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
            return projectLineCount;
        }

        /// <summary>
        /// Analyzes the source code of a file to obtain the cyclomatic complexity and line count for each namespace.
        /// Updates the provided <paramref name="projectNamespaceMetrics"/> dictionary with the line count for each namespace.
        /// </summary>
        /// <param name="projectNamespaceMetrics">The dictionary to store the line count for each namespace.</param>
        /// <param name="file">The path of the file to analyze.</param>
        /// <param name="lines">The lines of the file to analyze.</param>
        /// <param name="currentNamespace">Output parameter indicating the current namespace being analyzed.</param>
        /// <param name="fileLineCount">Output parameter indicating the total number of lines in the file.</param>
        /// <param name="fileCyclomaticComplexity">Output parameter indicating the cyclomatic complexity of the file.</param>
        public static void AnalyzeSourceCode(Dictionary<string, int> projectNamespaceMetrics, string file, string[] lines, out string? currentNamespace, out int fileLineCount, out int fileCyclomaticComplexity)
        {
            currentNamespace = null;
            fileLineCount = 0;
            fileCyclomaticComplexity = 0;

            // Analyze the file to obtain cyclomatic complexity
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
        }
    }
}
