// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using ReactiveUI.Binding.Benchmarks.Configs;
using ReactiveUI.Binding.Benchmarks.Mocks;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>Source-generated OneWayBind benchmarks, which read against the ReactiveUI baseline of the same name.</summary>
[Config(typeof(NativeAotBenchmarkConfig))]
public class OneWayBindBenchmark
{
    /// <summary>How many property changes each benchmark drives through one binding.</summary>
    private const int PropertyChangeCount = 1_000;

    /// <summary>The binding's source.</summary>
    private BenchmarkViewModel _vm = null!;

    /// <summary>The binding's target.</summary>
    private BenchmarkView _view = null!;

    /// <summary>Builds a fresh source and target before each iteration.</summary>
    [IterationSetup]
    public void Setup()
    {
        _vm = new() { Name = "Initial", Age = 0, Child = new() { Value = "ChildInitial" } };
        _view = new() { ViewModel = _vm };
    }

    /// <summary>One binding, driven through N changes.</summary>
    [Benchmark(Description = "OneWayBind")]
    public void Standard()
    {
        using var binding = _view.OneWayBind(_vm, x => x.Name, v => v.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
        }
    }

    /// <summary>Creating the binding and writing its first value, with no changes driven.</summary>
    /// <returns>The value written to the target.</returns>
    [Benchmark(Description = "First Binding")]
    public string FirstBinding()
    {
        using var binding = _view.OneWayBind(_vm, x => x.Name, v => v.DisplayName);
        return _view.DisplayName;
    }
}
