// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>ReactiveUI expression-tree WhenAnyValue benchmarks for comparison.</summary>
#if BENCH_NETFX
[SimpleJob(RuntimeMoniker.Net462)]
#endif
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[SimpleJob(RuntimeMoniker.Net11_0)]
[MemoryDiagnoser]
#if !BENCH_NETFX
[EventPipeProfiler(EventPipeProfile.GcVerbose)]
#endif
[MarkdownExporterAttribute.GitHub]
[DebuggerDisplay("Expression-tree observation over {PropertyChangeCount} changes")]
public class ReactiveUIObservationBenchmark
{
    /// <summary>The number of property changes to fire during each benchmark iteration.</summary>
    private const int PropertyChangeCount = 1_000;

    /// <summary>The view model instance used for observation benchmarks.</summary>
    private BenchmarkViewModel _vm = null!;

    /// <summary>Initializes static members of the <see cref="ReactiveUIObservationBenchmark"/> class. Ensures ReactiveUI is configured before any benchmarks run.</summary>
    static ReactiveUIObservationBenchmark() => ModuleInitializer.EnsureInitialized();

    /// <summary>Sets up a fresh view model before each benchmark iteration.</summary>
    [IterationSetup]
    public void Setup() =>
        _vm = new() { Name = "Initial", Age = 0, Child = new() { Value = "ChildInitial" } };

    /// <summary>Expression-tree single property observation: subscribe, fire N changes, dispose.</summary>
    [Benchmark(Description = "Single Property")]
    public void SingleProperty()
    {
        var last = string.Empty;
        using var sub = _vm.WhenAnyValue(x => x.Name)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
        }
    }

    /// <summary>Expression-tree deep chain observation: subscribe, fire N changes, dispose.</summary>
    [Benchmark(Description = "Deep Chain")]
    public void DeepChain()
    {
        var last = string.Empty;
        using var sub = _vm.WhenAnyValue(x => x.Child.Value)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Child.Value = $"Value_{i}";
        }
    }

    /// <summary>Expression-tree multi-property observation: subscribe, fire N changes, dispose.</summary>
    [Benchmark(Description = "Two Properties")]
    public void TwoProperties()
    {
        (string Name, int Age) last = default;
        using var sub = _vm.WhenAnyValue(x => x.Name, x => x.Age, static (name, age) => (name, age))
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
            _vm.Age = i;
        }
    }

    /// <summary>Cold start: subscribe, read initial value, dispose. Includes expression compilation overhead.</summary>
    /// <returns>The observed value.</returns>
    [Benchmark(Description = "First Observation")]
    public string FirstObservation()
    {
        var result = string.Empty;
        using var sub = _vm.WhenAnyValue(x => x.Name)
            .Subscribe(v => result = v);
        return result;
    }
}
