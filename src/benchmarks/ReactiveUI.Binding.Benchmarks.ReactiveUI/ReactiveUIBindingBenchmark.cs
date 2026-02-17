// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using ReactiveUI;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>
/// ReactiveUI expression-tree binding benchmarks for comparison.
/// </summary>
////[SimpleJob(RuntimeMoniker.Net462)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub]
public class ReactiveUIBindingBenchmark
{
    private const int PropertyChangeCount = 1000;

    private BenchmarkViewModel _source = null!;
    private BenchmarkView _target = null!;

    static ReactiveUIBindingBenchmark() => ModuleInitializer.EnsureInitialized();

    /// <summary>
    /// Sets up fresh source and target objects before each benchmark iteration.
    /// </summary>
    [IterationSetup]
    public void Setup()
    {
        _source = new BenchmarkViewModel { Name = "Initial", Age = 0 };
        _target = new BenchmarkView { ViewModel = _source };
    }

    /// <summary>
    /// Expression-tree one-way binding: setup, fire N changes, dispose.
    /// </summary>
    [Benchmark(Description = "OneWayBind")]
    public void OneWayBind()
    {
        using var binding = _target.OneWayBind(_source, x => x.Name, x => x.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _source.Name = $"Name_{i}";
        }
    }

    /// <summary>
    /// Expression-tree two-way binding: setup, fire N source changes, dispose.
    /// </summary>
    [Benchmark(Description = "Bind")]
    public void Bind()
    {
        using var binding = _target.Bind(_source, x => x.Name, x => x.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _source.Name = $"Name_{i}";
        }
    }

    /// <summary>
    /// Cold start: create one-way binding, dispose immediately.
    /// </summary>
    [Benchmark(Description = "First OneWayBind")]
    public void FirstOneWayBind()
    {
        using var binding = _target.OneWayBind(_source, x => x.Name, x => x.DisplayName);
    }

    /// <summary>
    /// Cold start: create two-way binding, dispose immediately.
    /// </summary>
    [Benchmark(Description = "First Bind")]
    public void FirstBind()
    {
        using var binding = _target.Bind(_source, x => x.Name, x => x.DisplayName);
    }
}
