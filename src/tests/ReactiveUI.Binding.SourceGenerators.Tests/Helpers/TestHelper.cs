// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

/// <summary>
/// Test helper for running the incremental generator against test source code.
/// Follows the pattern from the Roslyn Source Generators Cookbook.
/// </summary>
public static class TestHelper
{
    /// <summary>
    /// Creates a compilation from source code with appropriate references.
    /// Includes ReactiveUI for IReactiveObject testing.
    /// </summary>
    /// <param name="source">The source code to compile.</param>
    /// <param name="languageVersion">Optional C# language version to target. Defaults to the latest version.</param>
    /// <returns>A compilation ready for testing.</returns>
    public static Compilation CreateCompilation(string source, LanguageVersion? languageVersion = null)
    {
        var parseOptions = languageVersion.HasValue
            ? new CSharpParseOptions(languageVersion.Value)
            : new CSharpParseOptions(LanguageVersion.Default);

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
            typeof(ReactiveUI.IReactiveObject).Assembly,
            typeof(System.Reactive.Linq.Observable).Assembly,
            typeof(ReactiveUI.Binding.ReactiveUIBindingExtensions).Assembly,
            typeof(ReactiveUI.Binding.Reactive.ObserveOnObservable<>).Assembly,
        };

        var allReferences = references.Concat(GetTransitiveReferences(seedAssemblies));

        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            allReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    /// <summary>
    /// Tests a source generator scenario that is expected to succeed.
    /// Verifies the generated output against a snapshot.
    /// </summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <param name="callerType">The type of the calling test class for snapshot organization.</param>
    /// <param name="languageVersion">Optional C# language version to target. Defaults to the latest version.</param>
    /// <param name="file">The source file path of the caller (automatically populated).</param>
    /// <param name="memberName">The member name of the caller (automatically populated).</param>
    /// <returns>A task representing the asynchronous verification operation.</returns>
    public static Task TestPass(string source, Type callerType, LanguageVersion? languageVersion = null, [CallerFilePath] string file = "", [CallerMemberName] string memberName = "")
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
        return Verifier.Verify(result.Driver, settings, file);
    }

    /// <summary>
    /// Tests a source generator scenario that is expected to succeed.
    /// Verifies the generated output against a snapshot and returns the result for further assertions.
    /// </summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <param name="callerType">The type of the calling test class for snapshot organization.</param>
    /// <param name="languageVersion">Optional C# language version to target. Defaults to the latest version.</param>
    /// <param name="file">The source file path of the caller (automatically populated).</param>
    /// <param name="memberName">The member name of the caller (automatically populated).</param>
    /// <returns>The generator test result for additional assertions.</returns>
    public static async Task<GeneratorTestResult> TestPassWithResult(string source, Type callerType, LanguageVersion? languageVersion = null, [CallerFilePath] string file = "", [CallerMemberName] string memberName = "")
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
        await Verifier.Verify(result.Driver, settings, file);

        return result;
    }

    /// <summary>
    /// Runs the source generator on the provided source code and returns the result.
    /// </summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <param name="languageVersion">Optional C# language version to target. Defaults to the latest version.</param>
    /// <returns>A <see cref="GeneratorTestResult"/> containing driver, compilation, and diagnostics.</returns>
    public static GeneratorTestResult RunGenerator(string source, LanguageVersion? languageVersion = null)
    {
        var compilation = CreateCompilation(source, languageVersion);
        var generator = new BindingGenerator();
        var sourceGenerator = generator.AsSourceGenerator();

        var parseOptions = languageVersion.HasValue
            ? new CSharpParseOptions(languageVersion.Value)
            : new CSharpParseOptions(LanguageVersion.Default);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { sourceGenerator },
            additionalTexts: null,
            parseOptions: parseOptions,
            optionsProvider: null,
            driverOptions: new GeneratorDriverOptions(
                disabledOutputs: default,
                trackIncrementalGeneratorSteps: true));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return new GeneratorTestResult(driver, outputCompilation, diagnostics);
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
        _ => typeName,
    };

    /// <summary>
    /// Abbreviates a test method name by replacing common patterns with short tokens.
    /// Keeps snapshot file paths under the Windows MAX_PATH limit (260 chars).
    /// </summary>
    /// <param name="methodName">The test method name.</param>
    /// <returns>An abbreviated method name string.</returns>
    internal static string AbbreviateMethodName(string methodName) => methodName
        .Replace("MultipleSameTypeBindings", "MSTB")
        .Replace("TwoSameTypeBindings", "2STB")
        .Replace("MultipleInvocations", "MI")
        .Replace("MultipleBindings", "MB")
        .Replace("SingleProperty", "SP")
        .Replace("MultiProperty", "MP")
        .Replace("CallerFilePath", "CFP")
        .Replace("StringToString", "S2S")
        .Replace("IntToInt", "I2I")
        .Replace("WithConverters", "WC")
        .Replace("WithConverter", "WC")
        .Replace("WithSelector", "WS")
        .Replace("AndScheduler", "Sched")
        .Replace("FourLevelDeepChain", "4LDC")
        .Replace("DeepPropertyChain", "DPC")
        .Replace("WithDeepChains", "WDC")
        .Replace("DeepChain", "DC")
        .Replace("TwoObservables", "2O")
        .Replace("ThreeProperties", "3P")
        .Replace("TwoProperties", "2P")
        .Replace("CombineLatest", "CL")
        .Replace("GeneratesElseIf", "GEI")
        .Replace("SameTypeSignature", "STS");

    /// <summary>
    /// Recursively walks assembly references from the seed assemblies to collect
    /// all transitive dependencies as metadata references.
    /// </summary>
    /// <param name="seedAssemblies">The root assemblies to start from.</param>
    /// <returns>Metadata references for all reachable assemblies.</returns>
    private static IEnumerable<MetadataReference> GetTransitiveReferences(params System.Reflection.Assembly[] seedAssemblies)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<System.Reflection.Assembly>(seedAssemblies);

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
