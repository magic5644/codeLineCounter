using CodeLineCounter.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace CodeLineCounter.Tests
{
    public class CyclomaticComplexityCalculatorTests
    {
        [Fact]
        public void TestCalculateComplexity()
        {
            // Arrange
            var code = @"
                using System;
                namespace TestNamespace
                {
                    public class TestClass
                    {
                        public void TestMethod()
                        {
                            if (true) {}
                            for (int i = 0; i < 10; i++) {}
                        }
                    }
                }
            ";
            var tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("Test", new[] { tree });
            var model = compilation.GetSemanticModel(tree);
            var calculator = new CyclomaticComplexityCalculator();

            // Act
            var complexity = calculator.Calculate(tree.GetRoot(), model);

            // Assert
            Assert.Equal(3, complexity); // 1 (default) + 1 (if) + 1 (for)
        }
    }
}
