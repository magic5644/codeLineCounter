using CodeLineCounter.Models;
using CodeLineCounter.Utils;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CodeLineCounter.Services
{
    public static class CodeAnalyzer
    {
        public static (List<NamespaceMetrics>, Dictionary<string, int>, int, int, List<DuplicationCode>) AnalyzeSolution(string solutionFilePath)
        {
            string solutionDirectory = Path.GetDirectoryName(solutionFilePath) ?? string.Empty;
            var projectFiles = FileUtils.GetProjectFiles(solutionFilePath);

            var namespaceMetrics = new List<NamespaceMetrics>();
            var projectTotals = new Dictionary<string, int>();
            var codeDuplicationChecker = new CodeDuplicationChecker();
            int totalLines = 0;
            int totalFilesAnalyzed = 0;

            foreach (var projectFile in projectFiles)
            {
                AnalyzeProject(solutionDirectory, projectFile, ref totalFilesAnalyzed, ref totalLines, namespaceMetrics, projectTotals);
            }

            codeDuplicationChecker.DetectCodeDuplicationInFiles(FileUtils.GetAllCsFiles(solutionDirectory));

            var duplicationMap = codeDuplicationChecker.GetCodeDuplicationMap();
            var duplicationList = duplicationMap.Values.SelectMany(v => v).ToList();

            return (namespaceMetrics, projectTotals, totalLines, totalFilesAnalyzed, duplicationList);
        }

        private static void AnalyzeProject(string solutionDirectory, string projectFile, ref int totalFilesAnalyzed, ref int totalLines, List<NamespaceMetrics> namespaceMetrics, Dictionary<string, int> projectTotals)
        {
            string? projectDirectory = Path.GetDirectoryName(projectFile);
            string projectName = Path.GetFileNameWithoutExtension(projectFile);
            string relativeProjectPath = solutionDirectory != null ? Path.GetRelativePath(solutionDirectory, projectFile) : projectFile;

            var files = projectDirectory != null ? FileUtils.GetAllCsFiles(projectDirectory) : new List<string>();

            int projectLineCount = 0;
            var projectNamespaceMetrics = new Dictionary<string, int>();

            foreach (var file in files)
            {
                totalFilesAnalyzed++;
                projectLineCount = AnalyzeSourceFile(solutionDirectory, namespaceMetrics, projectName, relativeProjectPath, projectLineCount, projectNamespaceMetrics, file);
            }

            AddProjectMetrics(projectName, relativeProjectPath, projectDirectory, projectNamespaceMetrics, namespaceMetrics, projectTotals, projectLineCount);
            totalLines += projectLineCount;
        }

        private static void AddProjectMetrics(string projectName, string relativeProjectPath, string? projectDirectory, Dictionary<string, int> projectNamespaceMetrics, List<NamespaceMetrics> namespaceMetrics, Dictionary<string, int> projectTotals, int projectLineCount)
        {
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
                    CyclomaticComplexity = 0
                });
            }

            projectTotals[projectName] = projectLineCount;
        }

        public static int AnalyzeSourceFile(string? solutionDirectory, List<NamespaceMetrics> namespaceMetrics, string projectName, string relativeProjectPath, int projectLineCount, Dictionary<string, int> projectNamespaceMetrics, string file)
        {
            var lines = File.ReadAllLines(file);
            AnalyzeSourceCode(projectNamespaceMetrics, file, lines, out string? currentNamespace, out int fileLineCount, out int fileCyclomaticComplexity);

            namespaceMetrics.Add(new NamespaceMetrics
            {
                ProjectName = projectName,
                ProjectPath = relativeProjectPath,
                NamespaceName = currentNamespace ?? "No Namespace",
                FileName = Path.GetFileName(file),
                FilePath = solutionDirectory != null ? Path.GetRelativePath(solutionDirectory, file) : file,
                LineCount = fileLineCount,
                CyclomaticComplexity = fileCyclomaticComplexity
            });

            return projectLineCount + fileLineCount;
        }

        public static void AnalyzeSourceCode(Dictionary<string, int> projectNamespaceMetrics, string file, string[] lines, out string? currentNamespace, out int fileLineCount, out int fileCyclomaticComplexity)
        {
            currentNamespace = null;
            fileLineCount = 0;

            // lines variable should contain all source code lines
            string sourceCode = string.Join(Environment.NewLine, lines);
            var tree = CSharpSyntaxTree.ParseText(sourceCode);


            fileCyclomaticComplexity = CyclomaticComplexityCalculator.Calculate(tree.GetRoot());

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
                        projectNamespaceMetrics[currentNamespace] = fileLineCount;
                    }
                }
            }
        }
    }
}
