// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Concurrency;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using ReactiveUI.Binding;

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
    private const int PropertyChangeCount = 1000;

    private BenchmarkViewModel _source = null!;
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
    /// Triggers source generator dispatch entry for BenchmarkView.DisplayName.
    /// Required for the reverse observation path in BindTwoWay.
    /// </summary>
    internal static IObservable<string> TriggerViewGeneration(BenchmarkView view)
        => view.WhenChanged(x => x.DisplayName);
}
