// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>
/// Test helper for running the incremental generator against test source code.
/// Follows the pattern from the Roslyn Source Generators Cookbook.
/// </summary>
public static class TestHelper
{
    /// <summary>
    /// Returns the C# language version used to exercise the <c>CallerFilePath</c> + <c>CallerLineNumber</c>
    /// dispatch fallback. The version is deliberately kept below C# 10 (where
    /// <c>CallerArgumentExpression</c> would take over) so the file/line dispatch path is exercised, while
    /// still being high enough to compile the scenario source. Scenario view models use nullable reference
    /// type annotations (a C# 8 feature); compiling them under C# 7.3 would raise
    /// "nullable reference types is not available". When the scenario has no nullable annotations, C# 7.3 is
    /// used so the generated output is also asserted valid on the minimum supported language version.
    /// </summary>
    /// <param name="nullableEnabled">Whether the scenario source uses nullable reference type annotations.</param>
    /// <returns>C# 8 when the scenario uses nullable annotations; otherwise C# 7.3.</returns>
    public static LanguageVersion FallbackLanguageVersion(bool nullableEnabled) =>
        nullableEnabled ? LanguageVersion.CSharp8 : LanguageVersion.CSharp7_3;

    /// <summary>Creates a compilation from source code, targeting C# 7.3 to verify generated output compatibility.</summary>
    /// <param name="source">The source code to compile.</param>
    /// <returns>A compilation ready for testing.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Compilation CreateCompilation(string source) => CreateCompilation(source, null);

    /// <summary>Creates a compilation from source code with appropriate references. Includes ReactiveUI for IReactiveObject testing.</summary>
    /// <param name="source">The source code to compile.</param>
    /// <param name="languageVersion">The C# language version to target, or <see langword="null"/> for C# 7.3.</param>
    /// <returns>A compilation ready for testing.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Compilation CreateCompilation(string source, LanguageVersion? languageVersion) =>
        CreateCompilation(source, languageVersion, false);

    /// <summary>
    /// Creates a compilation from source code, referencing one of the two runtime packages. The packages share
    /// no type names, so a compilation references one or the other and never both - which is also what lets the
    /// generator tell which flavour it is generating for.
    /// </summary>
    /// <param name="source">The source code to compile.</param>
    /// <param name="languageVersion">The C# language version to target, or <see langword="null"/> for C# 7.3.</param>
    /// <param name="useReactiveRuntime">Whether to reference the System.Reactive flavour rather than the lean one.</param>
    /// <returns>A compilation ready for testing.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Compilation CreateCompilation(
        string source,
        LanguageVersion? languageVersion,
        bool useReactiveRuntime) =>
        CreateCompilation(source, languageVersion, useReactiveRuntime, "TestAssembly", []);

    /// <summary>
    /// Creates a compilation that also references separately built assemblies, so a scenario can put a type
    /// somewhere the generator cannot see a declaration for.
    /// </summary>
    /// <param name="source">The source code to compile.</param>
    /// <param name="languageVersion">The C# language version to target, or <see langword="null"/> for C# 7.3.</param>
    /// <param name="useReactiveRuntime">Whether to reference the System.Reactive flavour rather than the lean one.</param>
    /// <param name="assemblyName">The name to give the compilation's own assembly.</param>
    /// <param name="additionalReferences">References to add on top of the framework and runtime ones.</param>
    /// <returns>A compilation ready for testing.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Compilation CreateCompilation(
        string source,
        LanguageVersion? languageVersion,
        bool useReactiveRuntime,
        string assemblyName,
        ImmutableArray<MetadataReference> additionalReferences) =>
        CreateCompilation(source, ParseOptionsFor(languageVersion), useReactiveRuntime, assemblyName, additionalReferences);

    /// <summary>
    /// Creates a compilation from source code parsed exactly as the caller asks, which is how a scenario reaches
    /// the options a language version alone cannot express.
    /// </summary>
    /// <param name="source">The source code to compile.</param>
    /// <param name="parseOptions">The options the consumer's source is parsed with.</param>
    /// <param name="useReactiveRuntime">Whether to reference the System.Reactive flavour rather than the lean one.</param>
    /// <param name="assemblyName">The name to give the compilation's own assembly.</param>
    /// <param name="additionalReferences">References to add on top of the framework and runtime ones.</param>
    /// <returns>A compilation ready for testing.</returns>
    public static Compilation CreateCompilation(
        string source,
        CSharpParseOptions parseOptions,
        bool useReactiveRuntime,
        string assemblyName,
        ImmutableArray<MetadataReference> additionalReferences)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

#if NET11_0_OR_GREATER
        IEnumerable<MetadataReference> references = Basic.Reference.Assemblies.Net110.References.All;
#elif NET10_0_OR_GREATER
        IEnumerable<MetadataReference> references = Basic.Reference.Assemblies.Net100.References.All;
#elif NET9_0_OR_GREATER
        IEnumerable<MetadataReference> references = Basic.Reference.Assemblies.Net90.References.All;
#else
        IEnumerable<MetadataReference> references = Basic.Reference.Assemblies.Net80.References.All;
#endif

        // Add ReactiveUI and transitive assembly references.
        // ReactiveUI is seeded by ReactiveObject rather than IReactiveObject: the two live in
        // different assemblies, and the walk only follows references outward, so seeding from the
        // interface would leave the assembly that declares ReactiveObject out of the compilation.
        var runtimeSeeds = useReactiveRuntime
            ? new[] { typeof(ReactiveUI.Binding.Reactive.ReactiveUIBindingExtensions).Assembly }
            : new[]
            {
                typeof(ReactiveUIBindingExtensions).Assembly,
                typeof(ReactiveUI.Primitives.Concurrency.ISequencer).Assembly,
            };

        var seedAssemblies = new[]
        {
            typeof(ReactiveObject).Assembly, typeof(IReactiveObject).Assembly,
            typeof(System.Reactive.Linq.Observable).Assembly,
        }.Concat(runtimeSeeds).ToArray();

        var allReferences = references
            .Concat(GetTransitiveReferences(seedAssemblies))
            .Concat(additionalReferences);

        return CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            allReferences,
            new(OutputKind.DynamicallyLinkedLibrary));
    }

    /// <summary>The parse options a build produces from a language version, which is what the tests usually name.</summary>
    /// <param name="languageVersion">The C# language version to target, or <see langword="null"/> for C# 7.3.</param>
    /// <returns>The parse options.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CSharpParseOptions ParseOptionsFor(LanguageVersion? languageVersion) =>
        new(languageVersion ?? LanguageVersion.CSharp7_3);

    /// <summary>
    /// The parse options of a build that has the package's targets on it, which list the generated namespace so
    /// the compiler honours an interceptor emitted into it.
    /// </summary>
    /// <param name="languageVersion">The C# language version to target, or <see langword="null"/> for C# 7.3.</param>
    /// <returns>The parse options, carrying the opt-in.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CSharpParseOptions InterceptingParseOptionsFor(LanguageVersion? languageVersion) =>
        ParseOptionsFor(languageVersion)
            .WithFeatures([new KeyValuePair<string, string>("InterceptorsNamespaces", "ReactiveUI.Binding.Generated.Interceptors")]);

    /// <summary>
    /// Compiles source into a metadata reference, standing in for a type the consumer references rather than
    /// declares. A referenced type reaches the generator only as a symbol, never as a declaration.
    /// </summary>
    /// <param name="source">The source code of the referenced assembly.</param>
    /// <param name="assemblyName">The name to give the referenced assembly.</param>
    /// <param name="languageVersion">The C# language version to target, or <see langword="null"/> for C# 7.3.</param>
    /// <returns>A metadata reference to the compiled assembly.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the referenced source does not compile.</exception>
    public static MetadataReference CompileToReference(
        string source,
        string assemblyName,
        LanguageVersion? languageVersion)
    {
        var compilation = CreateCompilation(source, languageVersion, false, assemblyName, []);

        var errors = compilation.GetDiagnostics()
            .Where(static d => d.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();

        if (!errors.IsEmpty)
        {
            throw new InvalidOperationException(
                $"Referenced assembly source failed to compile:{Environment.NewLine}"
                + string.Join(Environment.NewLine, errors.Select(static d => $"  {d.Id}: {d.GetMessage()}")));
        }

        return compilation.ToMetadataReference();
    }

    /// <summary>Tests a source generator scenario that is expected to succeed. Verifies the generated output against a snapshot.</summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <param name="callerType">The type of the calling test class for snapshot organization.</param>
    /// <param name="file">The source file path of the caller (automatically populated).</param>
    /// <param name="memberName">The member name of the caller (automatically populated).</param>
    /// <returns>A task representing the asynchronous verification operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task TestPass(
        string source,
        Type callerType,
        [CallerFilePath] string file = "",
        [CallerMemberName] string memberName = "") =>
        TestPass(source, callerType, null, file, memberName);

    /// <summary>
    /// Tests a source generator scenario that is expected to succeed, targeting a specific language version.
    /// Verifies the generated output against a snapshot.
    /// </summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <param name="callerType">The type of the calling test class for snapshot organization.</param>
    /// <param name="languageVersion">The C# language version to target, or <see langword="null"/> for C# 7.3.</param>
    /// <param name="file">The source file path of the caller (automatically populated).</param>
    /// <param name="memberName">The member name of the caller (automatically populated).</param>
    /// <returns>A task representing the asynchronous verification operation.</returns>
    public static Task TestPass(
        string source,
        Type callerType,
        LanguageVersion? languageVersion,
        [CallerFilePath] string file = "",
        [CallerMemberName] string memberName = "")
    {
        ArgumentNullException.ThrowIfNull(callerType);
        ArgumentNullException.ThrowIfNull(memberName);

        var result = RunGenerator(source, languageVersion);

        // Log any diagnostics for debugging
        var allDiagnostics = result.OutputCompilation.GetDiagnostics()
            .Concat(result.GeneratorDiagnostics)
            .Where(static d => d.Severity >= DiagnosticSeverity.Warning)
            .ToImmutableArray();

        foreach (var diagnostic in allDiagnostics)
        {
            TestContext.Current?.OutputWriter.WriteLine($"{diagnostic.Severity}: {diagnostic.GetMessage()}");
        }

        VerifySettings settings = new();
        settings.DisableRequireUniquePrefix();
        settings.UseTypeName(AbbreviateTypeName(callerType.Name));
        settings.UseMethodName(AbbreviateMethodName(memberName));
        return Verify(result.Driver, settings, file);
    }

    /// <summary>
    /// Tests a source generator scenario that is expected to succeed.
    /// Verifies the generated output against a snapshot and returns the result for further assertions.
    /// </summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <param name="callerType">The type of the calling test class for snapshot organization.</param>
    /// <param name="file">The source file path of the caller (automatically populated).</param>
    /// <param name="memberName">The member name of the caller (automatically populated).</param>
    /// <returns>The generator test result for additional assertions.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<GeneratorTestResult> TestPassWithResult(
        string source,
        Type callerType,
        [CallerFilePath] string file = "",
        [CallerMemberName] string memberName = "") =>
        TestPassWithResult(source, callerType, null, file, memberName);

    /// <summary>
    /// Tests a source generator scenario that is expected to succeed, targeting a specific language version.
    /// Verifies the generated output against a snapshot and returns the result for further assertions.
    /// </summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <param name="callerType">The type of the calling test class for snapshot organization.</param>
    /// <param name="languageVersion">The C# language version to target, or <see langword="null"/> for C# 7.3.</param>
    /// <param name="file">The source file path of the caller (automatically populated).</param>
    /// <param name="memberName">The member name of the caller (automatically populated).</param>
    /// <returns>The generator test result for additional assertions.</returns>
    public static async Task<GeneratorTestResult> TestPassWithResult(
        string source,
        Type callerType,
        LanguageVersion? languageVersion,
        [CallerFilePath] string file = "",
        [CallerMemberName] string memberName = "")
    {
        ArgumentNullException.ThrowIfNull(callerType);
        ArgumentNullException.ThrowIfNull(memberName);

        var result = RunGenerator(source, languageVersion);

        // Log any diagnostics for debugging
        var allDiagnostics = result.OutputCompilation.GetDiagnostics()
            .Concat(result.GeneratorDiagnostics)
            .Where(static d => d.Severity >= DiagnosticSeverity.Warning)
            .ToImmutableArray();

        foreach (var diagnostic in allDiagnostics)
        {
            var writer = TestContext.Current?.OutputWriter;
            if (writer is not null)
            {
                await writer.WriteLineAsync($"{diagnostic.Severity}: {diagnostic.GetMessage()}");
            }
        }

        VerifySettings settings = new();
        settings.DisableRequireUniquePrefix();
        settings.UseTypeName(AbbreviateTypeName(callerType.Name));
        settings.UseMethodName(AbbreviateMethodName(memberName));
        await Verify(result.Driver, settings, file);

        return result;
    }

    /// <summary>Runs the source generator on the provided source code and returns the result.</summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <returns>A <see cref="GeneratorTestResult"/> containing driver, compilation, and diagnostics.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GeneratorTestResult RunGenerator(string source) => RunGenerator(source, null);

    /// <summary>Runs the source generator on the provided source code, targeting a specific language version.</summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <param name="languageVersion">The C# language version to target, or <see langword="null"/> for C# 7.3.</param>
    /// <returns>A <see cref="GeneratorTestResult"/> containing driver, compilation, and diagnostics.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GeneratorTestResult RunGenerator(string source, LanguageVersion? languageVersion) =>
        RunGenerator(source, languageVersion, null);

    /// <summary>
    /// Runs the source generator on the provided source code, targeting a specific language version and
    /// reporting the given root namespace to it the way a real build would.
    /// </summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <param name="languageVersion">The C# language version to target, or <see langword="null"/> for C# 7.3.</param>
    /// <param name="rootNamespace">
    /// The root namespace the build exposes, or <see langword="null"/> to run with none - which is what every
    /// other overload does, because an in-memory compilation carries no MSBuild properties.
    /// </param>
    /// <returns>A <see cref="GeneratorTestResult"/> containing driver, compilation, and diagnostics.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GeneratorTestResult RunGenerator(
        string source,
        LanguageVersion? languageVersion,
        string? rootNamespace) =>
        RunGenerator(source, languageVersion, rootNamespace, false);

    /// <summary>
    /// Runs the source generator against one of the two runtime packages, reporting the given root namespace to
    /// it the way a real build would.
    /// </summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <param name="languageVersion">The C# language version to target, or <see langword="null"/> for C# 7.3.</param>
    /// <param name="rootNamespace">The root namespace the build exposes, or <see langword="null"/> for none.</param>
    /// <param name="useReactiveRuntime">Whether to reference the System.Reactive flavour rather than the lean one.</param>
    /// <returns>A <see cref="GeneratorTestResult"/> containing driver, compilation, and diagnostics.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GeneratorTestResult RunGenerator(
        string source,
        LanguageVersion? languageVersion,
        string? rootNamespace,
        bool useReactiveRuntime) =>
        RunGenerator(source, languageVersion, rootNamespace, useReactiveRuntime, []);

    /// <summary>
    /// Runs the source generator against a compilation that also references separately built assemblies, so a
    /// scenario can observe a type the consumer references rather than declares.
    /// </summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <param name="languageVersion">The C# language version to target, or <see langword="null"/> for C# 7.3.</param>
    /// <param name="rootNamespace">The root namespace the build exposes, or <see langword="null"/> for none.</param>
    /// <param name="useReactiveRuntime">Whether to reference the System.Reactive flavour rather than the lean one.</param>
    /// <param name="additionalReferences">References to add on top of the framework and runtime ones.</param>
    /// <returns>A <see cref="GeneratorTestResult"/> containing driver, compilation, and diagnostics.</returns>
    public static GeneratorTestResult RunGenerator(
        string source,
        LanguageVersion? languageVersion,
        string? rootNamespace,
        bool useReactiveRuntime,
        ImmutableArray<MetadataReference> additionalReferences)
    {
        var compilation = CreateCompilation(
            source,
            languageVersion,
            useReactiveRuntime,
            "TestAssembly",
            additionalReferences);
        var generator = new BindingGenerator();
        var sourceGenerator = generator.AsSourceGenerator();

        var parseOptions = languageVersion.HasValue
            ? new CSharpParseOptions(languageVersion.Value)
            : new CSharpParseOptions(LanguageVersion.CSharp7_3);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [sourceGenerator],
            null,
            parseOptions,
            rootNamespace is null ? null : new BuildPropertyOptionsProvider(rootNamespace),
            new(
                default,
                true));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return new(driver, outputCompilation, diagnostics);
    }

    /// <summary>Runs the source generator over a compilation built by the caller, with the given build properties.</summary>
    /// <param name="compilation">The compilation to generate against.</param>
    /// <param name="languageVersion">The C# language version to parse generated code with.</param>
    /// <param name="rootNamespace">The root namespace the build exposes, or <see langword="null"/> for none.</param>
    /// <param name="emitGeneratedCodeMarkers">Whether the build asks for the generated-file markers.</param>
    /// <returns>A <see cref="GeneratorTestResult"/> containing driver, compilation, and diagnostics.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GeneratorTestResult RunGenerator(
        Compilation compilation,
        LanguageVersion? languageVersion,
        string? rootNamespace,
        bool emitGeneratedCodeMarkers) =>
        RunGenerator(compilation, ParseOptionsFor(languageVersion), rootNamespace, emitGeneratedCodeMarkers);

    /// <summary>Runs the source generator with the parse options the caller names, for both input and output.</summary>
    /// <param name="compilation">The compilation to generate against.</param>
    /// <param name="parseOptions">The options generated code is parsed with, and that the generator reads.</param>
    /// <param name="rootNamespace">The root namespace the build exposes, or <see langword="null"/> for none.</param>
    /// <param name="emitGeneratedCodeMarkers">Whether the build asks for the generated-file markers.</param>
    /// <returns>A <see cref="GeneratorTestResult"/> containing driver, compilation, and diagnostics.</returns>
    public static GeneratorTestResult RunGenerator(
        Compilation compilation,
        CSharpParseOptions parseOptions,
        string? rootNamespace,
        bool emitGeneratedCodeMarkers)
    {
        var generator = new BindingGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [generator.AsSourceGenerator()],
            null,
            parseOptions,
            new BuildPropertyOptionsProvider(rootNamespace, emitGeneratedCodeMarkers),
            new(
                default,
                true));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return new(driver, outputCompilation, diagnostics);
    }

    /// <summary>Emits the output compilation to memory and loads it into a collectible assembly load context.</summary>
    /// <param name="result">The generator test result to emit.</param>
    /// <returns>The loaded assembly and the load context (dispose context to unload).</returns>
    /// <exception cref="InvalidOperationException">Thrown when emission fails.</exception>
    [SuppressMessage(
        "Security",
        "SES1402:Assembly loaded from an unverifiable source",
        Justification = "loads the compilation this test just emitted, in-process, into a collectible context")]
    public static LoadedAssembly EmitAndLoad(GeneratorTestResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        using var assemblyStream = new MemoryStream();
        var emitResult = result.OutputCompilation.Emit(assemblyStream);

        if (!emitResult.Success)
        {
            var errors = string.Join(
                Environment.NewLine,
                emitResult.Diagnostics
                    .Where(static d => d.Severity == DiagnosticSeverity.Error)
                    .Select(static d => $"  {d.Id}: {d.GetMessage()}"));

            throw new InvalidOperationException(
                $"Failed to emit compilation:{Environment.NewLine}{errors}");
        }

        assemblyStream.Position = 0;
        var context = new CollectibleAssemblyLoadContext();
        var assembly = context.LoadFromStream(assemblyStream);
        return new(assembly, context);
    }

    /// <summary>
    /// Abbreviates a test class type name to a short prefix for snapshot file names.
    /// Keeps snapshot file paths under the Windows MAX_PATH limit (260 chars).
    /// </summary>
    /// <param name="typeName">The test class type name.</param>
    /// <returns>An abbreviated prefix string.</returns>
    internal static string AbbreviateTypeName(string typeName) => typeName switch
    {
        "BindGeneratorTests" => "BG",
        "BindOneWayGeneratorTests" => "BOG",
        "BindTwoWayGeneratorTests" => "BTG",
        "OneWayBindGeneratorTests" => "OBG",
        "WhenAnyGeneratorTests" => "WAG",
        "WhenAnyObservableGeneratorTests" => "WAOG",
        "WhenAnyValueGeneratorTests" => "WAVG",
        "WhenChangedGeneratorTests" => "WCG",
        "WhenChangingGeneratorTests" => "WCnG",
        "PlatformDetectionSnapshotTests" => "PDS",
        "BindInteractionGeneratorTests" => "BIG",
        "BindCommandGeneratorTests" => "BCG",
        "BindToGeneratorTests" => "BToG",
        "InvokeCommandGeneratorTests" => "ICG",
        "ViewLocatorDispatchGeneratorTests" => "VDG",
        _ => typeName
    };

    /// <summary>
    /// Abbreviates a test method name by replacing common patterns with short tokens.
    /// Keeps snapshot file paths under the Windows MAX_PATH limit (260 chars).
    /// </summary>
    /// <param name="methodName">The test method name.</param>
    /// <returns>An abbreviated method name string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string AbbreviateMethodName(string methodName) => methodName
        .Replace("MultipleSameTypeBindings", "MSTB", StringComparison.Ordinal)
        .Replace("TwoSameTypeBindings", "2STB", StringComparison.Ordinal)
        .Replace("MultipleInvocations", "MI", StringComparison.Ordinal)
        .Replace("MultipleBindings", "MB", StringComparison.Ordinal)
        .Replace("SingleProperty", "SP", StringComparison.Ordinal)
        .Replace("MultiProperty", "MP", StringComparison.Ordinal)
        .Replace("CallerFilePath", "CFP", StringComparison.Ordinal)
        .Replace("StringToString", "S2S", StringComparison.Ordinal)
        .Replace("IntToInt", "I2I", StringComparison.Ordinal)
        .Replace("WithConverters", "WC", StringComparison.Ordinal)
        .Replace("WithConverter", "WC", StringComparison.Ordinal)
        .Replace("WithSelector", "WS", StringComparison.Ordinal)
        .Replace("AndScheduler", "Sched", StringComparison.Ordinal)
        .Replace("FourLevelDeepChain", "4LDC", StringComparison.Ordinal)
        .Replace("DeepPropertyChain", "DPC", StringComparison.Ordinal)
        .Replace("WithDeepChains", "WDC", StringComparison.Ordinal)
        .Replace("DeepChain", "DC", StringComparison.Ordinal)
        .Replace("TwoObservables", "2O", StringComparison.Ordinal)
        .Replace("ThreeProperties", "3P", StringComparison.Ordinal)
        .Replace("TwoProperties", "2P", StringComparison.Ordinal)
        .Replace("CombineLatest", "CL", StringComparison.Ordinal)
        .Replace("GeneratesElseIf", "GEI", StringComparison.Ordinal)
        .Replace("SameTypeSignature", "STS", StringComparison.Ordinal);

    /// <summary>
    /// Recursively walks assembly references from the seed assemblies to collect
    /// all transitive dependencies as metadata references.
    /// </summary>
    /// <param name="seedAssemblies">The root assemblies to start from.</param>
    /// <returns>Metadata references for all reachable assemblies.</returns>
    private static IEnumerable<MetadataReference> GetTransitiveReferences(params Assembly[] seedAssemblies)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<Assembly>(seedAssemblies);

        while (queue.Count > 0)
        {
            var assembly = queue.Dequeue();
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
            {
                continue;
            }

            if (!seen.Add(assembly.Location))
            {
                continue;
            }

            yield return MetadataReference.CreateFromFile(assembly.Location);

            foreach (var referencedName in assembly.GetReferencedAssemblies())
            {
                try
                {
                    queue.Enqueue(System.Reflection.Assembly.Load(referencedName));
                }
                catch (Exception e)
                {
                    // Reference discovery is best-effort: anything that will not load is already
                    // covered by the Basic.Reference.Assemblies framework set. The walk must carry
                    // on, because letting a load failure escape truncates the reference list and
                    // surfaces later as "type could not be found" in every generated compilation.
                    TestContext.Current?.OutputWriter.WriteLine(
                        $"skipped unresolvable reference '{referencedName.Name}': {e.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Supplies the generator the MSBuild properties a real build would, which an in-memory compilation
    /// otherwise has no way to carry.
    /// </summary>
    private sealed class BuildPropertyOptionsProvider : AnalyzerConfigOptionsProvider
    {
        /// <summary>The analyzer config key a build exposes the root namespace under.</summary>
        private const string RootNamespaceKey = "build_property.RootNamespace";

        /// <summary>The analyzer config key a build opts out of the generated-file markers under.</summary>
        private const string EmitMarkersKey = "build_property.ReactiveUIBindingEmitGeneratedCodeMarkers";

        /// <summary>The per-file options, which nothing under test reads.</summary>
        private static readonly BuildPropertyOptions NoOptions = new(new Dictionary<string, string>(StringComparer.Ordinal));

        /// <summary>Initializes a new instance of the <see cref="BuildPropertyOptionsProvider"/> class.</summary>
        /// <param name="rootNamespace">The root namespace to report.</param>
        public BuildPropertyOptionsProvider(string rootNamespace) =>
            GlobalOptions = new BuildPropertyOptions(
                new Dictionary<string, string>(StringComparer.Ordinal) { [RootNamespaceKey] = rootNamespace });

        /// <summary>Initializes a new instance of the <see cref="BuildPropertyOptionsProvider"/> class.</summary>
        /// <param name="rootNamespace">The root namespace to report, or null to report none.</param>
        /// <param name="emitGeneratedCodeMarkers">Whether the build asks for the generated-file markers.</param>
        public BuildPropertyOptionsProvider(string? rootNamespace, bool emitGeneratedCodeMarkers)
        {
            var options = new Dictionary<string, string>(StringComparer.Ordinal) { [EmitMarkersKey] = emitGeneratedCodeMarkers ? "true" : "false" };

            if (rootNamespace is not null)
            {
                options[RootNamespaceKey] = rootNamespace;
            }

            GlobalOptions = new BuildPropertyOptions(options);
        }

        /// <inheritdoc/>
        public override AnalyzerConfigOptions GlobalOptions { get; }

        /// <inheritdoc/>
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => NoOptions;

        /// <inheritdoc/>
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => NoOptions;
    }

    /// <summary>A fixed set of analyzer config options.</summary>
    /// <param name="options">The options to expose.</param>
    /// <remarks>
    /// The out parameter is non-nullable where the base declares it nullable-with-a-postcondition, because the
    /// attribute that states the postcondition cannot be named in this assembly: the generator compiles its own
    /// netstandard2.0 copy of it and makes it visible here, so the name resolves to two types. Always assigning
    /// a value keeps the stronger promise, so nothing downstream can observe the difference.
    /// </remarks>
    private sealed class BuildPropertyOptions(IReadOnlyDictionary<string, string> options) : AnalyzerConfigOptions
    {
        /// <inheritdoc/>
        public override IEnumerable<string> Keys => options.Keys;

        /// <inheritdoc/>
        public override bool TryGetValue(string key, out string value)
        {
            if (options.TryGetValue(key, out var found))
            {
                value = found;
                return true;
            }

            value = string.Empty;
            return false;
        }
    }
}
