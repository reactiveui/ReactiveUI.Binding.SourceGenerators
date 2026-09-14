// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Configs;
using ReactiveUI.Binding.Benchmarks.Mocks;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>WhenChanged with two threads changing the observed property at the same time.</summary>
[Config(typeof(NativeAotBenchmarkConfig))]
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
