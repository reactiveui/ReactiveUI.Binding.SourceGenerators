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
    /// <returns>A compilation ready for testing.</returns>
    public static Compilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        IEnumerable<MetadataReference> references;

#if NET10_0_OR_GREATER
        references = Basic.Reference.Assemblies.Net100.References.All;
#elif NET9_0_OR_GREATER
        references = Basic.Reference.Assemblies.Net90.References.All;
#else
        references = Basic.Reference.Assemblies.Net80.References.All;
#endif

        // Add ReactiveUI assembly reference for IReactiveObject testing
        var reactiveUIAssembly = typeof(ReactiveUI.IReactiveObject).Assembly;
        var systemReactiveAssembly = typeof(System.Reactive.Linq.Observable).Assembly;

        // Add ReactiveUI.Binding assembly reference for stub extension methods
        var reactiveUIBindingAssembly = typeof(ReactiveUI.Binding.ReactiveUIBindingExtensions).Assembly;
        var reactiveUIBindingReactiveAssembly = typeof(ReactiveUI.Binding.Reactive.ObserveOnObservable<>).Assembly;
        var allReferences = references
            .Concat(new[]
            {
                MetadataReference.CreateFromFile(reactiveUIAssembly.Location),
                MetadataReference.CreateFromFile(systemReactiveAssembly.Location),
                MetadataReference.CreateFromFile(reactiveUIBindingAssembly.Location),
                MetadataReference.CreateFromFile(reactiveUIBindingReactiveAssembly.Location),
            });

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
    /// <param name="file">The source file path of the caller (automatically populated).</param>
    /// <param name="memberName">The member name of the caller (automatically populated).</param>
    /// <returns>A task representing the asynchronous verification operation.</returns>
    public static Task TestPass(string source, Type callerType, [CallerFilePath] string file = "", [CallerMemberName] string memberName = "")
    {
        ArgumentNullException.ThrowIfNull(callerType);

        var result = RunGenerator(source);

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
        settings.UseTypeName(callerType.Name);
        settings.UseMethodName(memberName);
        return Verifier.Verify(result.Driver, settings, file);
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
    public static async Task<GeneratorTestResult> TestPassWithResult(string source, Type callerType, [CallerFilePath] string file = "", [CallerMemberName] string memberName = "")
    {
        ArgumentNullException.ThrowIfNull(callerType);

        var result = RunGenerator(source);

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
        settings.UseTypeName(callerType.Name);
        settings.UseMethodName(memberName);
        await Verifier.Verify(result.Driver, settings, file);

        return result;
    }

    /// <summary>
    /// Runs the source generator on the provided source code and returns the result.
    /// </summary>
    /// <param name="source">The source code to compile and generate.</param>
    /// <returns>A <see cref="GeneratorTestResult"/> containing driver, compilation, and diagnostics.</returns>
    public static GeneratorTestResult RunGenerator(string source)
    {
        var compilation = CreateCompilation(source);
        var generator = new BindingGenerator();
        var sourceGenerator = generator.AsSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { sourceGenerator },
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
}
