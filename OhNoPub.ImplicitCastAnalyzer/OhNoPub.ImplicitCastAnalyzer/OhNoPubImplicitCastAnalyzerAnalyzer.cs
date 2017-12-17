using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace OhNoPub.ImplicitCastAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OhNoPubImplicitCastAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "OhNoPubImplicitCastAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeForEachSyntaxNode, SyntaxKind.ForEachStatement);
        }

        private void AnalyzeForEachSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (ForEachStatementSyntax)context.Node;

            if (forEachStatement.Type.IsVar)
            {
                // Fast path is var—we already know that will be compatible with the assignment.
                return;
            }

            var loopVariableTypeInfo = context.SemanticModel.GetTypeInfo(forEachStatement.Type);
            if (loopVariableTypeInfo.Type == null
                || loopVariableTypeInfo.Type.TypeKind == TypeKind.Error)
            {
                // Ignore untyped/error.
                return;
            }

            // There is a special case. In Roslyn, foreach-ing over an array is treated
            // in the syntax tree/semantics as IEnumerable instead of IEnumerable<ArrayType>.
            // Thus, it ends up looking like an implicitly explicit cast when it might not
            // be. So detect arrays first and handle them specially by checking the
            // element type.
            var expressionTypeInfo = context.SemanticModel.GetTypeInfo(forEachStatement.Expression);
            if (expressionTypeInfo.Type == null
                || expressionTypeInfo.Type.TypeKind == TypeKind.Error)
            {
                // Ignore untyped/error.
                return;
            }
            if (expressionTypeInfo.Type.TypeKind == TypeKind.Array)
            {
                var arrayTypeSymbol = (IArrayTypeSymbol)expressionTypeInfo.Type;
                considerConversion(
                    arrayTypeSymbol.ElementType,
                    loopVariableTypeInfo.Type);
                return;
            }

            // Now have to analyze type of assignment. Determine if is implicitly an explicit cast.
            var forEachStatementInfo = context.SemanticModel.GetForEachStatementInfo(forEachStatement);
            // Ensure it is a valid foreach. We may get called with an invalid foreach, in which case
            // one of the things will be null.
            if (forEachStatementInfo.ElementType == null)
            {
                return;
            }
            considerConversion(
                forEachStatementInfo.ElementType,
                loopVariableTypeInfo.Type);
            return;

            void considerConversion(
                ITypeSymbol from,
                ITypeSymbol to)
            {
                // Ignore forEachStatementInfo.CurrentConversion. It will claim that
                // conversion from object to int is “IsIdentity”. I.e., the compiler is
                // tricking itself out to think that such a conversion is permissible.
                // We need to actually ask the compiler directly if it thinks the conversion
                // exists instead.
                //
                // Also, for arrays, the conversion doesn’t take into account the
                // array type because the compiler tells itself that the array is IEnumerable
                // instead of IEnumerable<T>. So we need to classify that conversion based
                // on the compile-time known array type.
                var conversion = context.Compilation.ClassifyConversion(from, to);
                
                // Explicit conversion means that you would have to request it.
                // In most cases that means a runtime cast, especially likely
                // for reference and unboxing conversions. However, it is possible
                // that an explicit user-defined conversion exists. So ensure
                // is not user-defined.
                if (conversion.IsExplicit
                    && (conversion.IsReference || conversion.IsUnboxing)
                    && !conversion.IsUserDefined
                    // Converting to object is always allowed.
                    && to.SpecialType != SpecialType.System_Object)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Rule,
                            forEachStatement.Type.GetLocation(),
                            from.ToDisplayString(),
                            to.ToDisplayString()));
                }
            }
        }
    }
}
