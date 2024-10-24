using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeLineCounter.Services
{
    public class CyclomaticComplexityCalculator
    {
        public static int Calculate(SyntaxNode node)
        {
            var walker = new ComplexityWalker();
            walker.Visit(node);
            return walker.Complexity;
        }

        private sealed class ComplexityWalker : CSharpSyntaxWalker
        {
            public int Complexity { get; private set; } = 1;

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                Complexity += CalculateComplexity(node);
                base.VisitMethodDeclaration(node);
            }

            public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
            {
                Complexity += CalculateComplexity(node);
                base.VisitConstructorDeclaration(node);
            }

            public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                if (node.AccessorList != null)
                {
                    foreach (var accessor in node.AccessorList.Accessors)
                    {
                        Complexity += CalculateComplexity(accessor);
                    }
                }
                base.VisitPropertyDeclaration(node);
            }

            private static int CalculateComplexity(SyntaxNode node)
            {
                var complexity = 0;
                var methodBody = node.DescendantNodes().OfType<BlockSyntax>().FirstOrDefault();
                if (methodBody != null)
                {
                    foreach (var descendant in methodBody.DescendantNodes())
                    {
                        switch (descendant.Kind())
                        {
                            case SyntaxKind.IfStatement:
                            case SyntaxKind.ElseClause:
                            case SyntaxKind.SwitchStatement:
                            case SyntaxKind.CaseSwitchLabel:
                            case SyntaxKind.DefaultSwitchLabel:
                            case SyntaxKind.ForStatement:
                            case SyntaxKind.ForEachStatement:
                            case SyntaxKind.WhileStatement:
                            case SyntaxKind.DoStatement:
                            case SyntaxKind.TryStatement:
                            case SyntaxKind.CatchClause:
                                complexity++;
                                break;
                        }
                    }
                }
                return complexity;
            }
        }
    }
}
