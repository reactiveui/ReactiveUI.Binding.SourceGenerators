// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Source-generated BindTo benchmarks, which write a stream's values into a target property.</summary>
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
public class BindToBenchmark
{
    /// <summary>How many values each benchmark pushes through one binding.</summary>
    private const int ValueCount = 1_000;

    /// <summary>The stream the binding reads.</summary>
    private BenchmarkSource<string> _source = null!;

    /// <summary>The binding's target.</summary>
    private BenchmarkView _view = null!;

    /// <summary>Builds a fresh stream and target before each iteration.</summary>
    [IterationSetup]
    public void Setup()
    {
        _source = new();
        _view = new();
    }

    /// <summary>One binding, driven through N values.</summary>
    [Benchmark(Description = "BindTo")]
    public void Standard()
    {
        using var binding = _source.BindTo(_view, v => v.DisplayName);

        for (var i = 0; i < ValueCount; i++)
        {
            _source.Push($"Value_{i}");
        }
    }

    /// <summary>Creating the binding, with no values pushed.</summary>
    [Benchmark(Description = "First Binding")]
    public void FirstBinding()
    {
        using var binding = _source.BindTo(_view, v => v.DisplayName);
    }
}
