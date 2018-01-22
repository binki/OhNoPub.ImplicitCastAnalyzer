Analyzers for increasing type safety with looping constructs.

# Rationale

The .net base class library contains two widely used interfaces to represent
enumerables. Those are the untyped
[`IEnumerable`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.ienumerable)
and typed
[`IEnumerable<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1).
Vanilla C# and VB.NET has two constructs which support consuming both typed and untyped
enumerables.
For C#, these are
[`foreach`](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/foreach-in)
and
[LINQ query syntax](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/introduction-to-linq-queries).
For VB.NET, these are
[`For Each…Next`](https://docs.microsoft.com/en-us/dotnet/visual-basic/language-reference/statements/for-each-next-statement)
and
[LINQ query syntax](https://docs.microsoft.com/en-us/dotnet/visual-basic/programming-guide/language-features/linq/introduction-to-linq#StructureOfALINQQuery).

Modern libraries and environments use typed enumerables. This
means that the compiler can figure out an appropriate type when you use
type inference (the `var` keyword in C#, omitted `As` clause in VB.NET). For example:

```csharp
var numbers = new[] { 1, 2, 3, };
foreach (var i in numbers)
{
    int j = i;
}
```

```vbnet
For Each i In { 1, 2, 3 }
    Dim j As Integer = i
Next
```

However, to support consuming untyped enumerables—the only option in .net-1.x—, these
constructs allow specification of a type to cast the element to. For
example, to loop over a winforms `ControlsCollection`, one may:

```csharp
foreach (Control control in Controls)
{
    Control myControl = control;
}
```

```vbnet
For Each control As Control in Controls
    Dim myControl As Control = control
Next
```

This works all fine and well when the programmer is absolutely certain about
the runtime types of values in the iterated collection and it is known that
the API will never change. For example, this is considered acceptable by many
for interacting with legacy untyped winforms APIs. But, this same feature has two issues.
First, it results in a runtime cast with its performance penalty which, with
some planning/design/API improvement, may be unnecessary. Second and, in my opinion,
most importantly, this defers detection of some types of programming errors until
runtime.

For example, the following will compile without error or warning even though any
human glancing at the code could figure out that it is invalid:

```csharp
var numbers = new object[] { 0.1, 0.2, 0.3, };
foreach (int i in numbers)
{
	Console.WriteLine($"i={i}, 2*i={2*i}");
}
```

```vbnet
' Because VB.NET does runtime type coersion, this would run without exception
' if the strings were, e.g., "0.1", "4". However, that is unexpected behavior
' and effectively sidesteps the protections provided by Option Strict On.
For Each i As Integer in { "uhm", "yeah!" }
    Console.WriteLine($"i={i}, 2*i={2*i}")
Next
```

This analyzer introduces a compile-time warning that detects when a runtime cast would
occur. It also provides a codefix to conveniently convert the explicitly named type to
use inference to avoid the implicit runtime cast. This enables one to catch such issues at
compile time prior to even running tests. Additionally, it helps identify code
which could be benefited by moving to generic collections.

See [dotnet/roslyn#14382](https://github.com/dotnet/roslyn/issues/14382).

# TODO:

* LINQ query support:
  ```csharp
  var q =
      from int i from new object[]{ "a", "b", "c", }
	  select 2*i;
  ```

  ```vbnet
  Dim q =
      From i As Integer In { "a", "b", "c" }
	  Select 2*i
  ```
* VB.Net support. VB has basically the same constructs: . Right now,
  this project only minimally supports C#. I do not know if the
  actual analyzer can be generalized to support both C# and VB.Net
  at the same time, but it has basically the same constructs and,
  I assume, the same issues.
