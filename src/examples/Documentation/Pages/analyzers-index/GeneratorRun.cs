// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.Binding.SourceGenerators;

namespace ReactiveUI.Binding.Documentation.AnalyzersIndex;

/// <summary>
/// Runs the package's source generator over a piece of source text with the MSBuild properties and compiler
/// features a build would give it, and returns the C# the generator writes.
/// </summary>
public static class GeneratorRun
{
    /// <summary>The compiler references: every assembly the running program can load.</summary>
    private static readonly ImmutableArray<MetadataReference> References = LoadReferences();

    /// <summary>Runs the generator over source text.</summary>
    /// <param name="source">The C# source to compile.</param>
    /// <param name="buildProperties">The MSBuild properties the build exposes to the generator, by name.</param>
    /// <param name="compilerFeatures">The compiler features the build sets, by name.</param>
    /// <returns>Every file the generator wrote, one after the other.</returns>
    public static string Generate(
        string source,
        IReadOnlyDictionary<string, string> buildProperties,
        IEnumerable<KeyValuePair<string, string>> compilerFeatures)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest).WithFeatures(compilerFeatures);
        var compilation = CSharpCompilation.Create(
            "TodoApp",
            [CSharpSyntaxTree.ParseText(source, parseOptions)],
            References,
            new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new BindingGenerator().AsSourceGenerator()],
            additionalTexts: null,
            parseOptions,
            new BuildPropertiesOptionsProvider(buildProperties));

        driver = driver.RunGenerators(compilation);

        StringBuilder generated = new();
        foreach (var result in driver.GetRunResult().Results)
        {
            foreach (var file in result.GeneratedSources)
            {
                _ = generated.Append(file.SourceText.ToString());
            }
        }

        return generated.ToString();
    }

    /// <summary>Loads a reference to every assembly the running program can load.</summary>
    /// <returns>The references.</returns>
    private static ImmutableArray<MetadataReference> LoadReferences()
    {
        var platformAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        var builder = ImmutableArray.CreateBuilder<MetadataReference>();

        foreach (var path in platformAssemblies.Split(Path.PathSeparator))
        {
            builder.Add(MetadataReference.CreateFromFile(path));
        }

        return builder.ToImmutable();
    }
}
