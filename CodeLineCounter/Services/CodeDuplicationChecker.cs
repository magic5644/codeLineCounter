using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CodeLineCounter.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeLineCounter.Services
{
    public class CodeDuplicationChecker
    {
        private readonly ConcurrentDictionary<string, HashSet<(string filePath, string methodName, int startLine)>> duplicationMap;
        private readonly ConcurrentDictionary<string, HashSet<(string filePath, string methodName, int startLine)>> hashMap;
        private readonly object duplicationLock = new object();

        public CodeDuplicationChecker()
        {
            duplicationMap = new ConcurrentDictionary<string, HashSet<(string filePath, string methodName, int startLine)>>();
            hashMap = new ConcurrentDictionary<string, HashSet<(string filePath, string methodName, int startLine)>>();
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
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            Parallel.ForEach(methods, method =>
            {
                var blocks = ExtractBlocks(method);

                foreach (var block in blocks)
                {
                    var code = NormalizeCode(block.ToFullString());
                    var hash = HashUtils.ComputeHash(code);
                    var location = block.GetLocation().GetLineSpan().StartLinePosition.Line;

                    hashMap.AddOrUpdate(hash, new HashSet<(string filePath, string methodName, int startLine)>
                    {
                        (normalizedPath, method.Identifier.Text, location)
                    },
                    (key, set) =>
                    {
                        lock (set)
                        {
                            set.Add((normalizedPath, method.Identifier.Text, location));
                        }
                        return set;
                    });

                }
            });

            UpdateDuplicationMap();
        }

        private void UpdateDuplicationMap()
        {
            lock (duplicationLock)
            {
                foreach (var entry in hashMap)
                {
                    if (entry.Value.Count > 1)
                    {
                        duplicationMap[entry.Key] = entry.Value;
                    }
                }
            }
        }

        public Dictionary<string, List<(string filePath, string methodName, int startLine)>> GetCodeDuplicationMap()
        {
            return duplicationMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());
        }

        private static IEnumerable<BlockSyntax> ExtractBlocks(MethodDeclarationSyntax method)
        {
            return method.DescendantNodes().OfType<BlockSyntax>();
        }

        private string NormalizeCode(string code)
        {
            var stringBuilder = new StringBuilder();
            foreach (char c in code)
            {
                if (!char.IsWhiteSpace(c))
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString();
        }
    }
}
