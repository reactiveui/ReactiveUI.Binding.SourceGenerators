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
  1. Enable `VerifierSettings.AutoVerify()` in `AssemblySetup.cs`
  2. Run tests to accept all snapshots
  3. Disable `VerifierSettings.AutoVerify()` after accepting
  4. Re-run tests to confirm they pass without AutoVerify

### Generator Test Language Versions (Critical)

Generator tests use a **two-tier language version** strategy to verify generated output compiles under C# 7.3 (the minimum supported version for consumer projects):

- **Default: C# 7.3** — `TestHelper.CreateCompilation()` and `RunGenerator()` default to `LanguageVersion.CSharp7_3`. This ensures generated output contains no C# 8+ syntax (no nullable reference type annotations, no `static` lambdas, no `#nullable enable`).
- **CallerArgumentExpression tests: explicit C# 10** — Tests that verify `CallerArgumentExpression`-based dispatch (the primary dispatch mechanism for C# 10+ projects) must pass `LanguageVersion.CSharp10` explicitly. These are the majority of snapshot tests.
- **CallerFilePath fallback tests: explicit C# 7.3** — Tests that verify `CallerFilePath + CallerLineNumber` dispatch (the fallback for pre-C# 10 projects) pass `LanguageVersion.CSharp7_3` explicitly to document intent, even though it matches the default.
- **Edge case tests** (`RunGenerator` without snapshot verification) — These use the C# 7.3 default. They verify the generator doesn't crash on invalid lambdas and produces no dispatch code. They don't call `CompilationSucceeds()` since their test source may contain C# 8+ features that are only diagnostically invalid under C# 7.3.
- **Runtime execution tests** — These use `LanguageVersion.CSharp10` because their inline source uses C# 8+ features and they call `CompilationSucceeds()`.

**When adding new generator tests:**
1. If the test verifies CallerArgumentExpression dispatch → pass `LanguageVersion.CSharp10`
2. If the test verifies CallerFilePath fallback dispatch → pass `LanguageVersion.CSharp7_3`
3. If the test verifies the generator skips invalid input → use the default (no parameter)
4. If the test compiles and loads the generated assembly → pass `LanguageVersion.CSharp10`

### Code Coverage

Code coverage uses **Microsoft.Testing.Extensions.CodeCoverage** configured in `src/testconfig.json`. Coverage is collected for production assemblies only (test projects and TestModels are excluded).

```powershell
# Run tests with code coverage (from src/ folder)
dotnet test --solution ReactiveUI.Binding.SourceGenerators.slnx -c Release -- --coverage --coverage-output-format cobertura

# Generate HTML report using ReportGenerator (install if needed: dotnet tool install -g dotnet-reportgenerator-globaltool)
# Find all cobertura files and generate report to /tmp/<folder>
reportgenerator \
  -reports:"tests/**/TestResults/**/*.cobertura.xml" \
  -targetdir:/tmp/code_coverage \
  -reporttypes:"Html;TextSummary"

# View the text summary
cat /tmp/code_coverage/Summary.txt

# Open HTML report in browser
xdg-open /tmp/code_coverage/index.html   # Linux
open /tmp/code_coverage/index.html        # macOS
```

**Key configuration** (`src/testconfig.json`):
- `modulePaths.include`: `ReactiveUI\\.Binding\\..*` — covers all production assemblies
- `modulePaths.exclude`: `.*Tests.*`, `.*TestRunner.*`, `.*TestModels.*` — excludes test/runner/model assemblies
- `skipAutoProperties: true` — auto-properties excluded from coverage metrics

**Tips:**
- Always clean `bin/` and `obj/` folders before coverage runs to avoid stale results
- The `ReactiveUI.Binding.GeneratedCode.TestModels` assembly has `[assembly: ExcludeFromCodeCoverage]` so it won't appear in reports even though its module path matches the include pattern
- `ReactiveUI.Binding.Reactive` carries the same attribute (applied via `<AssemblyAttribute>` in its .csproj). It is a recompilation of `ReactiveUI.Binding.Shared`, so every line in it is a line `ReactiveUI.Binding` already covers — only the scheduler binding and the namespace differ. Cover shared code through the lean leaf; the `modulePaths` exclusions do not reach an assembly that is only present as a copied reference
- `DiagnosticWarnings.cs` coverage appears as 0% in `ReactiveUI.Binding.SourceGenerators` — this is a linked-file artifact; the code is actually tested via the `ReactiveUI.Binding.Analyzer` assembly
- Put coverage reports in `/tmp/` to avoid accidentally committing them

## Architecture Overview

### What This Project Does

ReactiveUI.Binding.SourceGenerators is an **incremental source generator** that replaces ReactiveUI's runtime expression tree analysis with compile-time code generation for property observation and binding. It eliminates runtime reflection, is fully AOT/trimming safe, and supports all ReactiveUI platform notification mechanisms.

### Project Structure

```
src/
├── ReactiveShim.props                           # The lean/.Reactive seam (REACTIVE_SHIM + ISequencer alias)
│
├── ReactiveUI.Binding.Shared/                   # The runtime library source; compiled by BOTH leaves below
│   ├── Interfaces/                              # ICreatesObservableForProperty, IObservedChange, etc.
│   ├── Mixins/                                  # ReactiveUIBindingExtensions, ReactiveSchedulerExtensions
│   ├── Observables/                             # Hand-rolled observables and disposables
│   └── View/                                    # ViewLocator, DefaultViewLocator, IViewFor<T>, attributes
│
├── ReactiveUI.Binding/                          # Lean leaf (net8.0;net9.0;net10.0;net462-net481)
├── ReactiveUI.Binding.Reactive/                 # System.Reactive leaf: same source, REACTIVE_SHIM
│
├── build/                                       # SkipMakePriOnNonWindows.targets (see below)
├── ReactiveUI.Binding.Platform.Shared/          # Private primitives for the platform observers
├── ReactiveUI.Binding.Wpf.Shared/               # Each platform's source, compiled by both its leaves
├── ReactiveUI.Binding.Wpf/                      # Lean platform leaf
├── ReactiveUI.Binding.Wpf.Reactive/             # System.Reactive platform leaf
│                                                # ...and the same triple for WinForms and Maui
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
│   │   ├── BindingInvocationInfo.cs             # Per-call-site: BindOneWay/BindTwoWay
│   │   ├── PropertyPathSegment.cs               # Per-segment: name, types, and how its declaring type notifies
│   │   ├── ObservablePropertyInfo.cs            # Per-property: DP field and change-event participation
│   │   └── ViewRegistrationInfo.cs              # Per-IViewFor<T>: view dispatch mapping
│   ├── Plugins/                                 # Mechanism selection (Pipeline A)
│   │   ├── ObservationPluginRegistry.cs         # Highest-affinity plugin that reaches a given property
│   │   ├── ObservedProperties.cs                # Whether one property participates in a type's mechanism
│   │   └── Observation/                         # One plugin per mechanism, scored from BindingAffinity
│   │       ├── KVOObservationPlugin.cs          # Apple KVO/NSObject (Kvo, 15)
│   │       ├── ReactiveObjectObservationPlugin.cs # IReactiveObject (ExactType, 10)
│   │       ├── WinFormsObservationPlugin.cs     # WinForms Component (WinFormsEvent, 8)
│   │       ├── WinUIObservationPlugin.cs        # WinUI DependencyObject (WinUiDependencyObject, 6)
│   │       ├── INPCObservationPlugin.cs         # INotifyPropertyChanged (Explicit, 5)
│   │       ├── AndroidObservationPlugin.cs      # Android View (Explicit, 5)
│   │       ├── WpfObservationPlugin.cs          # WPF DependencyObject (WpfDependencyObject, 4)
│   │       ├── NotifyPropertyObservationPlugin.cs # Base for the INPC-watching plugins above
│   │       └── NotifyPropertyEmitter.cs         # The observation those plugins all emit
│   ├── Generators/                              # Whole-compilation outputs
│   │   ├── RegistrationGenerator.cs             # Consolidates all → [ModuleInitializer]
│   │   ├── ObservationHelperGenerator.cs        # Declares the KVO/WinUI helper classes, once per compilation
│   │   └── ViewLocatorDispatchGenerator.cs      # IViewFor<T> → AOT view dispatch (Pipeline C)
│   ├── Invocations/                             # Per-invocation generators (Pipeline B)
│   │   ├── WhenChangedInvocationGenerator.cs    # After-change observation
│   │   ├── WhenChangingInvocationGenerator.cs   # Before-change observation
│   │   ├── BindOneWayInvocationGenerator.cs     # One-way binding
│   │   ├── BindTwoWayInvocationGenerator.cs     # Two-way binding
│   │   └── WhenAnyValueInvocationGenerator.cs   # WhenAnyValue compat shim
│   ├── Helpers/                                 # Extraction and validation helpers
│   │   ├── ViewRegistrationExtractor.cs         # IViewFor<T> → ViewRegistrationInfo extraction
│   │   └── ...                                  # ExtractorValidation, SymbolHelpers, etc.
│   └── CodeGeneration/
│       ├── CodeGenerator.cs                     # StringBuilder-based code generation
│       ├── PooledBuilder.cs                     # Per-thread StringBuilder pool for whole files
│       ├── PooledStringBuilder.cs               # char[]-backed builder for generated fragments
│       └── RuntimeFlavourRewriter.cs            # Retargets output onto the .Reactive package
│
├── ReactiveUI.Binding.Analyzer/                 # Roslyn analyzer (netstandard2.0)
│   └── Analyzers/
│       ├── BindingInvocationAnalyzer.cs          # RXUIBIND001, 003, 004, 005, 006, 007, 008
│       ├── DispatchReachAnalyzer.cs              # RXUIBIND009
│       └── TypeAnalyzer.cs                       # RXUIBIND002
│
├── benchmarks/
│   ├── ReactiveUI.Binding.Benchmarks/            # Runtime binding benchmarks
│   └── ReactiveUI.Binding.Generator.Benchmarks/  # Generation-pass benchmarks over a corpus
│
└── tests/
    ├── ReactiveUI.Binding.SourceGenerators.Tests/ # Generator snapshot tests
    ├── ReactiveUI.Binding.Analyzer.Tests/         # Analyzer diagnostic tests
    └── ReactiveUI.Binding.Tests/                  # Runtime library tests
```

### Three Pipelines

**Pipeline A (Type Detection)**: Scans classes with base lists → builds `ClassBindingInfo` POCOs with boolean flags for each notification mechanism (IReactiveObject, INPC, WPF DP, WinUI DP, KVO, WinForms, Android) and a per-property record of which of them each declared property actually participates in. Consolidates into a single `[ModuleInitializer]` registration.

Affinity values are the shared `BindingAffinity` scores the runtime library declares (`Fallback = 1` … `Kvo = 15`), the same numbers ReactiveUI's own plugins return, so a user-registered plugin and a generated one rank on one scale.

### Mechanisms Travel With the Property Path

A type advertises a mechanism; a *property* participates in it or does not, and each link of a chain is declared
by its own type. So `PropertyPathSegment` carries its declaring type's `ClassBindingInfo`, and plugin selection
takes the property name: a dependency object's plain CLR property and a component's property with no
`{Name}Changed` event fall through to the next mechanism, exactly as a zero affinity does at runtime. A property
the type does not declare - an inherited one - is unknown rather than absent and stays observable.

The mechanism is captured during extraction, which already holds the property symbol, rather than looked up from
the detected-type set afterwards. That is a performance constraint, not a preference: binding one of these
invocations is the single largest allocation in a generation pass (extension-method overload resolution and
generic type inference dominate the `GcVerbose` trace), so a second semantic pass over the same call sites is
not affordable. It also means a type from a *referenced* assembly is observed correctly even though the
declaration scan never sees it.

**Pipeline B (Invocation Detection)**: Scans method invocations (`WhenChanged`, `WhenChanging`, `BindOneWay`, `BindTwoWay`, `WhenAnyValue`) → extracts lambda property paths → generates optimized per-call-site observation/binding code. Uses **CallerFilePath + CallerLineNumber dispatch**: API stubs capture caller info, generated dispatch table routes to compile-time generated methods.

**Pipeline C (View Dispatch)**: Scans classes implementing `IViewFor<T>` → extracts `ViewRegistrationInfo` POCOs (VM FQN, View FQN, constructor availability, `[ViewContract]` contract, `[SingleInstanceView]` flag) → generates `ViewDispatch.g.cs` with a type-switch dispatch function. Supports contract-based multi-view resolution (contract checks emitted before default), singleton caching via `Interlocked.CompareExchange`, and 3-tier resolution (service locator → direct construction → null). Views can be excluded with `[ExcludeFromViewRegistration]`.

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

### Where the Dispatch Overloads Live

The generated concrete overloads have to beat the runtime stub at the call site, and they have to do it
without two generator-running assemblies seeing each other's copies. Extension-method lookup decides both:
it walks the enclosing namespaces of the call site from the inside out and **stops at the first level that
offers any candidate**, and a namespace brought in by a `using` (file-level or global) is only ever consulted
at the outermost level.

From C# 10 the overloads go in the **consumer's own root namespace** (`build_property.RootNamespace`,
rendered as identifier segments so a project name like `My-App` still yields a legal namespace), plus a
generated `global using` of it. Two routes, two jobs:

- the root namespace catches call sites nested under it, including a consumer whose own code sits under
  `ReactiveUI.Binding.*` — those reach the stub at an enclosing level, so a `global using` alone never gets
  looked at and the call falls through to the stub's runtime throw;
- the `global using` catches files declared outside the root namespace.

Either way the concrete overload lands in the same candidate set as the generic stub and wins outright, because
a non-generic candidate beats a generic one. A `global using` is scoped to the compilation that declares it and
is never exported, which is what keeps one assembly's overloads out of another's lookup — the case that matters
when the two are joined by `InternalsVisibleTo`, since identical overloads visible from both make every matching
call site ambiguous (CS0121). When the build exposes no root namespace, the fallback is
`ReactiveUI.Binding.Generated.<assembly>`.

The residual risk is two assemblies that **share a root namespace**, grant each other `InternalsVisibleTo`, and
both generate a dispatch for the same concrete types — those calls are ambiguous again. All three have to
coincide; a project and its test project have distinct root namespaces and are unaffected.

Below C# 10 there are no global usings, so there is nothing to scope a namespace with. The overloads stay in
`ReactiveUI.Binding`, where the import every consumer already has reaches them from any file — **unless the
assembly grants `InternalsVisibleTo`**, which is the only way another assembly can see them at all. Those
assemblies emit into their own root namespace instead, which nobody else's code sits under. The cost is that a
file declared outside the root namespace falls back to the runtime path; the alternative for those assemblies
is not universal reach but a build that does not compile (CS0121). With no root namespace to move to, the
shared namespace is kept — nowhere else would be reachable.

### Recognising the Class a Stub Was Declared In

Every extractor asks the same question of a call it has matched by name: is the method one of ours? It answers
by the declaring type's name, so the answer has to survive **how the API was declared**.
`ReactiveSchedulerExtensions` declares its whole surface as extension blocks, and an extension block's members
belong to a synthesized grouping type nested inside the static class, not to the class itself. The grouping type
has no name a consumer could write — empty when read from source, `<>E__N` when read from metadata — so
`ExtractorValidation.IsRecognizedExtensionClass` takes the symbol and reaches one level out when it finds one.

**Which of the two shapes a call site resolves to is decided by the Roslyn that loads the generator, not by
anything in the consumer's project.** The same source, the same references and the same `LangVersion` resolve
to the static class on one compiler and to the grouping type on another. Rejecting either shape is silent:
those call sites produce no dispatch, the runtime stub throws, and every other call site in the file still
generates, so the build stays green and only the affected bindings go missing.

There is no single version to code against. The generator is loaded by whatever compiler the consumer's SDK or
Visual Studio ships, and that spans the whole installed base at once - an unchanged project produces one shape
on one machine and the other shape on the next. **Accept every shape rather than the one this repository
happens to build with**, and never narrow a symbol test to what the current compiler returns.

That cuts against the suite, which pins a single `Microsoft.CodeAnalysis.CSharp` package version and so
exercises exactly one of those compilers. Passing tests say the generator works on that one, and say nothing
about the rest. Where behaviour could turn on the host compiler, verify against several: run the generator from
a throwaway single-file app that pins a different `Microsoft.CodeAnalysis.CSharp` version, over a real
project's sources, and compare the emitted files. When a call site generates in the suite but not in a real
build, suspect this first and check a real build's `EmitCompilerGeneratedFiles` output rather than adding
more tests that share the suite's compiler.

### Where This Engine Parts Company With ReactiveUI's

The generator is a replacement for `PropertyBinderImplementation`, so its behaviour is measured against that
engine. Two of its inputs are deliberately not offered, and one behaviour is deliberately cheaper.

**`TriggerUpdate` and `signalViewUpdate` are not offered.** ReactiveUI's `Bind` takes both: the first chooses
which side wins the first emission, the second replaces the view's own change stream with a caller-supplied
one. Neither has a generated overload, so asking for one does not compile - the divergence announces itself
at the call site rather than at run time. They are omitted because the generator resolves a binding from the
two lambdas alone; an overload taking a runtime stream would have to fall back to the reflection engine for
exactly the call sites this library exists to remove from it.

**Binding faults follow ReactiveUI's contract exactly.** A write that faults is logged against the bound
expression, and rethrown as a `TargetInvocationException` only when it carries an inner exception. This is
parity rather than divergence, and it is not optional: a setter that throws on the notifying thread has no
caller stack to surface on, so swallowing it loses the failure entirely.

**Hooks are consulted only when one is registered.** ReactiveUI asks the service locator for
`IPropertyBindingHook` on every binding it creates. `BindingHooks.Any` is tested first here, so an
application that registers none - which is nearly all of them - pays nothing, and one that registers a hook
gets the same veto. That is the "better" half of the divergence: same outcome, no cost for the common case.

**A registered plugin still outranks the generated observation.** The generator picks a mechanism from the
types it can see at compile time, but `ICreatesObservableForProperty` is registered at run time and the
highest affinity wins - which is how ReactiveUI resolves the observation behind `Bind` and `OneWayBind`, not
just behind `WhenChanged`. The choice is made per observed link rather than per binding:
`ObservationAffinityChecker.FindHigherAffinityPlugin` is asked for a registration outranking the affinity
that link was generated from, and the link observes through `PluginPropertyObservable` when one wins and
through the generated mechanism when none does. A two-way binding observes both sides, so either side's
registration takes that side.

**The registration drives the observation without an expression engine.** `PluginPropertyObservable`
subscribes to the registration for *when* the property changed and reads the value through the accessor the
generator emitted, so the honouring of a plugin costs no reflection. Everything the plugin needs is fixed at
compile time - the declaring type, the property name, the getter, and a lambda the compiler turns into member
tokens - which is what keeps a consumer publishing ahead-of-time free of trim and AOT warnings. Nothing on a
generated path reaches the runtime expression engine; routing a whole binding to it instead would put
`[RequiresUnreferencedCode]` back on every call site.

Leaving the override out is not a divergence anyone could see: the registration would apply to `WhenChanged`
and silently not to a binding of the same property. The check has to be on the path of every binding, so the
registered set is resolved once and kept rather than re-read from the locator per call - re-reading cost
~141 B and ~1.2 us per binding created, which a view full of bindings pays for repeatedly. `Refresh()`
drops the cache for a host that registers a plugin after its first binding.

### Operators Come From Primitives

`ReactiveUI.Binding.Shared/Observables/` holds only the observables this library's *domain* owns - the ones
that turn a property notification into an `IObservedChange`. Anything that is a general reactive operator
belongs to `ReactiveUI.Primitives`, which is already referenced, and is used from there rather than
hand-rolled again here.

Primitives exposes both a concrete type per operator under `ReactiveUI.Primitives.Advanced` and a factory or
extension that returns it. **Prefer the concrete type** - `new LeadSignal<T>(source, value)` over
`source.StartWith(value)` - so the generated and runtime code says exactly what it builds. Reach for the
factory only where it is the better path: `Signal.Never<T>()`, `Signal.Empty<T>()` and `Signal.Return<T>(v)`
hand back cached singletons or a specialised immediate form that the public constructors cannot express.

Two traps when naming these:

- **`ReactiveUI.Primitives.Core` does not shift.** It is one assembly shared by both runtime flavours rather
  than one recompiled per leaf, so the types in it - `ImmutableNeverSignal<T>`, `ImmediateReturnSignal<T>` -
  stay in `ReactiveUI.Primitives.Advanced` for the `.Reactive` leaf too. That leaf imports the unshifted
  namespace alongside its shifted one; the two declare disjoint types, which is what lets the lean leaf merge
  them already.
- **Generated code calls extension classes statically**, as
  `global::ReactiveUI.Primitives.LinqExtensions.ObserveOn(source, scheduler)`, so no import has to be emitted
  and `RuntimeFlavourRewriter` can retarget the whole path onto the `.Reactive` flavour.

What stays in `Observables/` is decided by whether the type is a general operator or something this domain
fuses:

| Type | Why it lives here |
|------|-------------------|
| `PropertyObservable`, `NotifyPropertyChangedObservable`, `PropertyChangingObservable` | Turn a property notification into an `IObservedChange`. The concept is this library's, so no general-purpose equivalent exists. |
| `EventObservable` | Fuses add/remove handler, the getter, `StartWith` and `DistinctUntilChanged` into **one** allocation. Assembling the same behaviour from `Signal.FromEventPattern` and three operators costs four. |
| `CombineLatestObservable` | A façade over `LinqExtensions.CombineLatest` that builds nothing itself, so every call site names one thing. |

**A fused type is not replaced by a chain of general operators.** The point of this library is the allocation
count, so a swap that trades one object for four is a regression however much code it removes. Measure before
assuming a replacement is free.

### One Body Per Reachable Branch

The two dispatch mechanisms differ in what they can tell apart, and the emitted bodies follow. File-and-line
dispatch keys on the call site, so every call site is distinct and each needs its own binding method.
Expression-text dispatch keys on the two lambdas as written, so call sites that spell them the same way all
produce the same condition — the first wins, and any later one is unreachable while still dragging a binding
method along. Binding the same pair of properties from more than one place is ordinary rather than exotic, so
that dead weight scales with the consumer.

`BindingEmitterHelpers.Generate` therefore collapses a group to one call site per distinct pair of expression
texts, but **only under expression-text dispatch**. Collapsing is sound there because the group already fixes
both types, so a shared pair of expressions means a shared pair of property paths and an identical body. Doing
the same under file-and-line dispatch would strand every collapsed call site on the stub's runtime throw.

### Generating for the Lean or the .Reactive Runtime

The two runtime packages share **no type names** — everything the lean one puts in `ReactiveUI.Binding.*`, the
other puts in `ReactiveUI.Binding.Reactive.*`, and the scheduler abstraction differs outright
(`ISequencer` vs `IScheduler`). Output written for one does not compile against the other at all, so the
generator branches on which package the consumer actually references, detected by looking up the stub class.

The emitters write the lean names throughout, and `RuntimeFlavourRewriter` retargets the finished text when the
consumer is on the System.Reactive package. It runs on the finished text rather than being threaded through the
emitters because most generated bodies are **non-interpolated** raw string literals whose braces are the braces
of the generated code — making them interpolated to inject a namespace would mean escaping every one.

The shift is anchored on the names the runtime namespace actually declares, read from the referenced assembly
rather than listed in the generator. That way it cannot go stale as the runtime grows, and it will not touch a
consumer type that merely happens to sit under `ReactiveUI.Binding` — which is exactly what a consumer whose own
root namespace starts that way would otherwise hit.

Every generated file goes out through `CodeGeneratorHelpers.AddGeneratedSource` so no emitter can forget the
retargeting. Forgetting would only ever show up as generated code that does not compile, for consumers of the
package the test suite reaches least. `ReactiveRuntimeFlavourTests` sweeps **every** shared scenario against the
System.Reactive package for that reason — one unshifted type name in one emitter is enough to break every
consumer on it, and no representative subset can be trusted to reach it.

### Matching the Stub's Parameter List

The concrete overload only beats the generic stub once their parameter lists match — a non-generic candidate is
preferred over a generic one, but that tie-break needs the two to be otherwise indistinguishable. A shorter
parameter list leaves both merely applicable, neither better, and **every matching call site fails with CS0121**.

The stub declares its `[CallerArgumentExpression]` parameters wherever the attribute is available to it, which
is a property of the *target framework*. Whether dispatch can use them is a property of the *language version*,
and the two vary independently: `<LangVersion>7.3</LangVersion>` on `net8.0` is a perfectly ordinary project.
So `LanguageFeatures` carries them separately:

- `StubHasExpressionParameters` — the attribute type is present **and accessible** from the consumer's assembly.
  Drives whether the parameters are emitted at all. Accessibility matters: on a framework without the attribute
  the only one in reach is the runtime library's own `internal` polyfill, which generated code cannot apply.
- `SupportsCallerArgExpr` — the above **and** C# 10. Drives whether the parameters carry the attribute and
  whether dispatch matches on expression text rather than on `CallerFilePath` + `CallerLineNumber`.

Below C# 10 on a framework that has the attribute, the parameters are emitted unattributed and inert: they exist
only so the lists line up, and dispatch runs off the file and line.

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
| RXUIBIND006 | Warning | Expression contains an unsupported path segment (indexer, field, or method call) |
| RXUIBIND007 | Warning | BindCommand control has no bindable event |
| RXUIBIND008 | Warning | Property does not implement IInteraction |
| RXUIBIND009 | Warning | Generated binding dispatch is out of reach from this file |
| RXUIBIND010 | Warning | Observed path passes through a type that raises no notification |

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

## Analyzer Suppression Policy

**NO analyzer suppressions are allowed unless discussed and approved first.** This applies to every form of suppression: `[SuppressMessage]` attributes, `#pragma warning disable`, `.editorconfig` severity downgrades, and `<NoWarn>` in project files. **Fix the underlying issue instead.**

If a rule genuinely cannot be fixed without changing behavior or public API, **STOP and raise it for discussion — do not suppress unilaterally, and do not present a suppression as if it were a fix.**

### Pre-approved suppressions (the ONLY ones allowed without further discussion)

| Rule | Where it may be suppressed | Justification text |
|------|----------------------------|--------------------|
| **S107** (too many parameters) | The **offending method only** (never class-level) where the parameter count is inherent — e.g. CombineLatest selector lambdas, CallerInfo dispatch stubs. | parameter count is inherent to the API/overload under test |
| **S4018** (generic type param not inferable) | **Public / interface-dictated** generic methods whose signature cannot change. Must be **fixed** (refactored) when the method is private/internal and refactorable. | type parameter is dictated by the interface / specified explicitly by the caller |
| **S100 / S101** (PascalCase naming) | Only for established domain acronyms: **INPC** (INotifyPropertyChanged), **KVO** (Key-Value Observing), **POCO**. | established acronym matching ReactiveUI domain terminology |
| **S2360** (optional parameters) | Only the CallerInfo dispatch stubs (e.g. `ReactiveSchedulerExtensions`) where converting to overloads would exceed the parameter-count limit (S107). | part of the CallerInfo dispatch contract; overloads would exceed the parameter limit |
| **CA1040** (empty interfaces) | Only interfaces that are intentional **marker interfaces** (e.g. `IActivatableView`). | intentional marker interface |

Anything **not** in this table — including (non-exhaustively) S103, S2342, S4070, RCS1157, CA1019, CA1508, S6566, S6562 — must be **fixed**, or **discussed and approved before any suppression is added**.

## Key Architectural Patterns

### Value-Equatable Models (Critical for Caching)

All pipeline models are `sealed record` types with value equality. NEVER include `ISymbol`, `SyntaxNode`, or `Location` in pipeline outputs. Use `EquatableArray<T>` for array equality. Extract strings from symbols using `ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)`.

### Code Generation Strategy

- Uses StringBuilder, NOT SyntaxFactory
- Generated code emitted as C# source via `context.AddSource()`
- `#pragma warning disable` at top of generated files
- All generated types use `[Microsoft.CodeAnalysis.Embedded]` attribute

### Where the Observation Helper Classes Are Declared

Some plugins (`KVOObservationPlugin`, `WinUIObservationPlugin`) emit observation code that instantiates helper
classes by bare name — `__KVOObservable<T>`, `__KVOObserver`, `__WinUIDPObservable<T>`. Every dispatch file is
another part of the same `__ReactiveUIGeneratedBindings` class, so one part declaring them is enough for all of
them, and two parts declaring them is a duplicate-member error.

`ObservationHelperGenerator` therefore owns the declarations outright, in `ObservationHelpers.g.cs`. Emitters
only ever reference the helpers; none of them declare any. Which helpers to declare is decided from the
**detected types**, not from the call sites — a reference can only be emitted for a type
`CodeGeneratorHelpers.FindClassInfo` matched, so the declarations are a superset of the references whichever
binding API reaches for them. Deciding it from the call sites instead leaves any API whose call sites were not
enumerated — `BindOneWay`, `BindTwoWay`, `Bind`, `OneWayBind`, `WhenAny`, `WhenAnyObservable` — emitting
references to types nobody declared.

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
- Generated output must NOT use `#nullable enable` or nullable reference type annotations (`T?` where T is a reference type) — these are C# 8+ features
- `static` lambdas are C# 9 — do not use in generated output

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

### The Lean / .Reactive Seam

`ReactiveUI.Binding.Shared` holds the whole runtime library and is compiled twice:

| Leaf | Scheduler type | Namespaces |
|------|----------------|------------|
| `ReactiveUI.Binding` | `ReactiveUI.Primitives.Concurrency.ISequencer` | `ReactiveUI.Binding.*` |
| `ReactiveUI.Binding.Reactive` | `System.Reactive.Concurrency.IScheduler` | `ReactiveUI.Binding.Reactive.*` |

`ReactiveShim.props` (imported by `Directory.Build.props`) keys on the `.Reactive` project-name suffix: it
defines `REACTIVE_SHIM` and aliases `ISequencer` onto `IScheduler`. A future platform leaf such as
`ReactiveUI.Binding.Wpf.Reactive` picks the seam up with no further wiring.

Rules for anything in `ReactiveUI.Binding.Shared`:

- Name only `ISequencer`, never `IScheduler` or a Primitives sequencer type directly.
- Declare namespaces with the macro, so both leaves get their own:
  ```csharp
  #if REACTIVE_SHIM
  namespace ReactiveUI.Binding.Reactive.Observables;
  #else
  namespace ReactiveUI.Binding.Observables;
  #endif
  ```
- Never write a per-file `using ReactiveUI.Binding.*;` — those namespaces shift between leaves, so the
  import has to come from each csproj as a `<Using>` item (unshifted vs shifted).
- Only call scheduler APIs that exist in both flavours. The state-carrying
  `Schedule<TState>(TState, Func<scheduler, TState, IDisposable>)` is the shared shape; the plain
  `Action<TState>` overload exists on `ISequencer` but binds to recursive scheduling on `IScheduler`.

The platform packages follow the same shape: `ReactiveUI.Binding.<Platform>.Shared` is compiled by
`ReactiveUI.Binding.<Platform>` and `ReactiveUI.Binding.<Platform>.Reactive`, with namespaces shifting
`ReactiveUI.Binding.<Platform>` to `ReactiveUI.Binding.Reactive.<Platform>`. Neither platform leaf
references System.Reactive directly — `AnonymousObservable` and `ActionDisposable<TState>` in
`ReactiveUI.Binding.Platform.Shared` replace `Observable.Create`/`Disposable.Create`, and are compiled
privately into each platform assembly rather than added to the base library's surface.

### Platform Test Projects

Each platform package is tested by a pair: `ReactiveUI.Binding.<Platform>.Tests` owns the test source, and
`ReactiveUI.Binding.<Platform>.Tests.Reactive` has no source of its own — it links the lean project's `.cs`
so the same assertions run against both leaves. **The `.Reactive` suffix must come last in the name**, or
`ReactiveShim.props` will not recognise it and the mirror silently compiles against the lean namespaces.

WPF and WinForms tests target the windows TFMs and need the Windows Desktop runtime, so off Windows
`Directory.Build.targets` demotes them (`IsTestProject`, `IsTestingPlatformApplication`,
`TestingPlatformDotnetTestSupport` to false, `OutputType` to `Library`). They still compile on every leg;
they only execute on the Windows one. That demotion has to live in `Directory.Build.targets`, not
`Directory.Build.props` — the testing-platform props set those back to true after props are evaluated, and
launching a windows-TFM app on Linux fails the whole run with "Zero tests ran" even when every real test
passed. The MAUI pair is exempt: it targets the plain TFMs and runs everywhere.

Test fixtures that the product code finds by reflection must be `public` — the WinForms observer looks up
`{PropertyName}Changed` over public members only, so an `internal` fixture would pass for the wrong reason.

### Building Windows Targets off Windows

`MakePri.exe` and `cswinrt.exe` are native Windows binaries. On Linux/macOS the build reaches for them
through Wine, which crashes. `build/SkipMakePriOnNonWindows.targets` turns that pipeline off, imported
by a `Directory.Build.targets` in each Windows-desktop project (WPF and MAUI, both leaves) under
`Condition="!$([MSBuild]::IsOsPlatform('Windows'))"`, so real Windows builds still generate PRI.

**That `Directory.Build.targets` must sit in the project's own folder.** MSBuild discovers it by walking
up from the project directory, so moving it into a `*.Shared` source folder silently disables it — the
build keeps working right up until Wine starts. Each copy chains to the repository-level file with
`GetPathOfFileAbove`; without that chain it shadows `src/Directory.Build.targets` instead of adding to it.

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

1. Enable `VerifierSettings.AutoVerify()` in `AssemblySetup.cs`
2. Run tests: `dotnet test --project tests/ReactiveUI.Binding.SourceGenerators.Tests/... -c Release`
3. Disable `VerifierSettings.AutoVerify()` in `AssemblySetup.cs`
4. Re-run tests to confirm they pass without AutoVerify

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
- **Where the analyzers ship:** `ReactiveUI.Binding` and `ReactiveUI.Binding.Reactive` each pack the generator
  and analyzer DLLs into `analyzers/dotnet/cs`, so referencing a runtime package is all a consumer needs.
  `ReactiveUI.Binding.SourceGenerators` is a compatibility package that ships only the MSBuild props: a second
  copy of the same assemblies under a different package root loads as a second generator and emits every
  dispatch file twice, which fails the consumer's build

**Philosophy:** Generate zero-reflection, AOT-compatible property observation and binding code at compile-time. Support all ReactiveUI platform notification mechanisms. Fall back to runtime expression analysis only when compile-time analysis is not possible.
