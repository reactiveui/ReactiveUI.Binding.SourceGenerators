# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Zero Tolerance Policy

- **NEVER abandon work halfway through** - if something gets difficult, push through it
- **NEVER use `git stash`** to hide incomplete work - fix the problem directly
- **NEVER give up because a task is complex** - break it down and keep going
- If a tool call is rejected, adapt your approach immediately and continue

## Build & Test Commands

This project uses **Microsoft Testing Platform (MTP)** with the **TUnit** testing framework. Test commands differ significantly from traditional VSTest.

See: https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test?tabs=dotnet-test-with-mtp

### Prerequisites

```powershell
# Check .NET installation (.NET 8.0, 9.0, and 10.0 required)
dotnet --info

# Restore NuGet packages
cd src
dotnet restore ReactiveUI.Binding.SourceGenerators.slnx
```

**Note:** This project uses the modern `.slnx` (XML-based solution file) format instead of the legacy `.sln` format.

### Build Commands

**CRITICAL:** The working folder must be `./src` folder. These commands won't function properly without the correct working folder.

```powershell
# Build the solution
dotnet build ReactiveUI.Binding.SourceGenerators.slnx -c Release

# Build with warnings as errors (includes StyleCop violations)
dotnet build ReactiveUI.Binding.SourceGenerators.slnx -c Release -warnaserror

# Clean the solution
dotnet clean ReactiveUI.Binding.SourceGenerators.slnx
```

### Test Commands (Microsoft Testing Platform)

**CRITICAL:** This repository uses MTP configured in `testconfig.json`. All TUnit-specific arguments must be passed after `--`:

The working folder must be `./src` folder.

**IMPORTANT:**
- Do NOT use `--no-build` flag when running tests. Always build before testing to ensure all code changes are compiled.
- Use `--output Detailed` to see Console.WriteLine output from tests (place BEFORE any `--` separator).

```powershell
# Run all tests in the solution
dotnet test --solution ReactiveUI.Binding.SourceGenerators.slnx -c Release

# Run all tests in a specific project
dotnet test --project tests/ReactiveUI.Binding.Analyzer.Tests/ReactiveUI.Binding.Analyzer.Tests.csproj -c Release
dotnet test --project tests/ReactiveUI.Binding.SourceGenerators.Tests/ReactiveUI.Binding.SourceGenerators.Tests.csproj -c Release
dotnet test --project tests/ReactiveUI.Binding.Tests/ReactiveUI.Binding.Tests.csproj -c Release

# Run a single test method using treenode-filter
dotnet test --project tests/ReactiveUI.Binding.SourceGenerators.Tests/ReactiveUI.Binding.SourceGenerators.Tests.csproj -- --treenode-filter "/*/*/*/MyTestMethod"

# Run all tests in a specific class
dotnet test --project tests/ReactiveUI.Binding.SourceGenerators.Tests/ReactiveUI.Binding.SourceGenerators.Tests.csproj -- --treenode-filter "/*/*/WhenChangedGeneratorTests/*"

# Run tests with code coverage
dotnet test --solution ReactiveUI.Binding.SourceGenerators.slnx -- --coverage --coverage-output-format cobertura
```

### TUnit Treenode-Filter Syntax

The `--treenode-filter` follows the pattern: `/{AssemblyName}/{Namespace}/{ClassName}/{TestMethodName}`

- Single test: `--treenode-filter "/*/*/*/MyTestMethod"`
- All tests in class: `--treenode-filter "/*/*/MyClassName/*"`
- Use single asterisks (`*`) to match segments.

### Key Configuration Files

- `src/ReactiveUI.Binding.SourceGenerators.slnx` - Modern XML-based solution file
- `src/testconfig.json` - Configures test execution and code coverage
- `src/Directory.Build.props` - Common build properties, package metadata
- `src/Directory.Packages.props` - Central package management
- `src/Directory.Build.targets` - Build targets

### Snapshot Testing with Verify

- Generator tests use **Verify.SourceGenerators** for snapshot testing
- Snapshots stored as `*.verified.cs` files alongside test classes
- To accept new/changed snapshots:
  1. Enable `VerifierSettings.AutoVerify()` in `ModuleInitializer.cs`
  2. Run tests to accept all snapshots
  3. Disable `VerifierSettings.AutoVerify()` after accepting
  4. Re-run tests to confirm they pass without AutoVerify

## Architecture Overview

### What This Project Does

ReactiveUI.Binding.SourceGenerators is an **incremental source generator** that replaces ReactiveUI's runtime expression tree analysis with compile-time code generation for property observation and binding. It eliminates runtime reflection, is fully AOT/trimming safe, and supports all ReactiveUI platform notification mechanisms.

### Project Structure

```
src/
├── ReactiveUI.Binding/                          # Runtime library (net8.0;net9.0;net10.0;net462-net481)
│   └── Interfaces/                              # ICreatesObservableForProperty, IObservedChange, etc.
│
├── ReactiveUI.Binding.SourceGenerators/         # Source generator (netstandard2.0)
│   ├── BindingGenerator.cs                      # [Generator] IIncrementalGenerator entry point
│   ├── Constants.cs                             # API stub text, metadata names (linked to Analyzer)
│   ├── DiagnosticWarnings.cs                    # Diagnostic descriptors (linked to Analyzer)
│   ├── RoslynHelpers.cs                         # Syntax predicates for CreateSyntaxProvider
│   ├── MetadataExtractor.cs                     # Semantic model → POCO extraction
│   ├── Models/                                  # Value-equatable pipeline POCOs
│   │   ├── EquatableArray.cs
│   │   ├── ClassBindingInfo.cs                  # Type-level: notification mechanism flags
│   │   ├── InvocationInfo.cs                    # Per-call-site: WhenChanged/WhenChanging
│   │   └── BindingInvocationInfo.cs             # Per-call-site: BindOneWay/BindTwoWay
│   ├── Generators/                              # Per-kind fallback generators (Pipeline A)
│   │   ├── ReactiveObjectBindingGenerator.cs    # IReactiveObject (affinity 24)
│   │   ├── INPCBindingGenerator.cs              # INotifyPropertyChanged (affinity 21)
│   │   ├── WpfBindingGenerator.cs               # WPF DependencyObject (affinity 20)
│   │   ├── WinUIBindingGenerator.cs             # WinUI DependencyObject (affinity 22)
│   │   ├── KVOBindingGenerator.cs               # Apple KVO/NSObject (affinity 25)
│   │   ├── WinFormsBindingGenerator.cs          # WinForms Component (affinity 23)
│   │   ├── AndroidBindingGenerator.cs           # Android View (affinity 19)
│   │   └── RegistrationGenerator.cs             # Consolidates all → [ModuleInitializer]
│   ├── Invocations/                             # Per-invocation generators (Pipeline B)
│   │   ├── WhenChangedInvocationGenerator.cs    # After-change observation
│   │   ├── WhenChangingInvocationGenerator.cs   # Before-change observation
│   │   ├── BindOneWayInvocationGenerator.cs     # One-way binding
│   │   ├── BindTwoWayInvocationGenerator.cs     # Two-way binding
│   │   └── WhenAnyValueInvocationGenerator.cs   # WhenAnyValue compat shim
│   └── CodeGeneration/
│       └── CodeGenerator.cs                     # StringBuilder-based code generation
│
├── ReactiveUI.Binding.Analyzer/                 # Roslyn analyzer (netstandard2.0)
│   └── Analyzers/
│       ├── BindingInvocationAnalyzer.cs          # RXUIBIND001, 003, 004, 005
│       └── TypeAnalyzer.cs                       # RXUIBIND002
│
└── tests/
    ├── ReactiveUI.Binding.SourceGenerators.Tests/ # Generator snapshot tests
    ├── ReactiveUI.Binding.Analyzer.Tests/         # Analyzer diagnostic tests
    └── ReactiveUI.Binding.Tests/                  # Runtime library tests
```

### Two Pipelines

**Pipeline A (Type Detection)**: Scans classes with base lists → builds `ClassBindingInfo` POCOs with boolean flags for each notification mechanism (IReactiveObject, INPC, WPF DP, WinUI DP, KVO, WinForms, Android). Per-kind generators filter from this shared pipeline. Consolidates into a single `[ModuleInitializer]` registration.

**Pipeline B (Invocation Detection)**: Scans method invocations (`WhenChanged`, `WhenChanging`, `BindOneWay`, `BindTwoWay`, `WhenAnyValue`) → extracts lambda property paths → generates optimized per-call-site observation/binding code. Uses **CallerFilePath + CallerLineNumber dispatch**: API stubs capture caller info, generated dispatch table routes to compile-time generated methods.

### API Pattern

```csharp
// User writes:
var obs = vm.WhenChanged(x => x.Name);

// Generator emits API stub (PostInitializationOutput) with CallerInfo dispatch:
public static IObservable<TReturn> WhenChanged<TObj, TReturn>(
    this TObj obj, Expression<Func<TObj, TReturn>> property,
    [CallerFilePath] string callerFilePath = "",
    [CallerLineNumber] int callerLineNumber = 0) where TObj : class
{
    if (__GeneratedBindingDispatcher.TryGetWhenChanged(callerFilePath, callerLineNumber, obj, out var result))
        return (IObservable<TReturn>)result!;
    throw new InvalidOperationException("...");  // Runtime fallback TBD
}

// Generator emits per-invocation optimized method:
private static IObservable<string> __WhenChanged_0(MyViewModel obj)
{
    return Observable.Create<string>(observer => { ... PropertyChanged subscription ... })
        .StartWith(obj.Name);
}
```

### WhenChanged vs WhenChanging

| API | Interface | Event | Timing |
|-----|-----------|-------|--------|
| `WhenChanged` | `INotifyPropertyChanged` | `PropertyChanged` | After value changes |
| `WhenChanging` | `INotifyPropertyChanging` | `PropertyChanging` | Before value changes |

Not all platforms support before-change notifications (WPF DP, WinUI DP, WinForms, Android do not). The analyzer reports RXUIBIND004 when `WhenChanging` targets an unsupported platform type.

### Diagnostic IDs

| ID | Severity | Description |
|----|----------|-------------|
| RXUIBIND001 | Info | Expression must be inline lambda for compile-time optimization |
| RXUIBIND002 | Warning | Type has no observable properties |
| RXUIBIND003 | Warning | Expression contains private/protected member |
| RXUIBIND004 | Warning | Type does not support before-change notifications |
| RXUIBIND005 | Info | Source type implements INotifyDataErrorInfo; validation binding requires runtime engine |

## Code Style & Quality Requirements

**CRITICAL:** All code must comply with ReactiveUI contribution guidelines: https://www.reactiveui.net/contribute/index.html

### Style Enforcement

- EditorConfig rules (`.editorconfig`)
- StyleCop Analyzers - builds fail on violations
- Roslynator Analyzers - additional code quality rules
- **All public APIs require XML documentation comments**
- **RS2008**: Analyzer release tracking enabled (`AnalyzerReleases.Shipped.md` / `AnalyzerReleases.Unshipped.md`)

### C# Style Rules

- **Braces:** Allman style
- **Indentation:** 4 spaces, no tabs
- **Fields:** `_camelCase` for private/internal
- **Visibility:** Always explicit, visibility first modifier
- **Namespaces:** File-scoped preferred
- **Modern C#:** Nullable reference types, pattern matching, records, init setters
- **netstandard2.0 targets:** Use `IsExternalInit.cs` polyfill for records; avoid APIs not available in netstandard2.0 (e.g., use `if (x is null) throw new System.ArgumentNullException(...)` instead of `ArgumentNullException.ThrowIfNull()`)

## Key Architectural Patterns

### Value-Equatable Models (Critical for Caching)

All pipeline models are `sealed record` types with value equality. NEVER include `ISymbol`, `SyntaxNode`, or `Location` in pipeline outputs. Use `EquatableArray<T>` for array equality. Extract strings from symbols using `ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)`.

### Code Generation Strategy

- Uses StringBuilder, NOT SyntaxFactory
- Generated code emitted as C# source via `context.AddSource()`
- `#pragma warning disable` at top of generated files
- All generated types use `[Microsoft.CodeAnalysis.Embedded]` attribute

### Two-Layer Language Version Constraint

There are **two distinct C# language contexts** in this project:

**Generator source code** (the `.cs` files in `ReactiveUI.Binding.SourceGenerators/`):
- Compiled with the **latest** C# language version (currently C# 12)
- Can freely use raw string literals (`$$"""`), file-scoped namespaces, pattern matching (`is not`), records, switch expressions, etc.
- Must target **netstandard2.0** (Roslyn requirement), but the SDK/language version is latest

**Generated output** (the strings emitted by the generator into user projects):
- Must be **C# 7.3 compatible** — user projects may target older frameworks
- Must follow the **ReactiveUI coding standard** (https://www.reactiveui.net/contribute/index.html) as closely as possible within C# 7.3 constraints
- Key rules for generated output:
  - **Allman-style braces** — each brace on a new line
  - **4-space indentation** — no tabs
  - **Properly formatted multi-line code** — no single-line walls of text for non-trivial expressions
  - **Explicit visibility modifiers** — visibility first (e.g. `private static`, not `static private`)
  - Method bodies indented consistently at 12 spaces (namespace=0, class=4, member=8, body=12)
- C# 7.3 restrictions for generated output — do NOT use:
  - `is not`, `and`, `or` pattern combinators (C# 9)
  - `??=` null-coalescing assignment (C# 8)
  - Switch expressions (C# 8)
  - `required` members (C# 11)
  - Raw string literals (C# 11)
  - File-scoped namespaces (C# 10)
  - `init` setters (C# 9)
- Generated output CAN use `#nullable enable` and `object?` (pragma-based, works in any C# version)

### Analyzer Separation (Roslyn Best Practice)

- Generator does NOT report diagnostics
- Separate analyzer project reports all RXUIBIND diagnostics
- `DiagnosticWarnings.cs` and `Constants.cs` are linked from generator to analyzer via `<Compile Include="..." Link="..." />`

### Shared File Linking

The analyzer project links shared files from the generator project:
```xml
<Compile Include="..\ReactiveUI.Binding.SourceGenerators\DiagnosticWarnings.cs" Link="DiagnosticWarnings.cs" />
<Compile Include="..\ReactiveUI.Binding.SourceGenerators\Constants.cs" Link="Constants.cs" />
```

### ConditionalWeakTable Symbol Caching

`MetadataExtractor.cs` uses `ConditionalWeakTable<Compilation, WellKnownSymbolsBox>` to cache resolved well-known type symbols per compilation, avoiding repeated `GetTypeByMetadataName` calls.

## Common Tasks

### Adding a New Generator Pipeline

1. Create value-equatable POCO in `Models/`
2. Add syntax predicate to `RoslynHelpers.cs`
3. Add extraction logic to `MetadataExtractor.cs`
4. Create invocation generator in `Invocations/` with `Register()` method
5. Wire into `BindingGenerator.cs` `Initialize()`
6. Add code generation to `CodeGeneration/CodeGenerator.cs`
7. Add snapshot test in generator test project
8. Accept snapshots using `VerifierSettings.AutoVerify()` trick

### Adding a New Analyzer Diagnostic

1. Add descriptor to `DiagnosticWarnings.cs` (shared file)
2. Update `AnalyzerReleases.Unshipped.md` in both projects
3. Create/update analyzer in `ReactiveUI.Binding.Analyzer/Analyzers/`
4. Add tests in `ReactiveUI.Binding.Analyzer.Tests/`
5. Use `AnalyzerTestHelper.GetDiagnosticsAsync<T>()` for testing

### Accepting Snapshot Changes

1. Enable `VerifierSettings.AutoVerify()` in `ModuleInitializer.cs`
2. Run tests: `dotnet test --project tests/ReactiveUI.Binding.SourceGenerators.Tests/... -c Release`
3. Remove `VerifierSettings.AutoVerify()` from `ModuleInitializer.cs`
4. Re-run tests to confirm they pass

## What to Avoid

- **ISymbol/SyntaxNode in pipeline outputs** - breaks incremental caching
- **Runtime reflection** in generated code - breaks AOT compatibility
- **SyntaxFactory for code generation** - use StringBuilder instead
- **Diagnostics in generator** - use separate analyzer project
- **LINQ in hot paths** - use manual loops (Roslyn convention)
- **Non-value-equatable models** in pipeline - breaks caching
- **APIs unavailable in netstandard2.0** in generator/analyzer projects

## Important Notes

- **Required .NET SDKs:** .NET 8.0, 9.0, and 10.0
- **Generator + Analyzer targets:** netstandard2.0 (Roslyn requirement)
- **Runtime library targets:** net8.0;net9.0;net10.0;net462;net472;net481
- **No shallow clones:** Repository requires full clone for Nerdbank.GitVersioning
- **PackBuildOutputs target:** Generator .csproj packages both generator and analyzer DLLs into `analyzers/dotnet/cs`

**Philosophy:** Generate zero-reflection, AOT-compatible property observation and binding code at compile-time. Support all ReactiveUI platform notification mechanisms. Fall back to runtime expression analysis only when compile-time analysis is not possible.
