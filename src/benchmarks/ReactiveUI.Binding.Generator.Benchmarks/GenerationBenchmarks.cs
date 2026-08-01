// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;

namespace ReactiveUI.Binding.Generator.Benchmarks;

/// <summary>Measures a full generation pass over a corpus of consumer code.</summary>
/// <remarks>
/// The driver is rebuilt per iteration so each measurement is a cold generation, which is what a consumer's
/// build actually pays. Reusing a primed driver would measure the incremental cache instead, and hoisting the
/// driver into setup would let one iteration's caches serve the next.
/// </remarks>
[MemoryDiagnoser]
public class GenerationBenchmarks
{
    /// <summary>The corpus compilation, built once per parameter set.</summary>
    private Compilation _compilation = null!;

    /// <summary>Gets or sets how many view-model and view pairs the corpus holds.</summary>
    [Params(1, 16, 64)]
    public int Pairs { get; set; }

    /// <summary>
    /// Builds the corpus compilation once per parameter set. Loading a framework's worth of metadata
    /// references costs far more than a generation pass and is work the host build does once, so measuring it
    /// per iteration would bury what this benchmark is for.
    /// </summary>
    [GlobalSetup]
    public void Setup() => _compilation = GeneratorHarness.BuildCompilation(GeneratorCorpus.Build(Pairs));

    /// <summary>Runs a whole cold generation: syntax scan, extraction, and emission.</summary>
    /// <returns>The number of generated characters, returned so the work cannot be optimized away.</returns>
    [Benchmark]
    public int Generate()
    {
        // A fresh driver per iteration: a reused one would serve the next iteration from its caches and
        // measure the incremental path rather than the cold generation a consumer's build pays for.
        var driver = GeneratorHarness.CreateDriver();
        var result = driver.RunGenerators(_compilation).GetRunResult();

        var characters = 0;
        foreach (var generated in result.Results[0].GeneratedSources)
        {
            characters += generated.SourceText.Length;
        }

        // A corpus that stopped matching the APIs would generate nothing and quietly turn this into a
        // measurement of driver overhead, so refuse to report a number for it.
        return characters == 0
            ? throw new InvalidOperationException("The corpus generated no source; the benchmark is measuring nothing.")
            : characters;
    }
}
