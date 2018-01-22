using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;

namespace OhNoPub.ImplicitCastAnalyzer.Test
{
    public class OhNoPubImplicitCastUnitTestsBase : CodeFixVerifier
    {
        protected static string OhNoPubImplicitCastForeachId = "OhNoPubImplicitCastForeach";
        protected static string DiagnosticMessage = "Implicit run time cast from '{0}' to '{1}'";
    }
}
