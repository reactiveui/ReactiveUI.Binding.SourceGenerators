// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Source-generated InvokeCommand benchmarks, which run a command with each value a stream produces.</summary>
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
public class InvokeCommandBenchmark
{
    /// <summary>How many values each benchmark pushes through one invocation.</summary>
    private const int ValueCount = 1_000;

    /// <summary>The stream driving the command.</summary>
    private BenchmarkSource<string> _source = null!;

    /// <summary>The view model holding the command.</summary>
    private BenchmarkViewModel _vm = null!;

    /// <summary>The command being run.</summary>
    private BenchmarkCommand _command = null!;

    /// <summary>Builds a fresh stream, view model and command before each iteration.</summary>
    [IterationSetup]
    public void Setup()
    {
        _source = new();
        _command = new();
        _vm = new() { Name = "Initial", Run = _command };
    }

    /// <summary>One invocation, driven through N values.</summary>
    [Benchmark(Description = "InvokeCommand")]
    public void Standard()
    {
        using var invocation = _source.InvokeCommand(_vm, x => x.Run);

        for (var i = 0; i < ValueCount; i++)
        {
            _source.Push($"Value_{i}");
        }
    }

    /// <summary>Wiring the invocation up, with no values pushed.</summary>
    [Benchmark(Description = "First Invocation")]
    public void FirstInvocation()
    {
        using var invocation = _source.InvokeCommand(_vm, x => x.Run);
    }
}
