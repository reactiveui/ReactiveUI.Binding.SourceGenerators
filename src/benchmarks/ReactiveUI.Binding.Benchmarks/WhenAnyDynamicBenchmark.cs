// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using ReactiveUI.Binding.Builder;

namespace ReactiveUI.Binding.Benchmarks;

/// <summary>
/// Reflection-walked observation benchmarks, and what the generated observation of the same chain costs.
/// The pair is the choice a caller actually makes: a chain named by an expression built at run time cannot
/// be resolved at compile time, and this says what that buys and what it costs.
/// </summary>
/// <remarks>
/// No NativeAOT job: these overloads walk members by reflection and say so with
/// <see cref="RequiresUnreferencedCodeAttribute"/>, so an ahead-of-time published run is not a
/// configuration they support.
/// </remarks>
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
[EventPipeProfiler(EventPipeProfile.GcVerbose)]
[MarkdownExporterAttribute.GitHub]
public class WhenAnyDynamicBenchmark
{
    /// <summary>Represents the number of property change events to be triggered during the benchmark tests.</summary>
    private const int PropertyChangeCount = 1_000;

    /// <summary>Names the chain reaching one property.</summary>
    private static readonly Expression NameChain = Chain(x => x.Name);

    /// <summary>Names the chain reaching a second property, so an arity above one has something to combine.</summary>
    private static readonly Expression AgeChain = Chain(x => x.Age);

    /// <summary>Names the chain reaching through an intermediate.</summary>
    private static readonly Expression ChildValueChain = Chain(x => x.Child.Value);

    /// <summary>The view model instance used for observation benchmarks.</summary>
    private BenchmarkViewModel _vm = null!;

    /// <summary>Registers the observation plugins the reflection walk resolves each link through.</summary>
    [GlobalSetup]
    public void Register()
    {
        var builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();
    }

    /// <summary>Sets up a fresh view model before each benchmark iteration.</summary>
    [IterationSetup]
    public void Setup() =>
        _vm = new() { Name = "Initial", Age = 0, Child = new() { Value = "ChildInitial" } };

    /// <summary>One chain: subscribe, fire N changes, dispose.</summary>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    [Benchmark(Description = "Single Chain")]
    public void SingleChain()
    {
        object? last = null;
        using var sub = _vm.WhenAnyDynamic(NameChain, static c1 => c1.Value)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
        }
    }

    /// <summary>Two chains combined: subscribe, fire N changes on each, dispose.</summary>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    [Benchmark(Description = "Two Chains")]
    public void TwoChains()
    {
        object? last = null;
        using var sub = _vm.WhenAnyDynamic(NameChain, AgeChain, static (c1, c2) => c1.Value ?? c2.Value)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
            _vm.Age = i;
        }
    }

    /// <summary>A chain through an intermediate: subscribe, fire N changes on the leaf, dispose.</summary>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    [Benchmark(Description = "Deep Chain")]
    public void DeepChain()
    {
        object? last = null;
        using var sub = _vm.WhenAnyDynamic(ChildValueChain, static c1 => c1.Value)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Child.Value = $"Value_{i}";
        }
    }

    /// <summary>Cold start: subscribe, read the initial value, dispose. No property changes.</summary>
    /// <returns>The observed value.</returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    [Benchmark(Description = "First Observation")]
    public object? FirstObservation()
    {
        object? result = null;
        using var sub = _vm.WhenAnyDynamic(NameChain, static c1 => c1.Value)
            .Subscribe(v => result = v);
        return result;
    }

    /// <summary>The same single chain resolved at compile time, which is what the reflection walk is weighed against.</summary>
    [Benchmark(Description = "Single Chain (generated)", Baseline = true)]
    public void SingleChainGenerated()
    {
        var last = string.Empty;
        using var sub = _vm.WhenChanged(x => x.Name)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
        }
    }

    /// <summary>Names a property chain the way a caller building one at run time hands it over.</summary>
    /// <typeparam name="TValue">The type the chain ends at.</typeparam>
    /// <param name="property">The chain to name.</param>
    /// <returns>The expression body, which is what these overloads take.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Expression Chain<TValue>(Expression<Func<BenchmarkViewModel, TValue>> property) =>
        property.Body;
}
