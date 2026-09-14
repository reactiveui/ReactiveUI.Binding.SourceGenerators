// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>WhenChanged with two threads changing the observed property at the same time.</summary>
#if BENCH_NETFX
[SimpleJob(RuntimeMoniker.Net462)]
#endif
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[SimpleJob(RuntimeMoniker.Net11_0)]
[SimpleJob(RuntimeMoniker.NativeAot10_0, id: nameof(RuntimeMoniker.NativeAot10_0))]
[SimpleJob(RuntimeMoniker.NativeAot11_0, id: nameof(RuntimeMoniker.NativeAot11_0))]
[MemoryDiagnoser]
#if !BENCH_NETFX
[EventPipeProfiler(EventPipeProfile.GcVerbose)]
#endif
[MarkdownExporterAttribute.GitHub]
public class WhenChangedContentionBenchmark
{
    /// <summary>How many property changes each writer drives.</summary>
    private const int PropertyChangeCount = 1_000;

    /// <summary>The view model under observation.</summary>
    private BenchmarkViewModel _vm = null!;

    /// <summary>Builds a fresh view model before each iteration.</summary>
    [IterationSetup]
    public void Setup() =>
        _vm = new() { Name = "Initial" };

    /// <summary>Two writers on separate threads, each driving N changes through one subscription.</summary>
    [Benchmark(Description = "Two Writers")]
    public void TwoWriters()
    {
        var last = string.Empty;
        using var sub = _vm.WhenChanged(x => x.Name)
            .Subscribe(v => last = v);

        Parallel.Invoke(
            () => Write("A_"),
            () => Write("B_"));
    }

    /// <summary>Drives N changes, each to a value the other writer never uses.</summary>
    /// <param name="prefix">The prefix that keeps this writer's values distinct.</param>
    private void Write(string prefix)
    {
        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = prefix + i;
        }
    }
}
