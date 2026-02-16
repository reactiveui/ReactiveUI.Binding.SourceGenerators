// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;

using ReactiveUI.Binding.Expressions;

namespace ReactiveUI.Binding.ObservableForProperty;

/// <summary>
/// A collection of helpers for <see cref="IObservedChange{TSender, TValue}"/>.
/// </summary>
public static class ObservedChangedMixin
{
    /// <summary>
    /// Returns the name of a property which has been changed.
    /// </summary>
    /// <typeparam name="TSender">The sender type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="item">The observed change.</param>
    /// <returns>The name of the property which has changed.</returns>
    public static string GetPropertyName<TSender, TValue>(this IObservedChange<TSender, TValue> item) =>
        item is null
            ? throw new ArgumentNullException(nameof(item))
            : Reflection.ExpressionToPropertyNames(item.Expression);

    /// <summary>
    /// Returns the current value of a property given a notification that it has changed.
    /// </summary>
    /// <typeparam name="TSender">The sender.</typeparam>
    /// <typeparam name="TValue">The changed value.</typeparam>
    /// <param name="item">The observed change instance to get the value of.</param>
    /// <returns>The current value of the property.</returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    public static TValue GetValue<TSender, TValue>(this IObservedChange<TSender, TValue> item) =>
        item is null
            ? throw new ArgumentNullException(nameof(item))
            : !item.TryGetValue(out var returnValue)
                ? throw new Exception($"One of the properties in the expression '{item.GetPropertyName()}' was null")
                : returnValue;

    /// <summary>
    /// Returns the current value of a property given a notification that it has changed,
    /// or the default value if the chain cannot be resolved.
    /// </summary>
    /// <typeparam name="TSender">The sender.</typeparam>
    /// <typeparam name="TValue">The changed value.</typeparam>
    /// <param name="item">The observed change instance to get the value of.</param>
    /// <returns>The current value of the property, or default.</returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    public static TValue? GetValueOrDefault<TSender, TValue>(this IObservedChange<TSender, TValue> item) =>
        item is null ? throw new ArgumentNullException(nameof(item)) : !item.TryGetValue(out var returnValue) ? default : returnValue;

    /// <summary>
    /// Given a stream of notification changes, this method will convert
    /// the property changes to the current value of the property.
    /// </summary>
    /// <typeparam name="TSender">The sender type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="item">The change notification stream to get the values of.</param>
    /// <returns>An Observable representing the stream of current values.</returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    public static IObservable<TValue> Value<TSender, TValue>(this IObservable<IObservedChange<TSender, TValue>> item) =>
        item.Select(GetValue);

    /// <summary>
    /// Attempts to return the current value of a property given a notification that it has changed.
    /// </summary>
    /// <typeparam name="TSender">The sender type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="item">The observed change instance to get the value of.</param>
    /// <param name="changeValue">The value of the property expression.</param>
    /// <returns>True if the entire expression was able to be followed, false otherwise.</returns>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    internal static bool TryGetValue<TSender, TValue>(this IObservedChange<TSender, TValue> item, out TValue changeValue)
    {
        if (!Equals(item.Value, default(TValue)))
        {
            changeValue = item.Value;
            return true;
        }

        return Reflection.TryGetValueForPropertyChain(out changeValue, item.Sender, item.Expression!.GetExpressionChain());
    }

    /// <summary>
    /// Given a fully filled-out IObservedChange object, SetValueToProperty
    /// will apply it to the specified object.
    /// </summary>
    /// <typeparam name="TSender">The sender type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="item">The observed change instance to use as a value to apply.</param>
    /// <param name="target">The target object to apply the change to.</param>
    /// <param name="property">The target property to apply the change to.</param>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    internal static void SetValueToProperty<TSender, TValue, TTarget>(
        this IObservedChange<TSender, TValue> item,
        TTarget target,
        System.Linq.Expressions.Expression<Func<TTarget, TValue>> property)
    {
        if (target is not null)
        {
            Reflection.TrySetValueToPropertyChain(target, Reflection.Rewrite(property.Body).GetExpressionChain(), item.GetValue());
        }
    }
}
