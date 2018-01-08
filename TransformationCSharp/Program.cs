using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace TransformationCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = CreateTestCompilation();

            foreach (SyntaxTree sourceTree in test.SyntaxTrees)
            {
                var model = test.GetSemanticModel(sourceTree);

                var rewriter = new TypeInferenceRewriter(model);
                var newSource = rewriter.Visit(sourceTree.GetRoot());

                if (newSource != sourceTree.GetRoot())
                {
                    File.WriteAllText(sourceTree.FilePath, newSource.ToFullString());
                }
            }
        }

        private static Compilation CreateTestCompilation()
        {
            var programPath = @"Program.cs";
            var programText = File.ReadAllText(programPath);
            var programTree = CSharpSyntaxTree.ParseText(programText)
                .WithFilePath(programPath);

            var rewriterPath = @"TypeInferenceRewriter.cs";
            var rewriterText = File.ReadAllText(rewriterPath);
            var rewriterTree = CSharpSyntaxTree.ParseText(rewriterText)
                .WithFilePath(rewriterPath);

            SyntaxTree[] sourceTrees = { programTree, rewriterTree };

            MetadataReference mscorlib = MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location);
            MetadataReference codeAnalysis = MetadataReference.CreateFromFile(
                typeof(SyntaxTree).Assembly.Location);
            MetadataReference cSharpCodeAnalysis = MetadataReference.CreateFromFile(
                typeof(CSharpSyntaxTree).Assembly.Location);

            MetadataReference[] references = { mscorlib, codeAnalysis, cSharpCodeAnalysis };

            return CSharpCompilation.Create("TransformationCS",
                sourceTrees, references, new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        }
    }
}
