using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OhNoPub.ImplicitCastAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic, Name = nameof(OhNoPubImplicitCastForeachVisualBasicCodeFixProvider)), Shared]
    public sealed class OhNoPubImplicitCastForeachVisualBasicCodeFixProvider : CodeFixProvider
    {
        const string title = "Avoid implicit cast by removing 'As' clause";

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(OhNoPubImplicitCastForeachAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the variable declaration identified by the diagnostic.
            if (root.FindNode(diagnosticSpan) is VariableDeclaratorSyntax declarator)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: ct => RemoveAsClause(context.Document, declarator, ct),
                        equivalenceKey: title),
                    diagnostic);
            }
        }

        async Task<Document> RemoveAsClause(Document document, VariableDeclaratorSyntax declarator, CancellationToken ct)
        {
            var newDeclarator = SyntaxFactory.VariableDeclarator(
                declarator.Names);

            var oldRoot = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
            var newRoot = oldRoot.ReplaceNode(declarator, newDeclarator);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
