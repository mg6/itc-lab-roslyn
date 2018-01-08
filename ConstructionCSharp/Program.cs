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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ConstructionCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            NameSyntax name = IdentifierName("System");
            Debug.Assert(name.ToString() == "System");

            name = QualifiedName(name, IdentifierName("Collections"));
            Debug.Assert(name.ToString() == "System.Collections");

            name = QualifiedName(name, IdentifierName("Generic"));
            Debug.Assert(name.ToString() == "System.Collections.Generic");

            var nonExistentName = IdentifierName("NonExistent");
            Debug.Assert(nonExistentName.ToString() == "NonExistent");

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

            var oldUsing = root.Usings[1]; // using System.Collections;
            var newUsing = oldUsing.WithName(name);
            Debug.Assert(root.ToString().Contains("using System.Collections;"));
            Debug.Assert(!root.ToString().Contains("using System.Collections.Generic;"));

            root = root.ReplaceNode(oldUsing, newUsing);
            Debug.Assert(!root.ToString().Contains("using System.Collections;"));
            Debug.Assert(root.ToString().Contains("using System.Collections.Generic;"));
        }
    }
}
