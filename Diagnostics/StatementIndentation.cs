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
    public class StatementIdentation : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "S002";
        internal const string Description = "Statements should be indented";
        internal const string MessageFormat = "Start this statement at column {0}.";
        internal const string Category = "SonarQube";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(
            SyntaxKind.BreakStatement,
            SyntaxKind.CheckedStatement,
            SyntaxKind.ContinueStatement,
            SyntaxKind.DoStatement,
            SyntaxKind.EmptyStatement,
            SyntaxKind.ExpressionStatement,
            SyntaxKind.FixedStatement,
            SyntaxKind.ForEachStatement,
            SyntaxKind.ForStatement,
            SyntaxKind.GlobalStatement,
            SyntaxKind.GotoCaseStatement,
            SyntaxKind.GotoDefaultStatement,
            SyntaxKind.GotoStatement,
            SyntaxKind.IfStatement,
            SyntaxKind.LabeledStatement,
            SyntaxKind.LocalDeclarationStatement,
            SyntaxKind.LockStatement,
            SyntaxKind.ReturnStatement,
            SyntaxKind.SwitchStatement,
            SyntaxKind.ThrowStatement,
            SyntaxKind.TryStatement,
            SyntaxKind.UncheckedStatement,
            SyntaxKind.UnsafeStatement,
            SyntaxKind.UsingStatement,
            SyntaxKind.WhileStatement,
            SyntaxKind.YieldBreakStatement,
            SyntaxKind.YieldReturnStatement); } }

        private static int StartLine(SyntaxNode node)
        {
            return node.SyntaxTree.GetLineSpan(node.Span).StartLinePosition.Line;
        }

        private static int StartColumn(SyntaxNode node)
        {
            return node.SyntaxTree.GetLineSpan(node.Span).StartLinePosition.Character;
        }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        {
            int column = StartColumn(node);
            int expectedColumn = StartColumn(node.Parent) + 4;

            if (StartLine(node) != StartLine(node.Parent) && column != expectedColumn)
            {
                addDiagnostic(Diagnostic.Create(Rule, node.GetFirstToken().GetLocation(), expectedColumn + 1));
            }
        }
    }
}
