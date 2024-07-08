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
        public Dictionary<string, List<string>> DetectCodeDuplicationInFiles(List<string> files)
        {
            var duplicationMap = new ConcurrentDictionary<string, List<string>>();
            var hashMap = new ConcurrentDictionary<string, string>();

            Parallel.ForEach(files, file =>
            {
                var normalizedPath = Path.GetFullPath(file);
                var sourceCode = File.ReadAllText(normalizedPath);
                DetectCodeDuplicationInSourceCode(duplicationMap, hashMap, normalizedPath, sourceCode);
            });

            return duplicationMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public void DetectCodeDuplicationInSourceCode(ConcurrentDictionary<string, List<string>> duplicationMap, ConcurrentDictionary<string, string> hashMap, string normalizedPath, string sourceCode)
        {
            var tree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = tree.GetRoot();
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            Parallel.ForEach(methods, method =>
            {
                var blocks = ExtractBlocks(method);
                //Console.WriteLine(blocks.ToString());

                foreach (var block in blocks)
                {
                    var code = NormalizeCode(block.ToFullString());
                    //Console.WriteLine(code);
                    var hash = HashUtils.ComputeHash(code);
                    hashMap.AddOrUpdate(hash, normalizedPath + ": " + method.Identifier.Text, (key, oldValue) =>
                    {
                        duplicationMap.AddOrUpdate(oldValue, new List<string> { oldValue, normalizedPath + ": " + method.Identifier.Text }, (key, list) =>
                        {
                            list.Add(normalizedPath + ": " + method.Identifier.Text);
                            return list;
                        });
                        return oldValue;
                    });
                }

            });
        }

        // Extracts and returns the blocks of syntax within the method declaration syntax provided.
        private static IEnumerable<BlockSyntax> ExtractBlocks(MethodDeclarationSyntax method)
        {
            return method.DescendantNodes().OfType<BlockSyntax>();
        }

        // Normalizes the input code by removing white spaces and returns the resulting string.
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
