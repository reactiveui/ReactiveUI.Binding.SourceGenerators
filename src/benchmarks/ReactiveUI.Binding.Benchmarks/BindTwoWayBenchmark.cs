// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Concurrency;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>
/// Source-generated BindTwoWay benchmarks with and without scheduler.
/// </summary>
////[SimpleJob(RuntimeMoniker.Net462)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[SimpleJob(RuntimeMoniker.NativeAot10_0, id: nameof(RuntimeMoniker.NativeAot10_0))]
[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub]
public class BindTwoWayBenchmark
{
    /// <summary>
    /// Represents the number of property change events to be triggered during the benchmark tests.
    /// </summary>
    private const int PropertyChangeCount = 1000;

    /// <summary>
    /// The source and target view models used for binding benchmarks.
    /// </summary>
    private BenchmarkViewModel _source = null!;

    /// <summary>
    /// The target view used for binding benchmarks.
    /// </summary>
    private BenchmarkView _target = null!;

    /// <summary>
    /// Sets up fresh source and target objects before each benchmark iteration.
    /// </summary>
    [IterationSetup]
    public void Setup()
    {
        _source = new BenchmarkViewModel { Name = "Initial", Age = 0 };
        _target = new BenchmarkView();
    }

    /// <summary>
    /// Two-way binding without scheduler: setup, fire N source changes, dispose.
    /// </summary>
    [Benchmark(Description = "BindTwoWay")]
    public void Standard()
    {
        using var binding = _source.BindTwoWay(_target, x => x.Name, x => x.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _source.Name = $"Name_{i}";
        }
    }

    /// <summary>
    /// Two-way binding with ImmediateScheduler: measures ObserveOnObservable overhead on both directions.
    /// </summary>
    [Benchmark(Description = "BindTwoWay + Scheduler")]
    public void WithScheduler()
    {
        using var binding = _source.BindTwoWay(_target, x => x.Name, x => x.DisplayName, ImmediateScheduler.Instance);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _source.Name = $"Name_{i}";
        }
    }

    /// <summary>
    /// Two-way binding with alternating source and target changes.
    /// </summary>
    [Benchmark(Description = "Bidirectional")]
    public void Bidirectional()
    {
        using var binding = _source.BindTwoWay(_target, x => x.Name, x => x.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            if (i % 2 == 0)
            {
                _source.Name = $"FromSource_{i}";
            }
            else
            {
                _target.DisplayName = $"FromTarget_{i}";
            }
        }
    }

    /// <summary>
    /// Observes changes to the <see cref="BenchmarkView.DisplayName"/> property using ReactiveUI's
    /// expression-tree-based binding APIs. This method is essential for setting up the reverse
    /// observation path required in bi-directional bindings when benchmarking BindTwoWay.
    /// </summary>
    /// <param name="view">The instance of <see cref="BenchmarkView"/> whose <see cref="BenchmarkView.DisplayName"/> property changes are observed.</param>
    /// <returns>An observable sequence of <see cref="string"/> values that represent the changes to the <see cref="BenchmarkView.DisplayName"/> property.</returns>
    internal static IObservable<string> TriggerViewGeneration(BenchmarkView view)
        => view.WhenChanged(x => x.DisplayName);
}
