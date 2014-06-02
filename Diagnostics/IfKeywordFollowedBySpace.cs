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
    public class IfKeywordFollowedBySpace : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "S001";
        internal const string Description = "if keywords should have exactly one space to separate them from the opening parenthesis";
        internal const string MessageFormat = "Use exactly one space to separate the if keyword from the opening parenthesis.";
        internal const string Category = "SonarQube";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(SyntaxKind.IfStatement); } }

        private static bool HasExactlyOneFollowingSpace(IfStatementSyntax node)
        {
            return node.IfKeyword.TrailingTrivia.Span.Length == 1;
        }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        {
            IfStatementSyntax ifNode = (IfStatementSyntax)node;

            if (!HasExactlyOneFollowingSpace(ifNode))
            {
                TextSpan textSpan = TextSpan.FromBounds(ifNode.IfKeyword.SpanStart, ifNode.OpenParenToken.SpanStart + 1);
                addDiagnostic(Diagnostic.Create(Rule, Location.Create(node.SyntaxTree, textSpan)));
            }
        }
    }
}
