using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Diagnostics
{
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(DiagnosticId, LanguageNames.CSharp)]
    public class DepthInheritanceTree : ISymbolAnalyzer
    {
        internal const string DiagnosticId = "S003";
        internal const string Description = "Limit the depth of inheritance tree";
        internal const string MessageFormat = "Reduce the depth of inheritance tree from {0} to at most {1}.";
        internal const string Category = "SonarQube";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SymbolKind> SymbolKindsOfInterest { get { return ImmutableArray.Create(SymbolKind.NamedType); } }

        public void AnalyzeSymbol(ISymbol symbol, Compilation compilation, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        {
            INamedTypeSymbol namedSymbol = (INamedTypeSymbol)symbol;

            int dit = 0;
            while (namedSymbol.BaseType != null)
            {
                dit++;
                namedSymbol = namedSymbol.BaseType;
            }

            if (dit > 2)
            {
                addDiagnostic(Diagnostic.Create(Rule, symbol.Locations[0], dit, 2));
            }
        }
    }
}
