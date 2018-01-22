using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace OhNoPub.ImplicitCastAnalyzer.Test
{
    [TestClass]
    public class OhNoPubImplicitCastForeachVisualBasicUnitTests : OhNoPubImplicitCastUnitTestsBase
    {

        // No diagnostics expected to show up
        [DataRow("String()")]
        [DataRow("System.Collections.IEnumerable")]
        [DataRow("System.Collections.Generic.IEnumerable(Of String)")]
        [DataTestMethod]
        public void TestNoCast(string type)
        {
            var source = @"
Class X
    Sub Main(numbers As " + type + @")
        For Each y In numbers
            Use(y)
        Next
    End Sub
    Sub Use(value As Object)
    End Sub
End Class
";
            VerifyBasicDiagnostic(source);
            VerifyBasicNoCompileErrors(source);
        }

        // No diagnostic expected to show up. Impossible to test IEnumerable with this.
        [DataRow("String()")]
        [DataRow("System.Collections.Generic.IEnumerable(Of String)")]
        [DataTestMethod]
        public void TestExplicitTypeName(string type)
        {
            var source = @"
Class X
    Sub Main(numbers As " + type + @")
        For Each y As String In numbers
            Use(y)
        Next
    End Sub
    Sub Use(value As Object)
    End Sub
End Class";
            VerifyBasicDiagnostic(source);
            VerifyBasicNoCompileErrors(source);
        }

        // No diagnostic expected to show up. Impossible to test IEnumerable with this.
        [DataRow("String()")]
        [DataRow("System.Collections.Generic.IEnumerable(Of String)")]
        [DataTestMethod]
        public void TestExplicitTypeName_ExistingVariable(string type)
        {
            var source = @"
Class X
    Sub Main(numbers As " + type + @")
        Dim y as String
        For Each y In numbers
            Use(y)
        Next
     End Sub
    Sub Use(value As Object)
    End Sub
End Class";
            VerifyBasicDiagnostic(source);
            VerifyBasicNoCompileErrors(source);
        }

        // No diagnostic expected to show up for Object.
        [DataRow("Object()")]
        [DataRow("System.Collections.IEnumerable")]
        [DataRow("System.Collections.Generic.IEnumerable(Of Object)")]
        [DataTestMethod]
        public void TestExplicitTypeName_Object(string type)
        {
            var source = @"
Class X
    Sub Main(numbers As " + type + @")
        For Each y As Object In numbers
            Use(y)
        Next
     End Sub
    Sub Use(value As Object)
    End Sub
End Class";
            VerifyBasicDiagnostic(source);
            VerifyBasicNoCompileErrors(source);
        }

        // No diagnostic expected to show up for no cast via existing variable.
        [DataRow("Object()")]
        [DataRow("System.Collections.IEnumerable")]
        [DataRow("System.Collections.Generic.IEnumerable(Of Object)")]
        [DataTestMethod]
        public void TestExplicitTypeName_Object_ExistingVariable(string type)
        {
            var source = @"
Class X
    Sub Main(numbers As " + type + @")
        For Each y As Object In numbers
            Use(y)
        Next
    End Sub
    Sub Use(value As Object)
    End Sub
End Class";
            VerifyBasicDiagnostic(source);
            VerifyBasicNoCompileErrors(source);
        }

        // Diagnostic expected
        [DataRow("Object()")]
        [DataRow("System.Collections.IEnumerable")]
        [DataRow("System.Collections.Generic.IEnumerable(Of Object)")]
        [DataTestMethod]
        public void TestForEachImplicitCast(string type)
        {
            var originalCode = @"
Class X
    Sub Main(numbers As " + type + @")
        For Each y As String In numbers
            Use(y)
        Next
    End Sub
    Sub Use(value As Object)
    End Sub
End Class";
            VerifyBasicDiagnostic(originalCode, new DiagnosticResult
            {
                Id = OhNoPubImplicitCastForeachId,
                Message = string.Format(DiagnosticMessage, "Object", "String"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.vb", 4, 18),
                },
            });
            VerifyBasicNoCompileErrors(originalCode);

            VerifyBasicFix(originalCode, @"
Class X
    Sub Main(numbers As " + type + @")
        For Each y In numbers
            Use(y)
        Next
    End Sub
    Sub Use(value As Object)
    End Sub
End Class");
        }
        
        // Diagnostic expected
        [DataRow("Object()")]
        [DataRow("System.Collections.IEnumerable")]
        [DataRow("System.Collections.Generic.IEnumerable(Of Object)")]
        [DataTestMethod]
        public void TestForEachImplicitCast_ExistingVariable(string type)
        {
            var originalCode = @"
Class X
    Sub Main(numbers As " + type + @")
        Dim y As String
        For Each y In numbers
            Use(y)
        Next
    End Sub
    Sub Use(value As Object)
    End Sub
End Class";
            VerifyBasicDiagnostic(originalCode, new DiagnosticResult
            {
                Id = OhNoPubImplicitCastForeachId,
                Message = string.Format(DiagnosticMessage, "Object", "String"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.vb", 5, 18),
                },
            });
            VerifyBasicNoCompileErrors(originalCode);

            // Even though the Diagnostic is raised, there should be *no* CodeFixes
            // because that’d involve a more complicated change which I don’t want to
            // have to figure out how to write and probably more should just require
            // the developer’s intervention.
            VerifyBasicNoFixes(originalCode);
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new OhNoPubImplicitCastForeachVisualBasicCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new OhNoPubImplicitCastForeachVisualBasicAnalyzer();
        }
    }
}
