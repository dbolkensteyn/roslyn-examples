using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Generic;
using System.Text;

namespace Diagnostics
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(DiagnosticId, LanguageNames.CSharp)]
    public class MethodsReturningNull : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "S006";
        internal const string Description = "Methods returning null should be annotated";
        internal const string MessageFormat = "Refactor this return statement to not return null.";
        internal const string Category = "SonarQube";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(SyntaxKind.MethodDeclaration); } }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        {
            MethodDeclarationSyntax methodDeclarationNode = (MethodDeclarationSyntax)node;

            ControlFlowGraph cfg = ControlFlowGraph.Create(methodDeclarationNode.Body);
            HashSet<ISymbol> nulls = new HashSet<ISymbol>();

            Check(cfg.BasicBlocks[0], nulls, semanticModel, addDiagnostic);
        }

        private static void Check(ControlFlowBasicBlock basicBlock, HashSet<ISymbol> nulls, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic)
        {
            foreach (SyntaxNode node in basicBlock.Statements)
            {
                if (node.IsKind(SyntaxKind.SimpleAssignmentExpression))
                {
                    BinaryExpressionSyntax statement = (BinaryExpressionSyntax)node;

                    ISymbol target = semanticModel.GetSymbolInfo(statement.Left).Symbol;
                    if (target != null)
                    {
                        if (IsNull(statement.Right, nulls, semanticModel))
                        {
                            nulls.Add(target);
                        }
                        else
                        {
                            nulls.Remove(target);
                        }
                    }
                }
                else if (node.IsKind(SyntaxKind.ReturnStatement))
                {
                    ReturnStatementSyntax statement = (ReturnStatementSyntax)node;

                    if (IsNull(statement.Expression, nulls, semanticModel))
                    {
                        addDiagnostic(Diagnostic.Create(Rule, statement.GetLocation()));
                    }
                }
            }
        }

        private static bool IsNull(ExpressionSyntax node, HashSet<ISymbol> nulls, SemanticModel semanticModel)
        {
            Optional<object> constantValue = semanticModel.GetConstantValue(node);
            if (constantValue.HasValue && constantValue.Value == null)
            {
                return true;
            }

            ISymbol source = semanticModel.GetSymbolInfo(node).Symbol;
            if (source != null && nulls.Contains(source))
            {
                return true;
            }

            return false;
        }
    }
}
