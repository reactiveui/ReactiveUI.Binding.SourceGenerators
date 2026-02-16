// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Concurrency;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using ReactiveUI.Binding;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>
/// Source-generated BindOneWay benchmarks with and without scheduler.
/// </summary>
////[SimpleJob(RuntimeMoniker.Net462)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[SimpleJob(RuntimeMoniker.NativeAot10_0, id: nameof(RuntimeMoniker.NativeAot10_0))]
[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub]
public class BindOneWayBenchmark
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
    /// One-way binding without scheduler: setup, fire N changes, dispose.
    /// </summary>
    [Benchmark(Description = "BindOneWay")]
    public void Standard()
    {
        using var binding = _source.BindOneWay(_target, x => x.Name, x => x.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _source.Name = $"Name_{i}";
        }
    }

    /// <summary>
    /// One-way binding with ImmediateScheduler: measures ObserveOnObservable overhead.
    /// </summary>
    [Benchmark(Description = "BindOneWay + Scheduler")]
    public void WithScheduler()
    {
        using var binding = _source.BindOneWay(_target, x => x.Name, x => x.DisplayName, ImmediateScheduler.Instance);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _source.Name = $"Name_{i}";
        }
    }

    /// <summary>
    /// Cold start: create binding, dispose immediately. No property changes.
    /// </summary>
    [Benchmark(Description = "First Binding")]
    public void FirstBinding()
    {
        using var binding = _source.BindOneWay(_target, x => x.Name, x => x.DisplayName);
    }

    /// <summary>
    /// Setup and teardown cost of 10 consecutive bindings.
    /// </summary>
    [Benchmark(Description = "10x Setup/Teardown")]
    public void SetupTeardown()
    {
        var bindings = new IDisposable[10];

        for (var i = 0; i < 10; i++)
        {
            _source.Name = $"VM_{i}";
            bindings[i] = _source.BindOneWay(_target, x => x.Name, x => x.DisplayName);
        }

        for (var i = 0; i < 10; i++)
        {
            bindings[i].Dispose();
        }
    }
}
