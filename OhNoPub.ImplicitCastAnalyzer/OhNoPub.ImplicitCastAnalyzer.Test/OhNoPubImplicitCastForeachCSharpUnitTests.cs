using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace OhNoPub.ImplicitCastAnalyzer.Test
{
    [TestClass]
    public class OhNoPubImplicitCastForeachCSharpUnitTests : OhNoPubImplicitCastUnitTestsBase
    {
        // No diagnostics expected to show up
        [TestMethod]
        public void TestEmptyCode()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        // No diagnostics expected to show up
        [DataRow("string[]")]
        [DataRow("System.Collections.IEnumerable")]
        [DataRow("System.Collections.Generic.IEnumerable<string>")]
        [DataTestMethod]
        public void TestVarNoCast(string type)
        {
            var test = @"
class MyClass {
    public MyClass(" + type + @" v) {
        foreach (var x in v) {
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        // No diagnostics expected to show up
        [TestMethod]
        public void TestExplicitTypeName_Array()
        {
            var test = @"
class MyClass {
    public MyClass() {
        foreach (string x in new[] { ""a"", ""b"", }) {
        }
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        // No diagnostics expected to show up
        [TestMethod]
        public void TestExplicitTypeName_Enumerable()
        {
            var test = @"
using System.Collections;

class MyClass {
    public MyClass(IEnumerable e) {
        foreach (object x in e) {
        }
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        // No diagnostics expected to show up
        [TestMethod]
        public void TestExplicitTypeName_GenericEnumerable()
        {
            var test = @"
using System.Collections.Generic;

class MyClass {
    public MyClass(IEnumerable e) {
        foreach (string x in e) {
        }
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        // No diagnostics should show up and analyzer should not crash
        [TestMethod]
        public void TestInvalidForEach()
        {
            var test = @"
using System.Collections.Generic;

class MyClass {
    public MyClass() {
        // Intentional error
        foreach (string x in (IEnumerable<string>)null) {
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        // No diagnostics expected to show up
        [TestMethod]
        public void TestStaticCast_Array()
        {
            var test = @"
class MyClass {
    public MyClass() {
        foreach (object x in new[] { ""a"", ""b"", }) {
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        // No diagnostics expected to show up
        [TestMethod]
        public void TestStaticCast_GenericEnumerable()
        {
            var test = @"
class MyClass {
    public MyClass(IEnumerable<object> e) {
        foreach (object x in e) {
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        // No diagnostics expected to show up
        [TestMethod]
        public void TestStaticCast_Enumerable()
        {
            var test = @"
class MyClass {
    public MyClass(IEnumerable e) {
        foreach (object x in e) {
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestForEachImplicitCast_Array()
        {
            var test = @"
class MyClass {
    public MyClass() {
        foreach (int x in new[] { (object)""a"", ""b"", }) {
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = OhNoPubImplicitCastForeachId,
                Message = string.Format(DiagnosticMessage, "object", "int"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 4, 18),
                }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
class MyClass {
    public MyClass() {
        foreach (var x in new[] { (object)""a"", ""b"", }) {
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestForEachImplicitCast_Enumerable()
        {
            var test = @"
using System.Collections;

class MyClass {
    public MyClass(IEnumerable e) {
        foreach (int x in e) {
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = OhNoPubImplicitCastForeachId,
                Message = string.Format(DiagnosticMessage, "object", "int"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 18)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using System.Collections;

class MyClass {
    public MyClass(IEnumerable e) {
        foreach (var x in e) {
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestForEachImplicitCast_GenericEnumerable()
        {
            var test = @"
using System.Collections.Generic;

class MyClass {
    public MyClass(IEnumerable<object> e) {
        foreach (int x in e) {
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = OhNoPubImplicitCastForeachId,
                Message = string.Format(DiagnosticMessage, "object", "int"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 18)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using System.Collections.Generic;

class MyClass {
    public MyClass(IEnumerable<object> e) {
        foreach (var x in e) {
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new OhNoPubImplicitCastForeachCSharpCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new OhNoPubImplicitCastForeachCSharpAnalyzer();
        }
    }
}
