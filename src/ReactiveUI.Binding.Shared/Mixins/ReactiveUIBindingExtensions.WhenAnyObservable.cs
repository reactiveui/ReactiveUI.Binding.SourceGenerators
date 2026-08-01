// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Extension methods for observing properties that are themselves observables (WhenAnyObservable).
/// Automatically switches to the latest observable when the property value changes.
/// </summary>
public static partial class ReactiveUIBindingExtensions
{
#if NET8_0_OR_GREATER
    /// <summary>Observes 1 observable property on the specified sender and switches to the latest observable.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the latest observed observable.</returns>
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#else
    /// <summary>Observes 1 observable property on the specified sender and switches to the latest observable.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the latest observed observable.</returns>
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#endif
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);

        return sender.SubscribeToExpressionChain<TSender, IObservable<TRet>?>(
                obs1.Body,
                skipInitial: false)
            .Select(static x => x.Value ?? EmptyObservable<TRet>.Instance)
            .Switch();
    }

#if NET8_0_OR_GREATER
    /// <summary>Observes 2 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#else
    /// <summary>Observes 2 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#endif
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);

        return RxBindingExtensions.Merge(
            sender.WhenAnyObservable(obs1),
            sender.WhenAnyObservable(obs2));
    }

#if NET8_0_OR_GREATER
    /// <summary>Observes 3 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#else
    /// <summary>Observes 3 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#endif
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);

        return RxBindingExtensions.Merge(
            sender.WhenAnyObservable(obs1),
            sender.WhenAnyObservable(obs2),
            sender.WhenAnyObservable(obs3));
    }

#if NET8_0_OR_GREATER
    /// <summary>Observes 4 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#else
    /// <summary>Observes 4 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#endif
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);

        return RxBindingExtensions.Merge(
            sender.WhenAnyObservable(obs1),
            sender.WhenAnyObservable(obs2),
            sender.WhenAnyObservable(obs3),
            sender.WhenAnyObservable(obs4));
    }

#if NET8_0_OR_GREATER
    /// <summary>Observes 5 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#else
    /// <summary>Observes 5 observable properties on the specified sender and merges the switched observables.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#endif
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);

        return RxBindingExtensions.Merge(
            sender.WhenAnyObservable(obs1),
            sender.WhenAnyObservable(obs2),
            sender.WhenAnyObservable(obs3),
            sender.WhenAnyObservable(obs4),
            sender.WhenAnyObservable(obs5));
    }

#if NET8_0_OR_GREATER
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
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerArgumentExpression("obs6")] string obs6Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#else
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
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#endif
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);

        return RxBindingExtensions.Merge(
            sender.WhenAnyObservable(obs1),
            sender.WhenAnyObservable(obs2),
            sender.WhenAnyObservable(obs3),
            sender.WhenAnyObservable(obs4),
            sender.WhenAnyObservable(obs5),
            sender.WhenAnyObservable(obs6));
    }

#if NET8_0_OR_GREATER
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
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="obs7Expression">The caller argument expression for <paramref name="obs7"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerArgumentExpression("obs6")] string obs6Expression = "",
        [CallerArgumentExpression("obs7")] string obs7Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#else
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
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#endif
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);
        ArgumentExceptionHelper.ThrowIfNull(obs7);

        return RxBindingExtensions.Merge(
            sender.WhenAnyObservable(obs1),
            sender.WhenAnyObservable(obs2),
            sender.WhenAnyObservable(obs3),
            sender.WhenAnyObservable(obs4),
            sender.WhenAnyObservable(obs5),
            sender.WhenAnyObservable(obs6),
            sender.WhenAnyObservable(obs7));
    }

#if NET8_0_OR_GREATER
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
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="obs7Expression">The caller argument expression for <paramref name="obs7"/>. Auto-populated by the compiler.</param>
    /// <param name="obs8Expression">The caller argument expression for <paramref name="obs8"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerArgumentExpression("obs6")] string obs6Expression = "",
        [CallerArgumentExpression("obs7")] string obs7Expression = "",
        [CallerArgumentExpression("obs8")] string obs8Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#else
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
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the N-property dispatch stub and its CallerInfo contract")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#endif
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

        return RxBindingExtensions.Merge(
            sender.WhenAnyObservable(obs1),
            sender.WhenAnyObservable(obs2),
            sender.WhenAnyObservable(obs3),
            sender.WhenAnyObservable(obs4),
            sender.WhenAnyObservable(obs5),
            sender.WhenAnyObservable(obs6),
            sender.WhenAnyObservable(obs7),
            sender.WhenAnyObservable(obs8));
    }

#if NET8_0_OR_GREATER
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
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="obs7Expression">The caller argument expression for <paramref name="obs7"/>. Auto-populated by the compiler.</param>
    /// <param name="obs8Expression">The caller argument expression for <paramref name="obs8"/>. Auto-populated by the compiler.</param>
    /// <param name="obs9Expression">The caller argument expression for <paramref name="obs9"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
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
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerArgumentExpression("obs6")] string obs6Expression = "",
        [CallerArgumentExpression("obs7")] string obs7Expression = "",
        [CallerArgumentExpression("obs8")] string obs8Expression = "",
        [CallerArgumentExpression("obs9")] string obs9Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#else
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
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the N-property dispatch stub and its CallerInfo contract")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
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
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#endif
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

        return RxBindingExtensions.Merge(
            sender.WhenAnyObservable(obs1),
            sender.WhenAnyObservable(obs2),
            sender.WhenAnyObservable(obs3),
            sender.WhenAnyObservable(obs4),
            sender.WhenAnyObservable(obs5),
            sender.WhenAnyObservable(obs6),
            sender.WhenAnyObservable(obs7),
            sender.WhenAnyObservable(obs8),
            sender.WhenAnyObservable(obs9));
    }

#if NET8_0_OR_GREATER
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
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="obs7Expression">The caller argument expression for <paramref name="obs7"/>. Auto-populated by the compiler.</param>
    /// <param name="obs8Expression">The caller argument expression for <paramref name="obs8"/>. Auto-populated by the compiler.</param>
    /// <param name="obs9Expression">The caller argument expression for <paramref name="obs9"/>. Auto-populated by the compiler.</param>
    /// <param name="obs10Expression">The caller argument expression for <paramref name="obs10"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
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
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerArgumentExpression("obs6")] string obs6Expression = "",
        [CallerArgumentExpression("obs7")] string obs7Expression = "",
        [CallerArgumentExpression("obs8")] string obs8Expression = "",
        [CallerArgumentExpression("obs9")] string obs9Expression = "",
        [CallerArgumentExpression("obs10")] string obs10Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#else
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
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the N-property dispatch stub and its CallerInfo contract")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
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
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#endif
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

        return RxBindingExtensions.Merge(
            sender.WhenAnyObservable(obs1),
            sender.WhenAnyObservable(obs2),
            sender.WhenAnyObservable(obs3),
            sender.WhenAnyObservable(obs4),
            sender.WhenAnyObservable(obs5),
            sender.WhenAnyObservable(obs6),
            sender.WhenAnyObservable(obs7),
            sender.WhenAnyObservable(obs8),
            sender.WhenAnyObservable(obs9),
            sender.WhenAnyObservable(obs10));
    }

#if NET8_0_OR_GREATER
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
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="obs7Expression">The caller argument expression for <paramref name="obs7"/>. Auto-populated by the compiler.</param>
    /// <param name="obs8Expression">The caller argument expression for <paramref name="obs8"/>. Auto-populated by the compiler.</param>
    /// <param name="obs9Expression">The caller argument expression for <paramref name="obs9"/>. Auto-populated by the compiler.</param>
    /// <param name="obs10Expression">The caller argument expression for <paramref name="obs10"/>. Auto-populated by the compiler.</param>
    /// <param name="obs11Expression">The caller argument expression for <paramref name="obs11"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
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
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerArgumentExpression("obs6")] string obs6Expression = "",
        [CallerArgumentExpression("obs7")] string obs7Expression = "",
        [CallerArgumentExpression("obs8")] string obs8Expression = "",
        [CallerArgumentExpression("obs9")] string obs9Expression = "",
        [CallerArgumentExpression("obs10")] string obs10Expression = "",
        [CallerArgumentExpression("obs11")] string obs11Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#else
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
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the N-property dispatch stub and its CallerInfo contract")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
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
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#endif
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

        return RxBindingExtensions.Merge(
            sender.WhenAnyObservable(obs1),
            sender.WhenAnyObservable(obs2),
            sender.WhenAnyObservable(obs3),
            sender.WhenAnyObservable(obs4),
            sender.WhenAnyObservable(obs5),
            sender.WhenAnyObservable(obs6),
            sender.WhenAnyObservable(obs7),
            sender.WhenAnyObservable(obs8),
            sender.WhenAnyObservable(obs9),
            sender.WhenAnyObservable(obs10),
            sender.WhenAnyObservable(obs11));
    }

#if NET8_0_OR_GREATER
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
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="obs3Expression">The caller argument expression for <paramref name="obs3"/>. Auto-populated by the compiler.</param>
    /// <param name="obs4Expression">The caller argument expression for <paramref name="obs4"/>. Auto-populated by the compiler.</param>
    /// <param name="obs5Expression">The caller argument expression for <paramref name="obs5"/>. Auto-populated by the compiler.</param>
    /// <param name="obs6Expression">The caller argument expression for <paramref name="obs6"/>. Auto-populated by the compiler.</param>
    /// <param name="obs7Expression">The caller argument expression for <paramref name="obs7"/>. Auto-populated by the compiler.</param>
    /// <param name="obs8Expression">The caller argument expression for <paramref name="obs8"/>. Auto-populated by the compiler.</param>
    /// <param name="obs9Expression">The caller argument expression for <paramref name="obs9"/>. Auto-populated by the compiler.</param>
    /// <param name="obs10Expression">The caller argument expression for <paramref name="obs10"/>. Auto-populated by the compiler.</param>
    /// <param name="obs11Expression">The caller argument expression for <paramref name="obs11"/>. Auto-populated by the compiler.</param>
    /// <param name="obs12Expression">The caller argument expression for <paramref name="obs12"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
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
        Expression<Func<TSender, IObservable<TRet>?>> obs12,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerArgumentExpression("obs3")] string obs3Expression = "",
        [CallerArgumentExpression("obs4")] string obs4Expression = "",
        [CallerArgumentExpression("obs5")] string obs5Expression = "",
        [CallerArgumentExpression("obs6")] string obs6Expression = "",
        [CallerArgumentExpression("obs7")] string obs7Expression = "",
        [CallerArgumentExpression("obs8")] string obs8Expression = "",
        [CallerArgumentExpression("obs9")] string obs9Expression = "",
        [CallerArgumentExpression("obs10")] string obs10Expression = "",
        [CallerArgumentExpression("obs11")] string obs11Expression = "",
        [CallerArgumentExpression("obs12")] string obs12Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#else
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
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "parameter count is inherent to the N-property dispatch stub and its CallerInfo contract")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet>(
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
        Expression<Func<TSender, IObservable<TRet>?>> obs12,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#endif
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

        return RxBindingExtensions.Merge(
            sender.WhenAnyObservable(obs1),
            sender.WhenAnyObservable(obs2),
            sender.WhenAnyObservable(obs3),
            sender.WhenAnyObservable(obs4),
            sender.WhenAnyObservable(obs5),
            sender.WhenAnyObservable(obs6),
            sender.WhenAnyObservable(obs7),
            sender.WhenAnyObservable(obs8),
            sender.WhenAnyObservable(obs9),
            sender.WhenAnyObservable(obs10),
            sender.WhenAnyObservable(obs11),
            sender.WhenAnyObservable(obs12));
    }

#if NET8_0_OR_GREATER
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
    /// <param name="obs1Expression">The caller argument expression for <paramref name="obs1"/>. Auto-populated by the compiler.</param>
    /// <param name="obs2Expression">The caller argument expression for <paramref name="obs2"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet, T1, T2>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Func<T1?, T2?, TRet> selector,
        [CallerArgumentExpression("obs1")] string obs1Expression = "",
        [CallerArgumentExpression("obs2")] string obs2Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#else
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
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservable<TSender, TRet, T1, T2>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Func<T1?, T2?, TRet> selector,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSender : class
#endif
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
