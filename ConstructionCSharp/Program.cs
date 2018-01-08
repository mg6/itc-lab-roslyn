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
        }
    }
}