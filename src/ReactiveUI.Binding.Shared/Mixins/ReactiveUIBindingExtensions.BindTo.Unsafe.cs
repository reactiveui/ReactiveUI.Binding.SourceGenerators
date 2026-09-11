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

/// <summary>Resolves a BindTo expression by reflection, for a call site the generator could not read.</summary>
/// <remarks>
/// The opt-in half of <c>BindTo</c>. The unsuffixed overloads resolve at compile time and throw when no generated
/// dispatch claimed the call site; these walk the chain by reflection instead, and are annotated so a consumer
/// publishing trimmed or ahead-of-time sees the requirement at their own call site.
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Applies an observable stream to a target property. Conceptually similar to <c>source.Subscribe(x =&gt; target.property = x)</c>.</summary>
    /// <typeparam name="TValue">The type of the value produced by the source observable.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetValue">The type of the property on the target object.</typeparam>
    /// <param name="source">The observable stream to bind to a target property.</param>
    /// <param name="target">The target object whose property will be set.</param>
    /// <param name="property">An expression that selects the target property to set.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindToUnsafe<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property)
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(property.Body);

        return RuntimeBindingFallback.BindTo(
            source,
            target,
            property,
            null,
            null,
            null,
            bindingExpression);
    }

    /// <summary>
    /// Applies an observable stream to a target property, using the supplied conversion hint when a
    /// converter is required to coerce the source value to the target property type.
    /// </summary>
    /// <typeparam name="TValue">The type of the value produced by the source observable.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetValue">The type of the property on the target object.</typeparam>
    /// <param name="source">The observable stream to bind to a target property.</param>
    /// <param name="target">The target object whose property will be set.</param>
    /// <param name="property">An expression that selects the target property to set.</param>
    /// <param name="conversionHint">An object that provides a hint to the converter. The semantics are defined by the converter.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindToUnsafe<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        object? conversionHint)
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(property.Body);

        return RuntimeBindingFallback.BindTo(
            source,
            target,
            property,
            conversionHint,
            null,
            null,
            bindingExpression);
    }

    /// <summary>
    /// Applies an observable stream to a target property, using the supplied converter to coerce the
    /// source value to the target property type.
    /// </summary>
    /// <typeparam name="TValue">The type of the value produced by the source observable.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetValue">The type of the property on the target object.</typeparam>
    /// <param name="source">The observable stream to bind to a target property.</param>
    /// <param name="target">The target object whose property will be set.</param>
    /// <param name="property">An expression that selects the target property to set.</param>
    /// <param name="converterOverride">An explicit converter to use when converting the source value to the target property type.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindToUnsafe<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        IBindingTypeConverter? converterOverride)
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(property.Body);

        return RuntimeBindingFallback.BindTo(
            source,
            target,
            property,
            null,
            converterOverride,
            null,
            bindingExpression);
    }

    /// <summary>
    /// Applies an observable stream to a target property, using the supplied converter and conversion
    /// hint to coerce the source value to the target property type.
    /// </summary>
    /// <typeparam name="TValue">The type of the value produced by the source observable.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetValue">The type of the property on the target object.</typeparam>
    /// <param name="source">The observable stream to bind to a target property.</param>
    /// <param name="target">The target object whose property will be set.</param>
    /// <param name="property">An expression that selects the target property to set.</param>
    /// <param name="conversionHint">An object that provides a hint to the converter. The semantics are defined by the converter.</param>
    /// <param name="converterOverride">An explicit converter to use when converting the source value to the target property type.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    /// <remarks>Resolves the property chain by reflection, for an expression the generator could not read.</remarks>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IDisposable BindToUnsafe<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        object? conversionHint,
        IBindingTypeConverter? converterOverride)
        where TTarget : class
    {
        var bindingExpression = Reflection.ExpressionToPropertyNames(property.Body);

        return RuntimeBindingFallback.BindTo(
            source,
            target,
            property,
            conversionHint,
            converterOverride,
            null,
            bindingExpression);
    }
}
