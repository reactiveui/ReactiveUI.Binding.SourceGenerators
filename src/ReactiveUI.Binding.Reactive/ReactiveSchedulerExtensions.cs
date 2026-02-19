// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding;

/// <summary>
/// Extension methods for property binding with IScheduler support.
/// Requires the ReactiveUI.Binding.Reactive package for System.Reactive interop.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ReactiveSchedulerExtensions
{
    private const string NoGeneratedBindingMessage =
        "No generated binding found. Ensure the expression is an inline lambda for compile-time optimization.";

#if NET8_0_OR_GREATER
    /// <summary>
    /// Creates a one-way binding from a source property to a target property with a specified scheduler.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="sourcePropertyExpression">The caller argument expression for <paramref name="sourceProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="targetPropertyExpression">The caller argument expression for <paramref name="targetProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindOneWay<TSource, TTarget, TProperty>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TProperty>> sourceProperty,
        Expression<Func<TTarget, TProperty>> targetProperty,
        IScheduler? scheduler,
        [CallerArgumentExpression("sourceProperty")] string sourcePropertyExpression = "",
        [CallerArgumentExpression("targetProperty")] string targetPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSource : class
        where TTarget : class
#else
    /// <summary>
    /// Creates a one-way binding from a source property to a target property with a specified scheduler.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindOneWay<TSource, TTarget, TProperty>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TProperty>> sourceProperty,
        Expression<Func<TTarget, TProperty>> targetProperty,
        IScheduler? scheduler,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSource : class
        where TTarget : class
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Creates a one-way binding from a source property to a target property with a conversion function and a specified scheduler.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="conversionFunc">A function that converts the source property value to the target property type.</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="sourcePropertyExpression">The caller argument expression for <paramref name="sourceProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="targetPropertyExpression">The caller argument expression for <paramref name="targetProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindOneWay<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        Func<TSourceProp, TTargetProp> conversionFunc,
        IScheduler? scheduler,
        [CallerArgumentExpression("sourceProperty")] string sourcePropertyExpression = "",
        [CallerArgumentExpression("targetProperty")] string targetPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSource : class
        where TTarget : class
#else
    /// <summary>
    /// Creates a one-way binding from a source property to a target property with a conversion function and a specified scheduler.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="conversionFunc">A function that converts the source property value to the target property type.</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindOneWay<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        Func<TSourceProp, TTargetProp> conversionFunc,
        IScheduler? scheduler,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSource : class
        where TTarget : class
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Creates a two-way binding between a source property and a target property with a specified scheduler.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="sourcePropertyExpression">The caller argument expression for <paramref name="sourceProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="targetPropertyExpression">The caller argument expression for <paramref name="targetProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindTwoWay<TSource, TTarget, TProperty>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TProperty>> sourceProperty,
        Expression<Func<TTarget, TProperty>> targetProperty,
        IScheduler? scheduler,
        [CallerArgumentExpression("sourceProperty")] string sourcePropertyExpression = "",
        [CallerArgumentExpression("targetProperty")] string targetPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSource : class
        where TTarget : class
#else
    /// <summary>
    /// Creates a two-way binding between a source property and a target property with a specified scheduler.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TProperty">The type of the property being bound.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindTwoWay<TSource, TTarget, TProperty>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TProperty>> sourceProperty,
        Expression<Func<TTarget, TProperty>> targetProperty,
        IScheduler? scheduler,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSource : class
        where TTarget : class
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Creates a two-way binding between a source property and a target property with conversion functions and a specified scheduler.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="sourceToTargetConv">A function that converts the source property value to the target property type.</param>
    /// <param name="targetToSourceConv">A function that converts the target property value back to the source property type.</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="sourcePropertyExpression">The caller argument expression for <paramref name="sourceProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="targetPropertyExpression">The caller argument expression for <paramref name="targetProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindTwoWay<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        Func<TSourceProp, TTargetProp> sourceToTargetConv,
        Func<TTargetProp, TSourceProp> targetToSourceConv,
        IScheduler? scheduler,
        [CallerArgumentExpression("sourceProperty")] string sourcePropertyExpression = "",
        [CallerArgumentExpression("targetProperty")] string targetPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSource : class
        where TTarget : class
#else
    /// <summary>
    /// Creates a two-way binding between a source property and a target property with conversion functions and a specified scheduler.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="sourceToTargetConv">A function that converts the source property value to the target property type.</param>
    /// <param name="targetToSourceConv">A function that converts the target property value back to the source property type.</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindTwoWay<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        Func<TSourceProp, TTargetProp> sourceToTargetConv,
        Func<TTargetProp, TSourceProp> targetToSourceConv,
        IScheduler? scheduler,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSource : class
        where TTarget : class
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Creates a one-way binding from a view model property to a view property with a specified selector and scheduler.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TProp">The type of the view model property.</typeparam>
    /// <typeparam name="TOut">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="vmProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="selector">A function that converts the view model property value to the view property type.</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="vmPropertyExpression">The caller argument expression for <paramref name="vmProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="viewPropertyExpression">The caller argument expression for <paramref name="viewProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    public static IReactiveBinding<TView, TOut> OneWayBind<TViewModel, TView, TProp, TOut>(
        this TView view,
        TViewModel viewModel,
        Expression<Func<TViewModel, TProp>> vmProperty,
        Expression<Func<TView, TOut>> viewProperty,
        Func<TProp, TOut> selector,
        IScheduler? scheduler,
        [CallerArgumentExpression("vmProperty")] string vmPropertyExpression = "",
        [CallerArgumentExpression("viewProperty")] string viewPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
#else
    /// <summary>
    /// Creates a one-way binding from a view model property to a view property with a specified selector and scheduler.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TProp">The type of the view model property.</typeparam>
    /// <typeparam name="TOut">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="vmProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="selector">A function that converts the view model property value to the view property type.</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    public static IReactiveBinding<TView, TOut> OneWayBind<TViewModel, TView, TProp, TOut>(
        this TView view,
        TViewModel viewModel,
        Expression<Func<TViewModel, TProp>> vmProperty,
        Expression<Func<TView, TOut>> viewProperty,
        Func<TProp, TOut> selector,
        IScheduler? scheduler,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Creates a two-way binding between a view model property and a view property with conversion functions and a specified scheduler.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
    /// <typeparam name="TVProp">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="vmProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="vmToViewConverter">A function that converts the view model property value to the view property type.</param>
    /// <param name="viewToVmConverter">A function that converts the view property value back to the view model property type.</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="vmPropertyExpression">The caller argument expression for <paramref name="vmProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="viewPropertyExpression">The caller argument expression for <paramref name="viewProperty"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    public static IReactiveBinding<TView, (object? view, bool isViewModel)> Bind<TViewModel, TView, TVMProp, TVProp>(
        this TView view,
        TViewModel viewModel,
        Expression<Func<TViewModel, TVMProp>> vmProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        Func<TVMProp, TVProp> vmToViewConverter,
        Func<TVProp, TVMProp> viewToVmConverter,
        IScheduler? scheduler,
        [CallerArgumentExpression("vmProperty")] string vmPropertyExpression = "",
        [CallerArgumentExpression("viewProperty")] string viewPropertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
#else
    /// <summary>
    /// Creates a two-way binding between a view model property and a view property with conversion functions and a specified scheduler.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
    /// <typeparam name="TVProp">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="vmProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="vmToViewConverter">A function that converts the view model property value to the view property type.</param>
    /// <param name="viewToVmConverter">A function that converts the view property value back to the view model property type.</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    public static IReactiveBinding<TView, (object? view, bool isViewModel)> Bind<TViewModel, TView, TVMProp, TVProp>(
        this TView view,
        TViewModel viewModel,
        Expression<Func<TViewModel, TVMProp>> vmProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        Func<TVMProp, TVProp> vmToViewConverter,
        Func<TVProp, TVMProp> viewToVmConverter,
        IScheduler? scheduler,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }

    /// <summary>
    /// Creates a one-way binding from a source property to a target property using an explicit <see cref="IBindingTypeConverter"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="converter">The binding type converter to use for converting between source and target types.</param>
    /// <param name="conversionHint">An optional hint passed to the converter (e.g., format string).</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindOneWay<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        IBindingTypeConverter converter,
        object? conversionHint = null,
        IScheduler? scheduler = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSource : class
        where TTarget : class
        => throw new InvalidOperationException(NoGeneratedBindingMessage);

    /// <summary>
    /// Creates a two-way binding between a source property and a target property using explicit <see cref="IBindingTypeConverter"/> instances.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TSourceProp">The type of the source property.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetProp">The type of the target property.</typeparam>
    /// <param name="source">The source object to observe for property changes.</param>
    /// <param name="target">The target object whose property will be updated.</param>
    /// <param name="sourceProperty">An expression that selects the source property to observe.</param>
    /// <param name="targetProperty">An expression that selects the target property to update.</param>
    /// <param name="sourceToTargetConverter">The converter for source-to-target conversion.</param>
    /// <param name="targetToSourceConverter">The converter for target-to-source conversion.</param>
    /// <param name="conversionHint">An optional hint passed to the converters (e.g., format string).</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindTwoWay<TSource, TSourceProp, TTarget, TTargetProp>(
        this TSource source,
        TTarget target,
        Expression<Func<TSource, TSourceProp>> sourceProperty,
        Expression<Func<TTarget, TTargetProp>> targetProperty,
        IBindingTypeConverter sourceToTargetConverter,
        IBindingTypeConverter targetToSourceConverter,
        object? conversionHint = null,
        IScheduler? scheduler = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TSource : class
        where TTarget : class
        => throw new InvalidOperationException(NoGeneratedBindingMessage);

    /// <summary>
    /// Creates a one-way binding from a view model property to a view property using an explicit <see cref="IBindingTypeConverter"/>.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
    /// <typeparam name="TVProp">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="vmProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="converter">The binding type converter to use for converting between source and target types.</param>
    /// <param name="conversionHint">An optional hint passed to the converter (e.g., format string).</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    public static IReactiveBinding<TView, TVProp> OneWayBind<TViewModel, TView, TVMProp, TVProp>(
        this TView view,
        TViewModel viewModel,
        Expression<Func<TViewModel, TVMProp>> vmProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        IBindingTypeConverter converter,
        object? conversionHint = null,
        IScheduler? scheduler = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
        => throw new InvalidOperationException(NoGeneratedBindingMessage);

    /// <summary>
    /// Creates a two-way binding between a view model property and a view property using explicit <see cref="IBindingTypeConverter"/> instances.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TVMProp">The type of the view model property.</typeparam>
    /// <typeparam name="TVProp">The type of the view property.</typeparam>
    /// <param name="view">The view to bind to.</param>
    /// <param name="viewModel">The view model to observe.</param>
    /// <param name="vmProperty">An expression that selects the view model property to observe.</param>
    /// <param name="viewProperty">An expression that selects the view property to update.</param>
    /// <param name="vmToViewConverter">The converter for view model-to-view conversion.</param>
    /// <param name="viewToVmConverter">The converter for view-to-view model conversion.</param>
    /// <param name="conversionHint">An optional hint passed to the converters (e.g., format string).</param>
    /// <param name="scheduler">The scheduler to use for the binding.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A reactive binding that can be disposed to disconnect the binding.</returns>
    public static IReactiveBinding<TView, (object? view, bool isViewModel)> Bind<TViewModel, TView, TVMProp, TVProp>(
        this TView view,
        TViewModel viewModel,
        Expression<Func<TViewModel, TVMProp>> vmProperty,
        Expression<Func<TView, TVProp>> viewProperty,
        IBindingTypeConverter vmToViewConverter,
        IBindingTypeConverter viewToVmConverter,
        object? conversionHint = null,
        IScheduler? scheduler = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TViewModel : class
        where TView : class, IViewFor
        => throw new InvalidOperationException(NoGeneratedBindingMessage);
}
