using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OhNoPub.ImplicitCastAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OhNoPubImplicitCastForeachCodeFixProvider)), Shared]
    public class OhNoPubImplicitCastForeachCodeFixProvider : CodeFixProvider
    {
        private const string title = "Avoid implicit cast with 'var'";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(OhNoPubImplicitCastForeachAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = (TypeSyntax)root.FindToken(diagnosticSpan.Start).Parent;

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => MakeUseVar(context.Document, declaration, ct), 
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> MakeUseVar(
            Document document,
            TypeSyntax type,
            CancellationToken cancellationToken)
        {
            var newType = SyntaxFactory.IdentifierName(
                SyntaxFactory.Identifier(
                    type.GetLeadingTrivia(),
                    "var",
                    type.GetTrailingTrivia()));

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(type, newType);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
