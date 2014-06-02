using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Diagnostics
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(DiagnosticId, LanguageNames.CSharp)]
    public class EveryParameterDocumented : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "S005";
        internal const string Description = "Document every parameter of a method";
        internal const string MessageFormat = "Document this parameter.";
        internal const string Category = "SonarQube";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(SyntaxKind.MethodDeclaration); } }

        private static bool isDocumented(ParameterSyntax node)
        {
            MethodDeclarationSyntax methodDeclarationNode = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            foreach (SyntaxTrivia trivia in methodDeclarationNode.GetLeadingTrivia())
            {
                if (trivia.HasStructure)
                {
                    SyntaxNode structure = trivia.GetStructure();
                    if (structure.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                    {
                       foreach (SyntaxNode child in structure.ChildNodes())
                       {
                            if (child.IsKind(SyntaxKind.XmlElement))
                            {
                                XmlElementSyntax xmlElementNode = (XmlElementSyntax)child;
                                if (xmlElementNode.GetText().ToString().Contains(node.Identifier.ValueText))
                                {
                                    return true;
                                }
                            }
                       }
                    }
                }
            }

            return false;
        }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        {
            MethodDeclarationSyntax methodDeclarationNode = (MethodDeclarationSyntax)node;

            foreach (ParameterSyntax parameterNode in methodDeclarationNode.ParameterList.Parameters)
            {
                if (!isDocumented(parameterNode))
                {
                    addDiagnostic(Diagnostic.Create(Rule, parameterNode.GetLocation()));
                }
            }
        }
    }
}
