using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace OhNoPub.ImplicitCastAnalyzer
{
    public abstract class OhNoPubImplicitCastForeachAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "OhNoPubImplicitCastForeach";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        protected static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitleForeach), Resources.ResourceManager, typeof(Resources));
        protected static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        protected static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        protected const string Category = "ImplicitCast";

        protected static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected void ReportImplicitCastDiagnostic(
            SyntaxNodeAnalysisContext context,
            Location location,
            string from,
            string to)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Rule,
                    location,
                    from,
                    to));
        }
    }
}
