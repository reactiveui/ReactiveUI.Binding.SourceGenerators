// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

    /// <summary>
    /// Creates a compilation from source code, targeting C# 7.3 to verify generated output compatibility.
    /// </summary>
    /// <param name="source">The source code to compile.</param>
    /// <returns>A compilation ready for testing.</returns>
    public static Compilation CreateCompilation(string source) => CreateCompilation(source, null);

    /// <summary>
    /// Creates a compilation from source code with appropriate references.
    /// Includes ReactiveUI for IReactiveObject testing.
    /// </summary>
    /// <param name="source">The source code to compile.</param>
    /// <param name="languageVersion">The C# language version to target, or <see langword="null"/> for C# 7.3.</param>
    /// <returns>A compilation ready for testing.</returns>
    public static Compilation CreateCompilation(string source, LanguageVersion? languageVersion)
    {
        var parseOptions = languageVersion.HasValue
            ? new CSharpParseOptions(languageVersion.Value)
            : new CSharpParseOptions(LanguageVersion.CSharp7_3);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

        IEnumerable<MetadataReference> references;

#if NET10_0_OR_GREATER
        references = Basic.Reference.Assemblies.Net100.References.All;
#elif NET9_0_OR_GREATER
        references = Basic.Reference.Assemblies.Net90.References.All;
#else
        references = Basic.Reference.Assemblies.Net80.References.All;
#endif

        // Add ReactiveUI and transitive assembly references
        var seedAssemblies = new[]
        {
            typeof(IReactiveObject).Assembly, typeof(System.Reactive.Linq.Observable).Assembly,
            typeof(ReactiveUIBindingExtensions).Assembly, typeof(Reactive.ObserveOnObservable<>).Assembly
        };

        var allReferences = references.Concat(GetTransitiveReferences(seedAssemblies));

        return CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            allReferences,
            new(OutputKind.DynamicallyLinkedLibrary));
    }

    /// <summary>
    /// Tests a source generator scenario that is expected to succeed.
    /// Verifies the generated output against a snapshot.
    /// </summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <param name="callerType">The type of the calling test class for snapshot organization.</param>
    /// <param name="file">The source file path of the caller (automatically populated).</param>
    /// <param name="memberName">The member name of the caller (automatically populated).</param>
    /// <returns>A task representing the asynchronous verification operation.</returns>
    public static Task TestPass(
        string source,
        Type callerType,
        [CallerFilePath] string file = "",
        [CallerMemberName] string memberName = "")
        => TestPass(source, callerType, null, file, memberName);

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
            .Where(d => d.Severity >= DiagnosticSeverity.Warning)
            .ToImmutableArray();

        foreach (var diagnostic in allDiagnostics)
        {
            Console.WriteLine($"{diagnostic.Severity}: {diagnostic.GetMessage()}");
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
    public static Task<GeneratorTestResult> TestPassWithResult(
        string source,
        Type callerType,
        [CallerFilePath] string file = "",
        [CallerMemberName] string memberName = "")
        => TestPassWithResult(source, callerType, null, file, memberName);

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
            .Where(d => d.Severity >= DiagnosticSeverity.Warning)
            .ToImmutableArray();

        foreach (var diagnostic in allDiagnostics)
        {
            Console.WriteLine($"{diagnostic.Severity}: {diagnostic.GetMessage()}");
        }

        VerifySettings settings = new();
        settings.DisableRequireUniquePrefix();
        settings.UseTypeName(AbbreviateTypeName(callerType.Name));
        settings.UseMethodName(AbbreviateMethodName(memberName));
        await Verify(result.Driver, settings, file);

        return result;
    }

    /// <summary>
    /// Runs the source generator on the provided source code and returns the result.
    /// </summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <returns>A <see cref="GeneratorTestResult"/> containing driver, compilation, and diagnostics.</returns>
    public static GeneratorTestResult RunGenerator(string source) => RunGenerator(source, null);

    /// <summary>
    /// Runs the source generator on the provided source code, targeting a specific language version.
    /// </summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <param name="languageVersion">The C# language version to target, or <see langword="null"/> for C# 7.3.</param>
    /// <returns>A <see cref="GeneratorTestResult"/> containing driver, compilation, and diagnostics.</returns>
    public static GeneratorTestResult RunGenerator(string source, LanguageVersion? languageVersion)
    {
        var compilation = CreateCompilation(source, languageVersion);
        var generator = new BindingGenerator();
        var sourceGenerator = generator.AsSourceGenerator();

        var parseOptions = languageVersion.HasValue
            ? new CSharpParseOptions(languageVersion.Value)
            : new CSharpParseOptions(LanguageVersion.CSharp7_3);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [sourceGenerator],
            null,
            parseOptions,
            null,
            new(
                default,
                true));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return new(driver, outputCompilation, diagnostics);
    }

    /// <summary>
    /// Emits the output compilation to memory and loads it into a collectible assembly load context.
    /// </summary>
    /// <param name="result">The generator test result to emit.</param>
    /// <returns>The loaded assembly and the load context (dispose context to unload).</returns>
    /// <exception cref="InvalidOperationException">Thrown when emission fails.</exception>
    public static (Assembly Assembly, CollectibleAssemblyLoadContext Context) EmitAndLoad(GeneratorTestResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        using var ms = new MemoryStream();
        var emitResult = result.OutputCompilation.Emit(ms);

        if (!emitResult.Success)
        {
            var errors = string.Join(
                Environment.NewLine,
                emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => $"  {d.Id}: {d.GetMessage()}"));

            throw new InvalidOperationException(
                $"Failed to emit compilation:{Environment.NewLine}{errors}");
        }

        ms.Position = 0;
        var context = new CollectibleAssemblyLoadContext();
        var assembly = context.LoadFromStream(ms);
        return (assembly, context);
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
        "ViewLocatorDispatchGeneratorTests" => "VDG",
        _ => typeName
    };

    /// <summary>
    /// Abbreviates a test method name by replacing common patterns with short tokens.
    /// Keeps snapshot file paths under the Windows MAX_PATH limit (260 chars).
    /// </summary>
    /// <param name="methodName">The test method name.</param>
    /// <returns>An abbreviated method name string.</returns>
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
                catch
                {
                    // System assemblies already covered by Basic.Reference.Assemblies
                }
            }
        }
    }
}
