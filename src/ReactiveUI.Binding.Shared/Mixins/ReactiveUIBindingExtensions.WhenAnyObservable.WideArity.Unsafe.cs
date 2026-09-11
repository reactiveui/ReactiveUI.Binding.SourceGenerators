// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Resolves a WhenAnyObservable expression by reflection, for a call site the generator could not read.</summary>
/// <remarks>
/// The opt-in half of <c>WhenAnyObservable</c>. The unsuffixed overloads resolve at compile time and throw when no generated
/// dispatch claimed the call site; these walk the chain by reflection instead, and are annotated so a consumer
/// publishing trimmed or ahead-of-time sees the requirement at their own call site.
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Observes 1 observable property on the specified sender and switches to the latest observable.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <returns>An observable sequence that emits values from the latest observed observable.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);

        return ObservableChainHelpers.SwitchLatest(sender, obs1);
    }

    /// <summary>Observes 2 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);

        return Signal.Merge(
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2));
    }

    /// <summary>Observes 3 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);

        return Signal.Merge(
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3));
    }

    /// <summary>Observes 4 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);

        return Signal.Merge(
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4));
    }

    /// <summary>Observes 5 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);

        return Signal.Merge(
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5));
    }

    /// <summary>Observes 6 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);

        return Signal.Merge(
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
            sender.WhenAnyObservableUnsafe(obs6));
    }

    /// <summary>Observes 7 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);
        ArgumentExceptionHelper.ThrowIfNull(obs7);

        return Signal.Merge(
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
            sender.WhenAnyObservableUnsafe(obs6),
            sender.WhenAnyObservableUnsafe(obs7));
    }

    /// <summary>Observes 8 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);
        ArgumentExceptionHelper.ThrowIfNull(obs7);
        ArgumentExceptionHelper.ThrowIfNull(obs8);

        return Signal.Merge(
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
            sender.WhenAnyObservableUnsafe(obs6),
            sender.WhenAnyObservableUnsafe(obs7),
            sender.WhenAnyObservableUnsafe(obs8));
    }

    /// <summary>Observes 9 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="obs9">An expression that selects observable property 9 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        Expression<Func<TSender, IObservable<TRet>?>> obs9)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);
        ArgumentExceptionHelper.ThrowIfNull(obs7);
        ArgumentExceptionHelper.ThrowIfNull(obs8);
        ArgumentExceptionHelper.ThrowIfNull(obs9);

        return Signal.Merge(
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
            sender.WhenAnyObservableUnsafe(obs6),
            sender.WhenAnyObservableUnsafe(obs7),
            sender.WhenAnyObservableUnsafe(obs8),
            sender.WhenAnyObservableUnsafe(obs9));
    }

    /// <summary>Observes 10 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="obs9">An expression that selects observable property 9 to observe.</param>
    /// <param name="obs10">An expression that selects observable property 10 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        Expression<Func<TSender, IObservable<TRet>?>> obs9,
        Expression<Func<TSender, IObservable<TRet>?>> obs10)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);
        ArgumentExceptionHelper.ThrowIfNull(obs7);
        ArgumentExceptionHelper.ThrowIfNull(obs8);
        ArgumentExceptionHelper.ThrowIfNull(obs9);
        ArgumentExceptionHelper.ThrowIfNull(obs10);

        return Signal.Merge(
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
            sender.WhenAnyObservableUnsafe(obs6),
            sender.WhenAnyObservableUnsafe(obs7),
            sender.WhenAnyObservableUnsafe(obs8),
            sender.WhenAnyObservableUnsafe(obs9),
            sender.WhenAnyObservableUnsafe(obs10));
    }

    /// <summary>Observes 11 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="obs9">An expression that selects observable property 9 to observe.</param>
    /// <param name="obs10">An expression that selects observable property 10 to observe.</param>
    /// <param name="obs11">An expression that selects observable property 11 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        Expression<Func<TSender, IObservable<TRet>?>> obs9,
        Expression<Func<TSender, IObservable<TRet>?>> obs10,
        Expression<Func<TSender, IObservable<TRet>?>> obs11)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);
        ArgumentExceptionHelper.ThrowIfNull(obs7);
        ArgumentExceptionHelper.ThrowIfNull(obs8);
        ArgumentExceptionHelper.ThrowIfNull(obs9);
        ArgumentExceptionHelper.ThrowIfNull(obs10);
        ArgumentExceptionHelper.ThrowIfNull(obs11);

        return Signal.Merge(
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
            sender.WhenAnyObservableUnsafe(obs6),
            sender.WhenAnyObservableUnsafe(obs7),
            sender.WhenAnyObservableUnsafe(obs8),
            sender.WhenAnyObservableUnsafe(obs9),
            sender.WhenAnyObservableUnsafe(obs10),
            sender.WhenAnyObservableUnsafe(obs11));
    }

    /// <summary>Observes 12 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="obs9">An expression that selects observable property 9 to observe.</param>
    /// <param name="obs10">An expression that selects observable property 10 to observe.</param>
    /// <param name="obs11">An expression that selects observable property 11 to observe.</param>
    /// <param name="obs12">An expression that selects observable property 12 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        Expression<Func<TSender, IObservable<TRet>?>> obs9,
        Expression<Func<TSender, IObservable<TRet>?>> obs10,
        Expression<Func<TSender, IObservable<TRet>?>> obs11,
        Expression<Func<TSender, IObservable<TRet>?>> obs12)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);
        ArgumentExceptionHelper.ThrowIfNull(obs7);
        ArgumentExceptionHelper.ThrowIfNull(obs8);
        ArgumentExceptionHelper.ThrowIfNull(obs9);
        ArgumentExceptionHelper.ThrowIfNull(obs10);
        ArgumentExceptionHelper.ThrowIfNull(obs11);
        ArgumentExceptionHelper.ThrowIfNull(obs12);

        return Signal.Merge(
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
            sender.WhenAnyObservableUnsafe(obs6),
            sender.WhenAnyObservableUnsafe(obs7),
            sender.WhenAnyObservableUnsafe(obs8),
            sender.WhenAnyObservableUnsafe(obs9),
            sender.WhenAnyObservableUnsafe(obs10),
            sender.WhenAnyObservableUnsafe(obs11),
            sender.WhenAnyObservableUnsafe(obs12));
    }

    /// <summary>
    /// Observes 2 observable properties with different types on the specified sender and applies a selector to the combined latest values.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <returns>An observable sequence of selector results.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet, T1, T2>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Func<T1?, T2?, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = ObservableChainHelpers.SwitchLatest(sender, obs1);
        var o2 = ObservableChainHelpers.SwitchLatest(sender, obs2);
        return CombineLatestObservable.Create(
            o1,
            o2,
            (v1, v2) => selector(v1, v2));
    }
}
