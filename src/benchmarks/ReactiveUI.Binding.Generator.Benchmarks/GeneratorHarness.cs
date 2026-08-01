// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Generator.Benchmarks;

/// <summary>Shared setup for the generator benchmarks: compilations and drivers over a corpus.</summary>
internal static class GeneratorHarness
{
    /// <summary>The assembly name given to the throwaway compilation the generator runs against.</summary>
    private const string CompilationAssemblyName = "Corpus";

    /// <summary>Builds a compilation over the corpus source.</summary>
    /// <param name="sourceText">The corpus source text.</param>
    /// <returns>The compilation.</returns>
    internal static CSharpCompilation BuildCompilation(string sourceText)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp10);
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, parseOptions);

        var references = new List<MetadataReference>(Basic.Reference.Assemblies.Net80.References.All)
        {
            MetadataReference.CreateFromFile(typeof(ReactiveUIBindingExtensions).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ReactiveUI.Primitives.Concurrency.ISequencer).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Splat.IEnableLogger).Assembly.Location)
        };

        return CSharpCompilation.Create(
            CompilationAssemblyName,
            [syntaxTree],
            references,
            new(OutputKind.DynamicallyLinkedLibrary));
    }

    /// <summary>Creates a cold generator driver, carrying no caches from a previous run.</summary>
    /// <returns>The generator driver.</returns>
    internal static GeneratorDriver CreateDriver() =>
        CSharpGeneratorDriver.Create(
            [new BindingGenerator().AsSourceGenerator()],
            null,
            new(LanguageVersion.CSharp10),
            null);
}
