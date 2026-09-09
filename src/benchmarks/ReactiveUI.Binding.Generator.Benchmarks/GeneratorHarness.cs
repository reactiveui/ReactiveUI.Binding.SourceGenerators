// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Generator.Benchmarks;

/// <summary>Shared setup for the generator benchmarks: compilations and drivers over a corpus.</summary>
internal static class GeneratorHarness
{
    /// <summary>The assembly name given to the throwaway compilation the generator runs against.</summary>
    private const string CompilationAssemblyName = "Corpus";

    /// <summary>The feature a build lists interceptable namespaces under.</summary>
    private const string InterceptorsNamespacesFeature = "InterceptorsNamespaces";

    /// <summary>The namespace the generator emits interceptors into.</summary>
    private const string InterceptorNamespace = "ReactiveUI.Binding.Generated.Interceptors";

    /// <summary>
    /// The parse options of a build that lists the generated namespace, which is what the shipped targets set
    /// wherever the compiler can honour an interceptor emitted into it.
    /// </summary>
    /// <param name="intercept">Whether the build lists the namespace.</param>
    /// <returns>The parse options.</returns>
    internal static CSharpParseOptions ParseOptions(bool intercept)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp10);

        return intercept
            ? parseOptions.WithFeatures([new KeyValuePair<string, string>(InterceptorsNamespacesFeature, InterceptorNamespace)])
            : parseOptions;
    }

    /// <summary>Builds a compilation over the corpus source.</summary>
    /// <param name="sourceText">The corpus source text.</param>
    /// <param name="intercept">Whether the build lists the generated namespace for interception.</param>
    /// <returns>The compilation.</returns>
    internal static CSharpCompilation BuildCompilation(string sourceText, bool intercept)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, ParseOptions(intercept));

        var references = new List<MetadataReference>(Basic.Reference.Assemblies.Net80.References.All)
        {
            MetadataReference.CreateFromFile(typeof(ReactiveUIBindingExtensions).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ReactiveUI.Primitives.Concurrency.ISequencer).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Splat.IEnableLogger).Assembly.Location),
        };

        return CSharpCompilation.Create(
            CompilationAssemblyName,
            [syntaxTree],
            references,
            new(OutputKind.DynamicallyLinkedLibrary));
    }

    /// <summary>Creates a cold generator driver, carrying no caches from a previous run.</summary>
    /// <param name="intercept">Whether the build lists the generated namespace for interception.</param>
    /// <returns>The generator driver.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static GeneratorDriver CreateDriver(bool intercept) =>
        CSharpGeneratorDriver.Create(
            [new BindingGenerator().AsSourceGenerator()],
            null,
            ParseOptions(intercept),
            null);
}
