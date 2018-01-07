using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace GettingStartedCSharpCodeAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(
                @"using System;
                using System.Collections;
                using System.Linq;
                using System.Text;

                namespace HelloWorld
                {
                    class Program
                    {
                        static void Main(String[] args)
                        {
                            Console.WriteLine(""Hello world!"");
                        }
                    }
                }");

            // manual traversal
            var root = tree.GetRoot() as CompilationUnitSyntax;
            var firstElement = root.Members[0] as NamespaceDeclarationSyntax;
            var programDeclaration = firstElement.Members[0] as ClassDeclarationSyntax;
            var mainDeclaration = programDeclaration.Members[0] as MethodDeclarationSyntax;
            var arrayParameterByManual = mainDeclaration.ParameterList.Parameters[0];

            // querying with LINQ
            var firstParameters = from methodDeclaration in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                                  where methodDeclaration.Identifier.ValueText == "Main"
                                  select methodDeclaration.ParameterList.Parameters.First();
            var arrayParameterByLinq = firstParameters.Single();

            Debug.Assert(arrayParameterByManual == arrayParameterByLinq);
        }
    }
}
