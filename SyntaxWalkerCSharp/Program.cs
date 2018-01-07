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

namespace SyntaxWalkerCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(
            @"using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;

            namespace TopLevel
            {
                using Microsoft;
                using System.ComponentModel;

                namespace Child1
                {
                    using Microsoft.Win32;
                    using System.Runtime.InteropServices;

                    class Foo { }
                }

                namespace Child2
                {
                    using System.CodeDom;
                    using Microsoft.CSharp;

                    class Bar { }
                }
            }");

            var root = tree.GetRoot() as CompilationUnitSyntax;
        }
    }
}
