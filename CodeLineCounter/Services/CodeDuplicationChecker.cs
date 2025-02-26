using System.Collections.Concurrent;
using System.Text;
using CodeLineCounter.Models;
using CodeLineCounter.Utils;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeLineCounter.Services
{
    public class CodeDuplicationChecker
    {
        private readonly ConcurrentDictionary<string, HashSet<DuplicationCode>> duplicationMap;
        private readonly ConcurrentDictionary<string, HashSet<DuplicationCode>> hashMap;
        private readonly Lock duplicationLock = new();

        public CodeDuplicationChecker()
        {
            duplicationMap = new ConcurrentDictionary<string, HashSet<DuplicationCode>>();
            hashMap = new ConcurrentDictionary<string, HashSet<DuplicationCode>>();
        }

        public void DetectCodeDuplicationInFiles(List<string> files)
        {
            Parallel.ForEach(files, file =>
            {
                var normalizedPath = Path.GetFullPath(file);
                var sourceCode = File.ReadAllText(normalizedPath);
                DetectCodeDuplicationInSourceCode(normalizedPath, sourceCode);
            });
            UpdateDuplicationMap();
        }

        public void DetectCodeDuplicationInSourceCode(string normalizedPath, string sourceCode)
        {
            var tree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = tree.GetRoot();
            // Get all method declarations
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            Parallel.ForEach(methods, method =>
            {
                // Extract code blocks from each method.
                var blocks = ExtractBlocks(method);

                foreach (var block in blocks)
                {
                    // Optimize normalization using string.Concat filtering whitespace.
                    var originalCode = block.ToFullString();
                    var code = NormalizeCode(originalCode);
                    var hash = HashUtils.ComputeHash(code);

                    // Compute start line and number of lines efficiently using location spans.
                    var span = block.GetLocation().GetLineSpan();
                    var startLine = span.StartLinePosition.Line;
                    var nbLines = span.EndLinePosition.Line - startLine + 1;

                    var duplicationCode = new DuplicationCode
                    {
                        CodeHash = hash,
                        FilePath = normalizedPath,
                        MethodName = method.Identifier.Text,
                        StartLine = startLine,
                        NbLines = nbLines
                    };

                    // Update the hash map with the new duplication code using thread-safe operations.
                    hashMap.AddOrUpdate(hash, 
                        key => new HashSet<DuplicationCode> { duplicationCode },
                        (key, existingSet) =>
                        {
                            lock (existingSet)
                            {
                                existingSet.Add(duplicationCode);
                            }
                            return existingSet;
                        });
                }
            });

        }

        public void UpdateDuplicationMap()
        {
            using (duplicationLock.EnterScope())
            {
                // Clear previous results to avoid stale data.
                duplicationMap.Clear();
                foreach (var entry in hashMap)
                {
                    if (entry.Value.Count > 1)
                    {
                        duplicationMap[entry.Key] = entry.Value;
                    }
                }
            }
        }

        public Dictionary<string, List<DuplicationCode>> GetCodeDuplicationMap()
        {
            return duplicationMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());
        }

        private static IEnumerable<BlockSyntax> ExtractBlocks(MethodDeclarationSyntax method)
        {
            return method.DescendantNodes().OfType<BlockSyntax>();
        }

        private static string NormalizeCode(string code)
        {
            // Use string.Concat with LINQ to filter out whitespace characters.
            return string.Concat(code.Where(c => !char.IsWhiteSpace(c)));
        }
    }
}