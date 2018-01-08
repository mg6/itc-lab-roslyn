using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConstAnalyzer
{
    /// <summary>
    /// Diagnostic analyzer that suggests adding `const` qualifiers to local variable declarations (compile-time constness).
    /// </summary>
    /// <remarks>
    /// Works with initializers in this forms:
    /// 
    ///     Type variable = SomeConstExpression;
    ///     Type variable1 = SomeConst1,
    ///          variable2 = SomeConst2, ... ;
    /// 
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConstAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ConstAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.LocalDeclarationStatement);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var localDeclaration = context.Node as LocalDeclarationStatementSyntax;

            // check if already const
            if (localDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
            {
                return;
            }

            var variableTypeName = localDeclaration.Declaration.Type;
            var variableType = context.SemanticModel.GetTypeInfo(variableTypeName).ConvertedType;

            foreach (var variable in localDeclaration.Declaration.Variables)
            {
                // ignore when no initializer: int x;
                var initializer = variable.Initializer;
                if (initializer == null)
                    return;

                // ignore non-compile-time const initializer: int x = f(y);
                var constValue = context.SemanticModel.GetConstantValue(initializer.Value);
                if (!constValue.HasValue)
                    return;

                // ignore declarations with incorrect type (non-castable expression): int i = "xyz";
                var conversion = context.SemanticModel.ClassifyConversion(initializer.Value, variableType);
                if (!conversion.Exists || conversion.IsUserDefined)
                    return;

                // ignore string declarations of non-string type
                if (constValue.Value is string)
                {
                    if (variableType.SpecialType != SpecialType.System_String)
                        return;
                }
                // ignore non-null references
                else if (variableType.IsReferenceType && constValue.Value != null)
                    return;
            }

            var dataFlowAnalysis = context.SemanticModel.AnalyzeDataFlow(localDeclaration);

            foreach (var variable in localDeclaration.Declaration.Variables)
            {
                var variableSymbol = context.SemanticModel.GetDeclaredSymbol(variable);
                if (dataFlowAnalysis.WrittenOutside.Contains(variableSymbol))
                    return;
            }

            var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
