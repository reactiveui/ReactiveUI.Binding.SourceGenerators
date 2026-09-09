// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Observes property chains that are only known as expressions at run time.</summary>
/// <remarks>
/// Every other observation in this library is resolved at compile time, which is what keeps it free of
/// reflection. These overloads take an <see cref="Expression"/> the caller built rather than a lambda the
/// compiler could read, so there is nothing for a generator to resolve and the chain has to be walked by
/// reflection. They are the one part of the observation surface that is not trim- or AOT-safe, and they
/// say so.
/// <para>
/// The walking itself is the engine the runtime observation fallback already uses, so resolving a link,
/// re-subscribing a deeper one when an intermediate moves, and filtering duplicates are not written twice.
/// Each arity differs only in how many chains it combines.
/// </para>
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Why every overload here is unsafe to trim.</summary>
    private const string DynamicChainRequiresUnreferencedCode =
        "Evaluates expression-based member chains via reflection; members may be trimmed.";

    /// <summary>Observes 1 dynamically-typed property chain and projects it with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Func<IObservedChange<TSender, object?>, TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, selector, true);

    /// <summary>Observes 1 dynamically-typed property chain and projects it with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Func<IObservedChange<TSender, object?>, TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        return ObserveDynamicChain(sender, property1, isDistinct).Select(selector);
    }

    /// <summary>Observes 2 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Func<IObservedChange<TSender, object?>, IObservedChange<TSender, object?>, TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, selector, true);

    /// <summary>Observes 2 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Func<IObservedChange<TSender, object?>, IObservedChange<TSender, object?>, TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        return CombineLatestObservable.Create(
            ObserveDynamicChain(sender, property1, isDistinct),
            ObserveDynamicChain(sender, property2, isDistinct),
            selector);
    }

    /// <summary>Observes 3 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Func<IObservedChange<TSender, object?>, IObservedChange<TSender, object?>, IObservedChange<TSender, object?>, TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, selector, true);

    /// <summary>Observes 3 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Func<IObservedChange<TSender, object?>, IObservedChange<TSender, object?>, IObservedChange<TSender, object?>, TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        return CombineLatestObservable.Create(
            ObserveDynamicChain(sender, property1, isDistinct),
            ObserveDynamicChain(sender, property2, isDistinct),
            ObserveDynamicChain(sender, property3, isDistinct),
            selector);
    }

    /// <summary>Observes 4 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, selector, true);

    /// <summary>Observes 4 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        return CombineLatestObservable.Create(
            ObserveDynamicChain(sender, property1, isDistinct),
            ObserveDynamicChain(sender, property2, isDistinct),
            ObserveDynamicChain(sender, property3, isDistinct),
            ObserveDynamicChain(sender, property4, isDistinct),
            selector);
    }

    /// <summary>Observes 5 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, selector, true);

    /// <summary>Observes 5 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        return CombineLatestObservable.Create(
            ObserveDynamicChain(sender, property1, isDistinct),
            ObserveDynamicChain(sender, property2, isDistinct),
            ObserveDynamicChain(sender, property3, isDistinct),
            ObserveDynamicChain(sender, property4, isDistinct),
            ObserveDynamicChain(sender, property5, isDistinct),
            selector);
    }

    /// <summary>Observes 6 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, property6, selector, true);

    /// <summary>Observes 6 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        return CombineLatestObservable.Create(
            ObserveDynamicChain(sender, property1, isDistinct),
            ObserveDynamicChain(sender, property2, isDistinct),
            ObserveDynamicChain(sender, property3, isDistinct),
            ObserveDynamicChain(sender, property4, isDistinct),
            ObserveDynamicChain(sender, property5, isDistinct),
            ObserveDynamicChain(sender, property6, isDistinct),
            selector);
    }

    /// <summary>Observes 7 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, property6, property7, selector, true);

    /// <summary>Observes 7 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        return CombineLatestObservable.Create(
            ObserveDynamicChain(sender, property1, isDistinct),
            ObserveDynamicChain(sender, property2, isDistinct),
            ObserveDynamicChain(sender, property3, isDistinct),
            ObserveDynamicChain(sender, property4, isDistinct),
            ObserveDynamicChain(sender, property5, isDistinct),
            ObserveDynamicChain(sender, property6, isDistinct),
            ObserveDynamicChain(sender, property7, isDistinct),
            selector);
    }

    /// <summary>Observes 8 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, property6, property7, property8, selector, true);

    /// <summary>Observes 8 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        return CombineLatestObservable.Create(
            ObserveDynamicChain(sender, property1, isDistinct),
            ObserveDynamicChain(sender, property2, isDistinct),
            ObserveDynamicChain(sender, property3, isDistinct),
            ObserveDynamicChain(sender, property4, isDistinct),
            ObserveDynamicChain(sender, property5, isDistinct),
            ObserveDynamicChain(sender, property6, isDistinct),
            ObserveDynamicChain(sender, property7, isDistinct),
            ObserveDynamicChain(sender, property8, isDistinct),
            selector);
    }

    /// <summary>Observes 9 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, property6, property7, property8, property9, selector, true);

    /// <summary>Observes 9 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        return CombineLatestObservable.Create(
            ObserveDynamicChain(sender, property1, isDistinct),
            ObserveDynamicChain(sender, property2, isDistinct),
            ObserveDynamicChain(sender, property3, isDistinct),
            ObserveDynamicChain(sender, property4, isDistinct),
            ObserveDynamicChain(sender, property5, isDistinct),
            ObserveDynamicChain(sender, property6, isDistinct),
            ObserveDynamicChain(sender, property7, isDistinct),
            ObserveDynamicChain(sender, property8, isDistinct),
            ObserveDynamicChain(sender, property9, isDistinct),
            selector);
    }

    /// <summary>Observes 10 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="property10">An expression naming property 10.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Expression? property10,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, selector, true);

    /// <summary>Observes 10 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="property10">An expression naming property 10.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Expression? property10,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        return CombineLatestObservable.Create(
            ObserveDynamicChain(sender, property1, isDistinct),
            ObserveDynamicChain(sender, property2, isDistinct),
            ObserveDynamicChain(sender, property3, isDistinct),
            ObserveDynamicChain(sender, property4, isDistinct),
            ObserveDynamicChain(sender, property5, isDistinct),
            ObserveDynamicChain(sender, property6, isDistinct),
            ObserveDynamicChain(sender, property7, isDistinct),
            ObserveDynamicChain(sender, property8, isDistinct),
            ObserveDynamicChain(sender, property9, isDistinct),
            ObserveDynamicChain(sender, property10, isDistinct),
            selector);
    }

    /// <summary>Observes 11 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="property10">An expression naming property 10.</param>
    /// <param name="property11">An expression naming property 11.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Expression? property10,
        Expression? property11,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, property11, selector, true);

    /// <summary>Observes 11 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="property10">An expression naming property 10.</param>
    /// <param name="property11">An expression naming property 11.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Expression? property10,
        Expression? property11,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        return CombineLatestObservable.Create(
            ObserveDynamicChain(sender, property1, isDistinct),
            ObserveDynamicChain(sender, property2, isDistinct),
            ObserveDynamicChain(sender, property3, isDistinct),
            ObserveDynamicChain(sender, property4, isDistinct),
            ObserveDynamicChain(sender, property5, isDistinct),
            ObserveDynamicChain(sender, property6, isDistinct),
            ObserveDynamicChain(sender, property7, isDistinct),
            ObserveDynamicChain(sender, property8, isDistinct),
            ObserveDynamicChain(sender, property9, isDistinct),
            ObserveDynamicChain(sender, property10, isDistinct),
            ObserveDynamicChain(sender, property11, isDistinct),
            selector);
    }

    /// <summary>Observes 12 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="property10">An expression naming property 10.</param>
    /// <param name="property11">An expression naming property 11.</param>
    /// <param name="property12">An expression naming property 12.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Expression? property10,
        Expression? property11,
        Expression? property12,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector)
        where TSender : class
        => sender.WhenAnyDynamic(property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, property11, property12, selector, true);

    /// <summary>Observes 12 dynamically-typed property chains and combines them with a selector.</summary>
    /// <typeparam name="TSender">The type of the object the chains are rooted on.</typeparam>
    /// <typeparam name="TRet">The type of the projected result.</typeparam>
    /// <param name="sender">The object the chains are rooted on.</param>
    /// <param name="property1">An expression naming property 1.</param>
    /// <param name="property2">An expression naming property 2.</param>
    /// <param name="property3">An expression naming property 3.</param>
    /// <param name="property4">An expression naming property 4.</param>
    /// <param name="property5">An expression naming property 5.</param>
    /// <param name="property6">An expression naming property 6.</param>
    /// <param name="property7">An expression naming property 7.</param>
    /// <param name="property8">An expression naming property 8.</param>
    /// <param name="property9">An expression naming property 9.</param>
    /// <param name="property10">An expression naming property 10.</param>
    /// <param name="property11">An expression naming property 11.</param>
    /// <param name="property12">An expression naming property 12.</param>
    /// <param name="selector">Projects the observed changes into a result.</param>
    /// <param name="isDistinct">Whether a chain reports only when its value changes.</param>
    /// <returns>An observable of the projected result.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [SuppressMessage("Design", "SST1472", Justification = "one expression per observed chain; the parameter count is the shape of this overload")]
    public static IObservable<TRet> WhenAnyDynamic<TSender, TRet>(
        this TSender sender,
        Expression? property1,
        Expression? property2,
        Expression? property3,
        Expression? property4,
        Expression? property5,
        Expression? property6,
        Expression? property7,
        Expression? property8,
        Expression? property9,
        Expression? property10,
        Expression? property11,
        Expression? property12,
        Func<
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            IObservedChange<TSender, object?>,
            TRet> selector,
        bool isDistinct)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        return CombineLatestObservable.Create(
            ObserveDynamicChain(sender, property1, isDistinct),
            ObserveDynamicChain(sender, property2, isDistinct),
            ObserveDynamicChain(sender, property3, isDistinct),
            ObserveDynamicChain(sender, property4, isDistinct),
            ObserveDynamicChain(sender, property5, isDistinct),
            ObserveDynamicChain(sender, property6, isDistinct),
            ObserveDynamicChain(sender, property7, isDistinct),
            ObserveDynamicChain(sender, property8, isDistinct),
            ObserveDynamicChain(sender, property9, isDistinct),
            ObserveDynamicChain(sender, property10, isDistinct),
            ObserveDynamicChain(sender, property11, isDistinct),
            ObserveDynamicChain(sender, property12, isDistinct),
            selector);
    }

    /// <summary>Subscribes to one property chain named by a run-time expression.</summary>
    /// <typeparam name="TSender">The type of the object the chain is rooted on.</typeparam>
    /// <param name="sender">The object the chain is rooted on.</param>
    /// <param name="property">An expression naming the chain.</param>
    /// <param name="isDistinct">Whether the chain reports only when its value changes.</param>
    /// <returns>An observable of the chain's observed changes.</returns>
    /// <remarks>Every arity funnels through here, so the reflection is written once.</remarks>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IObservable<IObservedChange<TSender, object?>> ObserveDynamicChain<TSender>(
        TSender sender,
        Expression? property,
        bool isDistinct)
        where TSender : class =>
        sender.SubscribeToExpressionChain<TSender, object?>(property, false, false, isDistinct);
}
