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

            var model = compilation.GetSemanticModel(tree);

            var usingInfo = model.GetSymbolInfo(root.Usings[0].Name);

            var usingSymbol = usingInfo.Symbol as INamespaceSymbol;
            Debug.Assert(usingSymbol.Name == "System");
            Debug.Assert(usingSymbol.Kind == SymbolKind.Namespace);

            var nsMembers = usingSymbol.GetNamespaceMembers();

            var actualNsMembers = nsMembers.Select(e => e.Name.ToString()).ToList();
            var expectedNsMembers = new List<string>() {
                "Collections",
                "IO",
                "Numerics",
                "Threading",
            };
            Debug.Assert(expectedNsMembers.All(e => actualNsMembers.Contains(e)));

            var helloWorldString = root.DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .First();
            Debug.Assert(helloWorldString.Token.ValueText == "Hello world!");

            var literalInfo = model.GetTypeInfo(helloWorldString);
            var literalType = literalInfo.Type;
            Debug.Assert(literalType.Name == "String");

            var members = (from method in literalType.GetMembers().OfType<IMethodSymbol>()
                           where method.ReturnType.Equals(literalType) &&
                                 method.DeclaredAccessibility == Accessibility.Public
                           select method.Name).Distinct();

            var actualStringMembers = members.ToList();
            var expectedStringMembers = new List<string> {
                "Join",
                "Substring",
                "Format",
            };
            Debug.Assert(expectedStringMembers.All(e => actualStringMembers.Contains(e)));
        }
    }
}
