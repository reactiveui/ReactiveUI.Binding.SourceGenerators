// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using ReactiveUI.Binding;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>
/// Source-generated WhenChanged benchmarks using lightweight observables.
/// </summary>
////[SimpleJob(RuntimeMoniker.Net462)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[SimpleJob(RuntimeMoniker.NativeAot10_0, id: nameof(RuntimeMoniker.NativeAot10_0))]
[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub]
public class WhenChangedBenchmark
{
    private const int PropertyChangeCount = 1000;

    private BenchmarkViewModel _vm = null!;

    /// <summary>
    /// Sets up a fresh view model before each benchmark iteration.
    /// </summary>
    [IterationSetup]
    public void Setup()
    {
        _vm = new BenchmarkViewModel
        {
            Name = "Initial",
            Age = 0,
            Child = new BenchmarkChildViewModel { Value = "ChildInitial" },
        };
    }

    /// <summary>
    /// Single property observation: subscribe, fire N changes, dispose.
    /// </summary>
    [Benchmark(Description = "Single Property")]
    public void SingleProperty()
    {
        var last = string.Empty;
        using var sub = _vm.WhenChanged(x => x.Name)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
        }
    }

    /// <summary>
    /// Deep chain observation: subscribe, fire N changes on nested property, dispose.
    /// </summary>
    [Benchmark(Description = "Deep Chain")]
    public void DeepChain()
    {
        var last = string.Empty;
        using var sub = _vm.WhenChanged(x => x.Child.Value)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Child.Value = $"Value_{i}";
        }
    }

    /// <summary>
    /// Multi-property observation with CombineLatest: subscribe, fire N changes, dispose.
    /// </summary>
    [Benchmark(Description = "Two Properties")]
    public void TwoProperties()
    {
        (string name, int age) last = default;
        using var sub = _vm.WhenChanged(x => x.Name, x => x.Age)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
            _vm.Age = i;
        }
    }

    /// <summary>
    /// Cold start: subscribe, read initial value, dispose. No property changes.
    /// </summary>
    /// <returns>The observed value.</returns>
    [Benchmark(Description = "First Observation")]
    public string FirstObservation()
    {
        var result = string.Empty;
        using var sub = _vm.WhenChanged(x => x.Name)
            .Subscribe(v => result = v);
        return result;
    }
}
