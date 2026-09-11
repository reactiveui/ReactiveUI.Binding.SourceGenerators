// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>
/// Source-generated view-first two-way binding. Both directions run through one pipeline that is routed once
/// and applied once, and the changes it wrote are published to whoever subscribes to the binding - so the cost
/// of making one, of driving it from either side, and of watching what it did are all measured here.
/// </summary>
#if BENCH_NETFX
[SimpleJob(RuntimeMoniker.Net462)]
#endif
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[SimpleJob(RuntimeMoniker.Net11_0)]
[SimpleJob(RuntimeMoniker.NativeAot10_0, id: nameof(RuntimeMoniker.NativeAot10_0))]
[SimpleJob(RuntimeMoniker.NativeAot11_0, id: nameof(RuntimeMoniker.NativeAot11_0))]
[MemoryDiagnoser]
[EventPipeProfiler(EventPipeProfile.GcVerbose)]
[MarkdownExporterAttribute.GitHub]
public class BindBenchmark
{
    /// <summary>Represents the number of property change events to be triggered during the benchmark tests.</summary>
    private const int PropertyChangeCount = 1_000;

    /// <summary>The modulus used to alternate updates between the view model and the view each iteration.</summary>
    private const int AlternationModulus = 2;

    /// <summary>The view model used for binding benchmarks.</summary>
    private BenchmarkViewModel _viewModel = null!;

    /// <summary>The view used for binding benchmarks.</summary>
    private BenchmarkView _view = null!;

    /// <summary>Sets up a fresh view model and view before each benchmark iteration.</summary>
    [IterationSetup]
    public void Setup()
    {
        _viewModel = new() { Name = "Initial", Age = 0 };
        _view = new() { ViewModel = null };
    }

    /// <summary>Binding once and driving it from the view model, which is the ordinary direction.</summary>
    [Benchmark(Description = "Bind")]
    public void Standard()
    {
        using var binding = _view.Bind(_viewModel, x => x.Name, x => x.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _viewModel.Name = $"Name_{i}";
        }
    }

    /// <summary>Driving the binding from both sides, which is what the merged pipeline exists to order.</summary>
    [Benchmark(Description = "Bind bidirectional")]
    public void Bidirectional()
    {
        using var binding = _view.Bind(_viewModel, x => x.Name, x => x.DisplayName);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            if (i % AlternationModulus == 0)
            {
                _viewModel.Name = $"FromViewModel_{i}";
            }
            else
            {
                _view.DisplayName = $"FromView_{i}";
            }
        }
    }

    /// <summary>
    /// Watching what the binding wrote. A subscriber shares the binding's own upstream subscription, so this
    /// measures whether observing a binding costs a second set of property handlers.
    /// </summary>
    [Benchmark(Description = "Bind + observed changes")]
    public void WithObservedChanges()
    {
        using var binding = _view.Bind(_viewModel, x => x.Name, x => x.DisplayName);
        using var watching = binding.Changed.Subscribe(new CountingObserver());

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _viewModel.Name = $"Name_{i}";
        }
    }

    /// <summary>Counts what a binding reported without allocating per change.</summary>
    private sealed class CountingObserver : IObserver<BindingChange>
    {
        /// <summary>Gets how many changes were reported.</summary>
        public int Count { get; private set; }

        /// <inheritdoc/>
        public void OnNext(BindingChange value) => Count++;

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
        }

        /// <inheritdoc/>
        public void OnCompleted()
        {
        }
    }
}
