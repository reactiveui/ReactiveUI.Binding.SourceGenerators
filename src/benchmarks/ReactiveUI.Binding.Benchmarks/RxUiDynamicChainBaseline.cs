// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using ReactiveUI;
using ReactiveUI.Builder;
using BenchmarkVm = ReactiveUI.Binding.Benchmarks.BenchmarkViewModel;

namespace RxUiDynamicChain;

/// <summary>
/// The same dynamic-chain scenarios as <c>WhenAnyDynamicBenchmark</c>, run against ReactiveUI's own engine so
/// the two are read side by side.
/// </summary>
/// <remarks>
/// Declared outside the <c>ReactiveUI.Binding</c> namespace on purpose. Extension lookup walks the enclosing
/// namespaces from the inside out and stops at the first level offering a candidate, so a class nested under
/// <c>ReactiveUI.Binding</c> reaches this library's overloads and never consults ReactiveUI's. The view model
/// arrives through a type alias rather than an import for the same reason: importing its namespace would put
/// both libraries' overloads at the outermost level and make every call ambiguous.
/// </remarks>
#if BENCH_NETFX
[SimpleJob(RuntimeMoniker.Net462)]
#endif
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[SimpleJob(RuntimeMoniker.Net11_0)]
[MemoryDiagnoser]
[EventPipeProfiler(EventPipeProfile.GcVerbose)]
[MarkdownExporterAttribute.GitHub]
#if NET8_0_OR_GREATER
[RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
#endif
public class RxUiDynamicChainBaseline
{
    /// <summary>Represents the number of property change events to be triggered during the benchmark tests.</summary>
    private const int PropertyChangeCount = 1_000;

    /// <summary>Names the chain reaching one property.</summary>
    private static readonly Expression NameChain = Chain(x => x.Name);

    /// <summary>Names the chain reaching a second property, so an arity above one has something to combine.</summary>
    private static readonly Expression AgeChain = Chain(x => x.Age);

    /// <summary>Names the chain reaching through an intermediate.</summary>
    private static readonly Expression ChildValueChain = Chain(x => x.Child.Value);

    /// <summary>Reads one observed change.</summary>
    /// <remarks>
    /// Typed rather than inferred so the binding is pinned: the parameter names ReactiveUI's
    /// <see cref="IObservedChange{TSender, TValue}"/>, so a call that resolved to this library's overload of
    /// the same name would not compile rather than quietly benchmarking the wrong engine.
    /// </remarks>
    private static readonly Func<IObservedChange<BenchmarkVm?, object?>, object?> ReadOne =
        static c1 => c1.Value;

    /// <summary>Reads whichever of two observed changes carries a value.</summary>
    private static readonly Func<IObservedChange<BenchmarkVm?, object?>, IObservedChange<BenchmarkVm?, object?>, object?> ReadEither =
        static (c1, c2) => c1.Value ?? c2.Value;

    /// <summary>The view model instance used for observation benchmarks.</summary>
    private BenchmarkVm _vm = null!;

    /// <summary>Registers the observation plugins ReactiveUI resolves each link through.</summary>
    [GlobalSetup]
    public void Register()
    {
        var builder = RxAppBuilder.CreateReactiveUIBuilder();
        _ = builder.WithCoreServices();
        _ = builder.BuildApp();
    }

    /// <summary>Sets up a fresh view model before each benchmark iteration.</summary>
    [IterationSetup]
    public void Setup() =>
        _vm = new() { Name = "Initial", Age = 0, Child = new() { Value = "ChildInitial" } };

    /// <summary>One chain: subscribe, fire N changes, dispose.</summary>
    [Benchmark(Description = "Single Chain")]
    public void SingleChain()
    {
        object? last = null;
        using var sub = _vm.WhenAnyDynamic(NameChain, ReadOne)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
        }
    }

    /// <summary>Two chains combined: subscribe, fire N changes on each, dispose.</summary>
    [Benchmark(Description = "Two Chains")]
    public void TwoChains()
    {
        object? last = null;
        using var sub = _vm.WhenAnyDynamic(NameChain, AgeChain, ReadEither)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Name = $"Name_{i}";
            _vm.Age = i;
        }
    }

    /// <summary>A chain through an intermediate: subscribe, fire N changes on the leaf, dispose.</summary>
    [Benchmark(Description = "Deep Chain")]
    public void DeepChain()
    {
        object? last = null;
        using var sub = _vm.WhenAnyDynamic(ChildValueChain, ReadOne)
            .Subscribe(v => last = v);

        for (var i = 0; i < PropertyChangeCount; i++)
        {
            _vm.Child.Value = $"Value_{i}";
        }
    }

    /// <summary>Cold start: subscribe, read the initial value, dispose. No property changes.</summary>
    /// <returns>The observed value.</returns>
    [Benchmark(Description = "First Observation")]
    public object? FirstObservation()
    {
        object? result = null;
        using var sub = _vm.WhenAnyDynamic(NameChain, ReadOne)
            .Subscribe(v => result = v);
        return result;
    }

    /// <summary>Names a property chain the way a caller building one at run time hands it over.</summary>
    /// <typeparam name="TValue">The type the chain ends at.</typeparam>
    /// <param name="property">The chain to name.</param>
    /// <returns>The expression body, which is what these overloads take.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Expression Chain<TValue>(Expression<Func<BenchmarkVm, TValue>> property) =>
        property.Body;
}
