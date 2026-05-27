// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding;

/// <summary>
/// Extension methods for binding an observable stream directly to a target property (<c>BindTo</c>).
/// These generic stubs throw when no concrete generated overload is found; the source generator emits
/// concrete typed overloads that C# overload resolution prefers over these generic stubs.
/// </summary>
public static partial class ReactiveUIBindingExtensions
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// Applies an observable stream to a target property. Conceptually similar to
    /// <c>source.Subscribe(x =&gt; target.property = x)</c>.
    /// </summary>
    /// <typeparam name="TValue">The type of the value produced by the source observable.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetValue">The type of the property on the target object.</typeparam>
    /// <param name="source">The observable stream to bind to a target property.</param>
    /// <param name="target">The target object whose property will be set.</param>
    /// <param name="property">An expression that selects the target property to set.</param>
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TTarget : class
#else
    /// <summary>
    /// Applies an observable stream to a target property. Conceptually similar to
    /// <c>source.Subscribe(x =&gt; target.property = x)</c>.
    /// </summary>
    /// <typeparam name="TValue">The type of the value produced by the source observable.</typeparam>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TTargetValue">The type of the property on the target object.</typeparam>
    /// <param name="source">The observable stream to bind to a target property.</param>
    /// <param name="target">The target object whose property will be set.</param>
    /// <param name="property">An expression that selects the target property to set.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TTarget : class
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }

#if NET8_0_OR_GREATER
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
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        object? conversionHint,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TTarget : class
#else
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
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        object? conversionHint,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TTarget : class
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }

#if NET8_0_OR_GREATER
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
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        IBindingTypeConverter? converterOverride,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TTarget : class
#else
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
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        IBindingTypeConverter? converterOverride,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TTarget : class
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }

#if NET8_0_OR_GREATER
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
    /// <param name="propertyExpression">The caller argument expression for <paramref name="property"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        object? conversionHint,
        IBindingTypeConverter? converterOverride,
        [CallerArgumentExpression("property")] string propertyExpression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TTarget : class
#else
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
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>A disposable that, when disposed, disconnects the binding.</returns>
    public static IDisposable BindTo<TValue, TTarget, TTargetValue>(
        this IObservable<TValue> source,
        TTarget? target,
        Expression<Func<TTarget, TTargetValue?>> property,
        object? conversionHint,
        IBindingTypeConverter? converterOverride,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TTarget : class
#endif
    {
        throw new InvalidOperationException(NoGeneratedBindingMessage);
    }
}
