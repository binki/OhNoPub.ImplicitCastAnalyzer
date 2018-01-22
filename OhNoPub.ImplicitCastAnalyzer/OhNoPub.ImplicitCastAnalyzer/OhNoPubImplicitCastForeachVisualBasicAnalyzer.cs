using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace OhNoPub.ImplicitCastAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public class OhNoPubImplicitCastForeachVisualBasicAnalyzer : OhNoPubImplicitCastForeachAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeForEachSyntaxNode, SyntaxKind.ForEachStatement);
        }

        void AnalyzeForEachSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (ForEachStatementSyntax)context.Node;

            // Get information about the control variable.
            var controlVariable = forEachStatement.ControlVariable;
            if (controlVariable == null)
            {
                // Ignore if error.
                return;
            }
            // Figure out what the target type is. If this is a variable declaration
            // (e.g., Dim thing As X), it is a VariableDeclaratorSyntax. Otherwise, it
            // is likely a reference to a local variable.
            ITypeSymbol toType;
            if (controlVariable is VariableDeclaratorSyntax declarator)
            {
                var asClauseType = declarator?.AsClause?.Type();
                if (asClauseType == null)
                {
                    // Variable declaration has no type specified, so no cast to worry about.
                    return;
                }
                toType = context.SemanticModel.GetTypeInfo(asClauseType).Type;
            }
            else
            {
                // Is a reference to a local variable. Try to get the type directly.
                toType = context.SemanticModel.GetTypeInfo(controlVariable).Type;
            }

            if (toType == null
                || toType.TypeKind == TypeKind.Error
                // Converting to object is always allowed.
                || toType.SpecialType == SpecialType.System_Object)
            {
                return;
            }

            // Figure out source type.
            var expressionType = context.SemanticModel.GetTypeInfo(forEachStatement.Expression).Type;
            if (expressionType == null
                || expressionType.TypeKind == TypeKind.Error)
            {
                return;
            }

            // Calculate source element type.
            if (expressionType.TypeKind == TypeKind.Array)
            {
                var arrayType = (IArrayTypeSymbol)expressionType;
                consider(
                    from: arrayType.ElementType);
            }
            else
            {
                var forEachStatementInfo = context.SemanticModel.GetForEachStatementInfo(forEachStatement);
                consider(
                    from: forEachStatementInfo.CurrentProperty.Type);
            }

            void consider(
                ITypeSymbol from)
            {
                var conversion = context.Compilation.ClassifyConversion(
                    from,
                    toType);

                if (conversion.IsNarrowing)
                {
                    ReportImplicitCastDiagnostic(
                        context,
                        controlVariable.GetLocation(),
                        from.ToDisplayString(),
                        toType.ToDisplayString());
                }
            }
        }
    }
}
