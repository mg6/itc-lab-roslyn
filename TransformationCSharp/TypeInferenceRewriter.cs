using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TransformationCSharp
{
    /// <summary>
    /// Semantic model rewriter of explicit types in local variable declarations into `var` keywords.
    /// </summary>
    /// <remarks>
    /// This rewriter provides support only for simplest form of local variable declaration:
    /// 
    ///     Type variable = expression;
    ///
    /// Other possible forms, that are not supported:
    ///
    ///     Type variable;  # no initializer
    ///
    ///     Type variable1 = expression1,   # multiple identifiers
    ///          variable2 = expression2;
    ///
    /// </remarks>
    class TypeInferenceRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel SemanticModel;

        public TypeInferenceRewriter(SemanticModel semanticModel)
        {
            this.SemanticModel = semanticModel;
        }

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (node.Declaration.Variables.Count > 1 ||
                node.Declaration.Variables[0].Initializer == null)
            {
                return node;
            }

            VariableDeclaratorSyntax declarator = node.Declaration.Variables.First();

            TypeSyntax variableTypeName = node.Declaration.Type;
            ITypeSymbol variableType = SemanticModel.GetSymbolInfo(variableTypeName).Symbol as ITypeSymbol;

            TypeInfo initializerInfo = SemanticModel.GetTypeInfo(declarator.Initializer.Value);

            if (variableType == initializerInfo.Type)
            {
                // rewrite preserving whitespace (= trivia) around target
                TypeSyntax varTypeName = IdentifierName("var")
                    .WithLeadingTrivia(variableTypeName.GetLeadingTrivia())
                    .WithTrailingTrivia(variableTypeName.GetTrailingTrivia());
                return node.ReplaceNode(variableTypeName, varTypeName);
            }

            return node;
        }
    }
}
