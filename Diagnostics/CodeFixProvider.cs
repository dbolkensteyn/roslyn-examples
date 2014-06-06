using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Diagnostics
{
    [ExportCodeFixProvider(IfKeywordFollowedBySpace.DiagnosticId, LanguageNames.CSharp)]
    internal class CodeFixProvider : ICodeFixProvider
    {
        public IEnumerable<string> GetFixableDiagnosticIds()
        {
            return new[] { IfKeywordFollowedBySpace.DiagnosticId };
        }

        public async Task<IEnumerable<CodeAction>> GetFixesAsync(Document document, TextSpan span, IEnumerable<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest

            var diagnosticSpan = diagnostics.First().Location.SourceSpan;

            // Find the if statement identified by the diagnostic.
            var statement = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<IfStatementSyntax>().First();

            // Return a code action that will invoke the fix.
            return new[] { CodeAction.Create("Separate by exactly one space", c => SeparateByExactlyOnSpace(document, statement, c)) };
        }

        private async Task<Document> SeparateByExactlyOnSpace(Document document, IfStatementSyntax statement, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            SyntaxToken newIfToken = statement.IfKeyword.WithTrailingTrivia(SyntaxFactory.Whitespace(" "));
            SyntaxToken newOpenParenToken = statement.OpenParenToken.WithLeadingTrivia();
            IfStatementSyntax newStatement = statement.WithIfKeyword(newIfToken).WithOpenParenToken(newOpenParenToken);

            var newRoot = root.ReplaceNode(statement, newStatement);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
