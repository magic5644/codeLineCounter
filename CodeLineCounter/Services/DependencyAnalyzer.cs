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
        private static readonly object _dependencyLock = new();

        public static void AnalyzeSolution(string solutionFilePath)
        {
            var projectFiles = FileUtils.GetProjectFiles(solutionFilePath);

            AnalyzeProjects(projectFiles);
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
                var className = classDeclaration.Identifier.Text;
                var dependencies = ExtractDependencies(classDeclaration);

                foreach (var dependency in dependencies)
                {
                    var relation = new DependencyRelation
                    {
                        SourceClass = className,
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
            var typesToExclude = new HashSet<string>
            { 
            // System Object
            "object",
            "Object",
            "System.Object",
            
            // Value Types
            "bool",
            "Boolean",
            "System.Boolean",
            "byte",
            "Byte",
            "System.Byte",
            "sbyte",
            "SByte",
            "System.SByte",
            "char",
            "Char",
            "System.Char",
            "decimal",
            "Decimal",
            "System.Decimal",
            "double",
            "Double",
            "System.Double",
            "float",
            "Single",
            "System.Single",
            "int",
            "Int32",
            "System.Int32",
            "uint",
            "UInt32",
            "System.UInt32",
            "long",
            "Int64",
            "System.Int64",
            "ulong",
            "UInt64",
            "System.UInt64",
            "short",
            "Int16",
            "System.Int16",
            "ushort",
            "UInt16",
            "System.UInt16",
            
            // String
            "string",
            "String",
            "System.String",
            
            // Common Base Types
            "ValueType",
            "System.ValueType",
            "Enum",
            "System.Enum",
            "Delegate",
            "System.Delegate",
            "MulticastDelegate",
            "System.MulticastDelegate",
            
            // Common System Types
            "void",
            "Void",
            "System.Void",
            "DateTime",
            "System.DateTime",
            "TimeSpan",
            "System.TimeSpan",
            "Guid",
            "System.Guid",
            
            // Common Collections
            "Array",
            "System.Array",
            "IEnumerable",
            "System.Collections.IEnumerable",
            "IEnumerable<>",
            "System.Collections.Generic.IEnumerable<>",
            "ICollection",
            "System.Collections.ICollection",
            "ICollection<>",
            "System.Collections.Generic.ICollection<>",
            "IList",
            "System.Collections.IList",
            "IList<>",
            "System.Collections.Generic.IList<>",
            "List<>",
            "System.Collections.Generic.List<>",
            "IDictionary",
            "System.Collections.IDictionary",
            "IDictionary<,>",
            "System.Collections.Generic.IDictionary<,>",
            "Dictionary<,>",
            "System.Collections.Generic.Dictionary<,>",
            
            // Task Types
            "Task",
            "System.Threading.Tasks.Task",
            "Task<>",
            "System.Threading.Tasks.Task<>",
            "ValueTask",
            "System.Threading.Tasks.ValueTask",
            "ValueTask<>",
            "System.Threading.Tasks.ValueTask<>"
        };

        

            // Analyze inheritance
            if (classDeclaration.BaseList != null)
            {
                foreach (var baseType in classDeclaration.BaseList.Types)
                {
                    var typeName = baseType.Type.ToString();
                    if (!typesToExclude.Contains(typeName))
                    {
                        dependencies.Add(typeName);
                    }
                }
            }

            // Rest of the method remains the same
            foreach (var member in classDeclaration.Members)
            {
                /*if (member is FieldDeclarationSyntax field)
                {
                    foreach (var variable in field.Declaration.Variables)
                    {
                        var typeName = field.Declaration.Type.ToString();
                        if (!typesToExclude.Contains(typeName))
                        {
                            dependencies.Add(typeName);
                        }
                    }
                }
                else */
                if (member is PropertyDeclarationSyntax property)
                {
                    var typeName = property.Type.ToString();
                    if (!typesToExclude.Contains(typeName))
                    {
                        dependencies.Add(typeName);
                    }
                }
                else if (member is MethodDeclarationSyntax method)
                {
                    var returnType = method.ReturnType.ToString();
                    if (!typesToExclude.Contains(returnType))
                    {
                        dependencies.Add(returnType);
                    }

                    foreach (var parameter in method.ParameterList.Parameters)
                    {
                        var paramType = parameter.Type.ToString();
                        if (!typesToExclude.Contains(paramType))
                        {
                            dependencies.Add(paramType);
                        }
                    }
                }
            }

            return dependencies;
        }

        public static List<DependencyRelation> GetDependencies()
        {
            return _dependencyMap.SelectMany(kvp => kvp.Value).ToList() ?? new List<DependencyRelation>();
        }
    }
}