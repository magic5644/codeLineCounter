using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace CodeLineCounter.Services
{
    public class CyclomaticComplexityCalculator
    {
        public int Calculate(SyntaxNode node, SemanticModel model)
        {
            var walker = new ComplexityWalker(model);
            walker.Visit(node);
            return walker.Complexity;
        }

        private class ComplexityWalker : CSharpSyntaxWalker
        {
            private readonly SemanticModel _semanticModel;
            public int Complexity { get; private set; } = 1;

            public ComplexityWalker(SemanticModel semanticModel)
            {
                _semanticModel = semanticModel;
            }

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

            private int CalculateComplexity(SyntaxNode node)
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
                            case SyntaxKind.ForStatement:
                            case SyntaxKind.ForEachStatement:
                            case SyntaxKind.WhileStatement:
                            case SyntaxKind.DoStatement:
                            case SyntaxKind.CaseSwitchLabel:
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