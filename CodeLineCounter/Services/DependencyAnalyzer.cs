using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeLineCounter.Models;
using CodeLineCounter.Utils;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace CodeLineCounter.Services
{
    public static class DependencyAnalyzer
    {
        private static readonly ConcurrentDictionary<string, HashSet<DependencyRelation>> _dependencyMap = new();
        private static readonly HashSet<string> _solutionClasses = new();
        private static readonly object _dependencyLock = new();

        public static void AnalyzeSolution(string solutionFilePath)
        {
            var projectFiles = FileUtils.GetProjectFiles(solutionFilePath);
            CollectAllClasses(projectFiles);

            AnalyzeProjects(projectFiles);
        }

        private static void CollectAllClasses(IEnumerable<string> projectFiles)
        {
            foreach (var projectFile in projectFiles)
            {
                string projectDirectory = Path.GetDirectoryName(projectFile) ?? string.Empty;
                var files = FileUtils.GetAllCsFiles(projectDirectory);

                foreach (var file in files)
                {
                    var sourceCode = File.ReadAllText(file);
                    var tree = CSharpSyntaxTree.ParseText(sourceCode);
                    var root = tree.GetRoot();

                    var classes = root.DescendantNodes()
                        .OfType<ClassDeclarationSyntax>()
                        .Select(c => GetFullTypeName(c));

                    lock (_dependencyLock)
                    {
                        foreach (var className in classes)
                        {
                            _solutionClasses.Add(className);
                        }
                    }
                }
            }
        }

        private static string GetFullTypeName(ClassDeclarationSyntax classDeclaration)
        {
            // First check the file-scoped namespace
            var fileScopedNamespace = classDeclaration.SyntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<FileScopedNamespaceDeclarationSyntax>()
                .FirstOrDefault();
            if (fileScopedNamespace != null)
            {
                return $"{fileScopedNamespace.Name}.{classDeclaration.Identifier.Text}";
            }

            // Then check the regular namespace
            var namespaceDeclaration = classDeclaration.Ancestors()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault();
            if (namespaceDeclaration != null)
            {
                return $"{namespaceDeclaration.Name}.{classDeclaration.Identifier.Text}";
            }

            return classDeclaration.Identifier.Text;
        }

        public static void AnalyzeProjects(IEnumerable<string> projectFiles)
        {
            foreach (var projectFile in projectFiles)
            {
                AnalyzeProject(projectFile);
            }
        }

        public static void AnalyzeProject(string projectPath)
        {
            string projectDirectory = Path.GetDirectoryName(projectPath) ?? string.Empty;
            var files = FileUtils.GetAllCsFiles(projectDirectory);

            foreach (var file in files)
            {
                var sourceCode = File.ReadAllLines(file);
                AnalyzeFile(file, string.Join(Environment.NewLine, sourceCode));
            }
        }

        public static void AnalyzeFile(string filePath, string sourceCode)
        {
            var tree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = tree.GetRoot();


            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            Parallel.ForEach(classes, classDeclaration =>
            {
                var className = GetFullTypeName(classDeclaration);
                var dependencies = ExtractDependencies(classDeclaration);

                foreach (var dependency in dependencies)
                {
                    var relation = new DependencyRelation
                    {
                        SourceClass = GetFullTypeName(classDeclaration),
                        TargetClass = dependency,
                        FilePath = filePath,
                        StartLine = classDeclaration.GetLocation().GetLineSpan().StartLinePosition.Line
                    };

                    _dependencyMap.AddOrUpdate(className,
                        new HashSet<DependencyRelation> { relation },
                        (key, set) =>
                        {
                            lock (_dependencyLock)
                            {
                                // Check if the relation already exists
                                if (!set.Any(r => r.Equals(relation)))
                                {
                                    set.Add(relation);
                                }
                            }
                            return set;
                        }
                    );
                }
            });
        }

        private static IEnumerable<string> ExtractDependencies(ClassDeclarationSyntax classDeclaration)
        {
            var dependencies = new HashSet<string>();
            var usings = GetUsingsWithCurrentNamespace(classDeclaration);
            

            AnalyzeInheritance(classDeclaration, usings, dependencies);
            AnalyzeClassMembers(classDeclaration, usings, dependencies);

            return dependencies;
        }

        private static List<string?> GetUsingsWithCurrentNamespace(ClassDeclarationSyntax classDeclaration)
        {
            var root = classDeclaration.SyntaxTree.GetRoot();
            var usings = (root as CompilationUnitSyntax)?.Usings
                .Select(u => u.Name?.ToString())
                .Where(u => u != null)
                .ToList() ?? new List<string?>();

            var currentNamespace = classDeclaration.Ancestors()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault()?.Name.ToString();

            if (currentNamespace != null)
            {
                usings.Add(currentNamespace);
            }

            return usings;
        }

        private static void AnalyzeInheritance(ClassDeclarationSyntax classDeclaration,
            List<string?> usings, HashSet<string> dependencies)
        {
            if (classDeclaration.BaseList == null) return;

            foreach (var baseType in classDeclaration.BaseList.Types)
            {
                AddDependencyIfExists(baseType.Type.ToString(), usings, dependencies);
            }
        }

        private static void AnalyzeClassMembers(ClassDeclarationSyntax classDeclaration,
            List<string?> usings, HashSet<string> dependencies)
        {
            var allNodes = classDeclaration.DescendantNodes();

            foreach (var node in allNodes)
            {
                AnalyzeNode(node, usings, dependencies);
            }
        }

        private static void AnalyzeNode(SyntaxNode node, List<string?> usings, HashSet<string> dependencies)
        {
            switch (node)
            {
                case FieldDeclarationSyntax field:
                    AddDependencyIfExists(field.Declaration.Type.ToString(), usings, dependencies);
                    break;

                case PropertyDeclarationSyntax property:
                    AddDependencyIfExists(property.Type.ToString(), usings, dependencies);
                    break;

                case VariableDeclarationSyntax variable:
                    AddDependencyIfExists(variable.Type.ToString(), usings, dependencies);
                    break;

                case ObjectCreationExpressionSyntax creation:
                    AddDependencyIfExists(creation.Type.ToString(), usings, dependencies);
                    break;

                case InvocationExpressionSyntax invocation:
                    AnalyzeInvocation(invocation, usings, dependencies);
                    break;

                case ParameterSyntax parameter:
                    AddDependencyIfExists(parameter.Type?.ToString() ?? "", usings, dependencies);
                    break;

                case GenericNameSyntax generic:
                    AnalyzeGenericType(generic, usings, dependencies);
                    break;

                case IdentifierNameSyntax identifier:
                    AddDependencyIfExists(identifier.Identifier.Text, usings, dependencies);
                    break;

                case MemberAccessExpressionSyntax memberAccess when !(memberAccess.Parent is InvocationExpressionSyntax):
                    AddDependencyIfExists(memberAccess.Expression.ToString(), usings, dependencies);
                    break;
            }
        }

        private static void AnalyzeInvocation(InvocationExpressionSyntax invocation,
            List<string?> usings, HashSet<string> dependencies)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                AddDependencyIfExists(memberAccess.Expression.ToString(), usings, dependencies);
            }
        }

        private static void AnalyzeGenericType(GenericNameSyntax generic,
            List<string?> usings, HashSet<string> dependencies)
        {
            foreach (var typeArg in generic.TypeArgumentList.Arguments)
            {
                AddDependencyIfExists(typeArg.ToString(), usings, dependencies);
            }
        }

        private static void AddDependencyIfExists(string typeName, List<string?> usings, HashSet<string> dependencies)
        {
            var fullTypeName = GetFullTypeNameFromSymbol(typeName, usings);
            if (_solutionClasses.Contains(fullTypeName))
            {
                dependencies.Add(fullTypeName);
            }
        }

        private static string GetFullTypeNameFromSymbol(string typeName, IEnumerable<string?> usings)
        {
            if (string.IsNullOrEmpty(typeName))
                return typeName;

            if (!typeName.Contains("<"))
                return ResolveSimpleTypeName(typeName, usings);

            return HandleGenericType(typeName, usings);
        }

        private static string ResolveSimpleTypeName(string typeName, IEnumerable<string?> usings)
        {
            if (typeName.Contains("."))
                return typeName;

            return FindTypeInUsings(typeName, usings);
        }

        private static string FindTypeInUsings(string typeName, IEnumerable<string?> usings)
        {
            foreach (var usingStatement in usings)
            {
                var fullName = $"{usingStatement}.{typeName}";
                if (_solutionClasses.Contains(fullName))
                {
                    return fullName;
                }
            }
            return typeName;
        }

        private static string HandleGenericType(string typeName, IEnumerable<string?> usings)
        {
            var parts = SplitGenericType(typeName);
            return string.Join("", parts.Select(p => p.Contains("<")
                ? p
                : GetFullTypeNameFromSymbol(p, usings)));
        }

        private static List<string> SplitGenericType(string typeName)
        {
            var parts = new List<string>();
            var depth = 0;
            var lastIndex = 0;

            for (var i = 0; i < typeName.Length; i++)
            {
                if (typeName[i] == '<')
                {
                    if (depth == 0)
                    {
                        parts.Add(typeName[lastIndex..i]);
                        lastIndex = i;
                    }
                    depth++;
                }
                else if (typeName[i] == '>')
                {
                    depth--;
                    if (depth == 0)
                    {
                        parts.Add(typeName[lastIndex..(i + 1)]);
                        lastIndex = i + 1;
                    }
                }
            }

            if (lastIndex < typeName.Length)
                parts.Add(typeName[lastIndex..]);

            return parts;
        }

        public static List<DependencyRelation> GetDependencies()
        {
            return _dependencyMap.SelectMany(kvp => kvp.Value).ToList() ?? new List<DependencyRelation>();
        }

        public static void Clear()
        {
            _dependencyMap.Clear();
            _solutionClasses.Clear();
        }
    }
}