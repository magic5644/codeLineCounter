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
    public class DependencyAnalyzer
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
            // Vérifier d'abord le namespace filescoped
            var fileScopedNamespace = classDeclaration.SyntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<FileScopedNamespaceDeclarationSyntax>()
                .FirstOrDefault();
            if (fileScopedNamespace != null)
            {
                return $"{fileScopedNamespace.Name}.{classDeclaration.Identifier.Text}";
            }

            // Vérifier ensuite le namespace classique
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
            var compilation = root as CompilationUnitSyntax;
            var usings = compilation?.Usings.Select(u => u.Name.ToString()) ?? Enumerable.Empty<string>();

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
                                // Vérifier si la relation existe déjà
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
            var root = classDeclaration.SyntaxTree.GetRoot();
            // Récupérer les usings une seule fois au début
            var usings = (root as CompilationUnitSyntax)?.Usings.Select(u => u.Name.ToString()).ToList()
                         ?? new List<string>();

            // Ajouter le namespace courant aux usings si présent
            var currentNamespace = classDeclaration.Ancestors()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault()?.Name.ToString();
            if (currentNamespace != null)
            {
                usings.Add(currentNamespace);
            }

            // Analyze inheritance
            if (classDeclaration.BaseList != null)
            {
                foreach (var baseType in classDeclaration.BaseList.Types)
                {
                    var typeName = GetFullTypeNameFromSymbol(baseType.Type.ToString(), usings);
                    if (_solutionClasses.Contains(typeName))
                    {
                        dependencies.Add(typeName);
                    }
                }
            }

            // Analyze all nodes in the class
            var allNodes = classDeclaration.DescendantNodes();

            foreach (var node in allNodes)
            {
                switch (node)
                {
                    case FieldDeclarationSyntax field:
                        var fieldType = GetFullTypeNameFromSymbol(field.Declaration.Type.ToString(), usings);
                        if (_solutionClasses.Contains(fieldType))
                        {
                            dependencies.Add(fieldType);
                        }
                        break;

                    case PropertyDeclarationSyntax property:
                        var propertyType = GetFullTypeNameFromSymbol(property.Type.ToString(), usings);
                        if (_solutionClasses.Contains(propertyType))
                        {
                            dependencies.Add(propertyType);
                        }
                        break;

                    case VariableDeclarationSyntax variable:
                        var varType = GetFullTypeNameFromSymbol(variable.Type.ToString(), usings);
                        if (_solutionClasses.Contains(varType))
                        {
                            dependencies.Add(varType);
                        }
                        break;

                    case ObjectCreationExpressionSyntax creation:
                        var creationType = GetFullTypeNameFromSymbol(creation.Type.ToString(), usings);
                        if (_solutionClasses.Contains(creationType))
                        {
                            dependencies.Add(creationType);
                        }
                        break;

                    case InvocationExpressionSyntax invocation:
                        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                        {
                            var expressionType = GetFullTypeNameFromSymbol(memberAccess.Expression.ToString(), usings);
                            if (_solutionClasses.Contains(expressionType))
                            {
                                dependencies.Add(expressionType);
                            }
                        }
                        break;

                    case ParameterSyntax parameter:
                        var paramType = GetFullTypeNameFromSymbol(parameter.Type?.ToString() ?? "", usings);
                        if (_solutionClasses.Contains(paramType))
                        {
                            dependencies.Add(paramType);
                        }
                        break;

                    case GenericNameSyntax generic:
                        foreach (var typeArg in generic.TypeArgumentList.Arguments)
                        {
                            var genericType = GetFullTypeNameFromSymbol(typeArg.ToString(), usings);
                            if (_solutionClasses.Contains(genericType))
                            {
                                dependencies.Add(genericType);
                            }
                        }
                        break;
                    case IdentifierNameSyntax identifier:
                        var identifierType = GetFullTypeNameFromSymbol(identifier.Identifier.Text, usings);
                        if (_solutionClasses.Contains(identifierType))
                        {
                            dependencies.Add(identifierType);
                        }
                        break;

                    case MemberAccessExpressionSyntax innerMemberAccess when !(innerMemberAccess.Parent is InvocationExpressionSyntax):
                        var memberType = GetFullTypeNameFromSymbol(innerMemberAccess.Expression.ToString(), usings);
                        if (_solutionClasses.Contains(memberType))
                        {
                            dependencies.Add(memberType);
                        }
                        break;
                }
            }

            return dependencies;
        }

        private static IEnumerable<string> GetUsings(SyntaxNode node)
        {
            var root = node.SyntaxTree.GetRoot();
            var compilation = root as CompilationUnitSyntax;
            return compilation?.Usings.Select(u => u.Name.ToString()) ?? Enumerable.Empty<string>();
        }

        private static string GetFullTypeNameFromSymbol(string typeName, IEnumerable<string> usings)
        {
            if (string.IsNullOrEmpty(typeName))
                return typeName;

            // Gérer les types génériques imbriqués
            if (typeName.Contains("<"))
            {
                var depth = 0;
                var lastIndex = 0;
                var parts = new List<string>();

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

                return string.Join("", parts.Select(p => p.Contains("<")
                    ? p
                    : GetFullTypeNameFromSymbol(p, usings)));
            }

            // Si le type contient déjà un namespace, on le retourne tel quel
            if (typeName.Contains("."))
                return typeName;

            // Chercher dans les using si on trouve le namespace correspondant
            foreach (var usingStatement in usings)
            {
                var fullName = $"{usingStatement}.{typeName}";
                if (_solutionClasses.Contains(fullName))
                {
                    return fullName;
                }
            }

            // Vérifier dans le namespace courant
            return typeName;
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