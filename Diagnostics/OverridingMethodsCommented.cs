using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Diagnostics
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(DiagnosticId, LanguageNames.CSharp)]
    public class OverridingMethodsCommented : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "S004";
        internal const string Description = "Overriding methods should be commented";
        internal const string MessageFormat = "Add a comment to this overriding method.";
        internal const string Category = "SonarQube";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(SyntaxKind.MethodDeclaration); } }

        private static bool IsOverriding(MethodDeclarationSyntax node, SemanticModel semanticModel)
        {
            IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(node);
            return methodSymbol.OverriddenMethod != null;
        }

        private static bool HasComment(SyntaxNode node)
        {
            return node.GetLeadingTrivia().Any(SyntaxKind.SingleLineCommentTrivia) ||
                node.GetLeadingTrivia().Any(SyntaxKind.MultiLineCommentTrivia);
        }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        {
            MethodDeclarationSyntax methodDeclarationNode = (MethodDeclarationSyntax)node;

            if (IsOverriding(methodDeclarationNode, semanticModel) && !HasComment(methodDeclarationNode))
            {
                addDiagnostic(Diagnostic.Create(Rule, methodDeclarationNode.GetLocation()));
            }
        }
    }
}
