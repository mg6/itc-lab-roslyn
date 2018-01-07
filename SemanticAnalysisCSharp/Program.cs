using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace SemanticAnalysisCSharp
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

            var root = tree.GetRoot() as CompilationUnitSyntax;

            var compilation = CSharpCompilation.Create("HelloWorld")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);
        }
    }
}
