using CodeLineCounter.Services;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeLineCounter.Tests
{
    public class CyclomaticComplexityCalculatorTests : TestBase
    {
        [Fact]
        public void TestCalculateComplexity()
        {
            // Arrange
            var code = @"
                    public class TestClass
                    {
                        public void TestMethod()
                        {
                            if (true) {}
                            for (int i = 0; i < 10; i++) {}
                        }
                    }
                ";
            var tree = CSharpSyntaxTree.ParseText(code);

            // Act
            var complexity = CyclomaticComplexityCalculator.Calculate(tree.GetRoot());

            // Assert
            Assert.Equal(3, complexity); // 1 (default) + 1 (if) + 1 (for)

        }

        [Fact]
        public void Calculate_Should_Return_Correct_Complexity()
        {
            // Arrange
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
                public class MyClass
                {
                    public void MyMethod()
                    {
                        if (true)
                        {
                            // Do something
                        }
                        else
                        {
                            // Do something else
                        }
                    }
                }
                ");
            var root = syntaxTree.GetRoot();
            var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

            // Act
            var complexity = CyclomaticComplexityCalculator.Calculate(methodDeclaration);

            // Assert
            Assert.Equal(3, complexity);

        }

        [Fact]
        public void Calculate_Should_Return_Correct_Complexity_6()
        {
            // Arrange
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
                class Program
                {
                    static void Main(string[] args)
                    {
                        int x = 5;

                        switch (x)
                        {
                        case 1:
                        Console.WriteLine(1);
                        break;
                        case 2:
                        Console.WriteLine(2);  
                        break;
                        case 3:
                        Console.WriteLine(3);
                        break;
                        case 4:
                        Console.WriteLine(4);
                        break;
                        }
                    }
                }
                ");
            var root = syntaxTree.GetRoot();
            var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

            // Act
            var complexity = CyclomaticComplexityCalculator.Calculate(methodDeclaration);

            // Assert
            Assert.Equal(6, complexity);
        }
    }
}
