// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reactive.Linq;
using ReactiveUI.Binding.ObservableForProperty;

namespace ReactiveUI.Binding.Fallback;

/// <summary>
/// Provides runtime fallback implementations for WhenChanged, WhenChanging, and WhenAnyValue
/// when the source generator cannot handle an invocation at compile time.
/// Uses the ported expression chain analysis via <see cref="ICreatesObservableForProperty"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
public static class RuntimeObservationFallback
{
    /// <summary>
    /// Runtime fallback for WhenChanged with a single property.
    /// </summary>
    /// <typeparam name="TObj">The type of object being observed.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="obj">The object to observe.</param>
    /// <param name="property">The property expression.</param>
    /// <returns>An observable that emits property values after they change.</returns>
    public static IObservable<TValue> WhenChanged<TObj, TValue>(
        TObj obj,
        Expression<Func<TObj, TValue>> property)
        where TObj : class
    {
        ArgumentExceptionHelper.ThrowIfNull(property);

        return obj.SubscribeToExpressionChain<TObj, TValue>(
                property.Body,
                beforeChange: false,
                skipInitial: false,
                isDistinct: true)
            .Select(x => x.Value);
    }

    /// <summary>
    /// Runtime fallback for WhenChanging with a single property.
    /// </summary>
    /// <typeparam name="TObj">The type of object being observed.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="obj">The object to observe.</param>
    /// <param name="property">The property expression.</param>
    /// <returns>An observable that emits property values before they change.</returns>
    public static IObservable<TValue> WhenChanging<TObj, TValue>(
        TObj obj,
        Expression<Func<TObj, TValue>> property)
        where TObj : class
    {
        ArgumentExceptionHelper.ThrowIfNull(property);

        return obj.SubscribeToExpressionChain<TObj, TValue>(
                property.Body,
                beforeChange: true,
                skipInitial: false,
                isDistinct: true)
            .Select(x => x.Value);
    }

    /// <summary>
    /// Runtime fallback for WhenAnyValue with a single property.
    /// </summary>
    /// <typeparam name="TSender">The type of object being observed.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="sender">The object to observe.</param>
    /// <param name="property">The property expression.</param>
    /// <returns>An observable that emits property values after they change.</returns>
    public static IObservable<TValue> WhenAnyValue<TSender, TValue>(
        TSender sender,
        Expression<Func<TSender, TValue>> property)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(property);

        return sender.SubscribeToExpressionChain<TSender, TValue>(
                property.Body,
                beforeChange: false,
                skipInitial: false,
                isDistinct: true)
            .Select(x => x.Value);
    }

    /// <summary>
    /// Runtime fallback for multi-property WhenChanged, returning a tuple of values.
    /// </summary>
    /// <typeparam name="TObj">The type of object being observed.</typeparam>
    /// <typeparam name="T1">The type of the first property value.</typeparam>
    /// <typeparam name="T2">The type of the second property value.</typeparam>
    /// <param name="obj">The object to observe.</param>
    /// <param name="property1">The first property expression.</param>
    /// <param name="property2">The second property expression.</param>
    /// <returns>An observable that emits a tuple of property values when any changes.</returns>
    public static IObservable<(T1 Value1, T2 Value2)> WhenChanged<TObj, T1, T2>(
        TObj obj,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2)
        where TObj : class
    {
        var o1 = WhenChanged(obj, property1);
        var o2 = WhenChanged(obj, property2);
        return o1.CombineLatest(o2, (v1, v2) => (v1, v2));
    }

    /// <summary>
    /// Runtime fallback for multi-property WhenChanged with 3 properties.
    /// </summary>
    /// <typeparam name="TObj">The type of object being observed.</typeparam>
    /// <typeparam name="T1">The type of the first property value.</typeparam>
    /// <typeparam name="T2">The type of the second property value.</typeparam>
    /// <typeparam name="T3">The type of the third property value.</typeparam>
    /// <param name="obj">The object to observe.</param>
    /// <param name="property1">The first property expression.</param>
    /// <param name="property2">The second property expression.</param>
    /// <param name="property3">The third property expression.</param>
    /// <returns>An observable that emits a tuple of property values when any changes.</returns>
    public static IObservable<(T1 Value1, T2 Value2, T3 Value3)> WhenChanged<TObj, T1, T2, T3>(
        TObj obj,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3)
        where TObj : class
    {
        var o1 = WhenChanged(obj, property1);
        var o2 = WhenChanged(obj, property2);
        var o3 = WhenChanged(obj, property3);
        return o1.CombineLatest(o2, o3, (v1, v2, v3) => (v1, v2, v3));
    }

    /// <summary>
    /// Runtime fallback for multi-property WhenChanging with 2 properties.
    /// </summary>
    /// <typeparam name="TObj">The type of object being observed.</typeparam>
    /// <typeparam name="T1">The type of the first property value.</typeparam>
    /// <typeparam name="T2">The type of the second property value.</typeparam>
    /// <param name="obj">The object to observe.</param>
    /// <param name="property1">The first property expression.</param>
    /// <param name="property2">The second property expression.</param>
    /// <returns>An observable that emits a tuple of property values before any changes.</returns>
    public static IObservable<(T1 Value1, T2 Value2)> WhenChanging<TObj, T1, T2>(
        TObj obj,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2)
        where TObj : class
    {
        var o1 = WhenChanging(obj, property1);
        var o2 = WhenChanging(obj, property2);
        return o1.CombineLatest(o2, (v1, v2) => (v1, v2));
    }

    /// <summary>
    /// Runtime fallback for multi-property WhenChanging with 3 properties.
    /// </summary>
    /// <typeparam name="TObj">The type of object being observed.</typeparam>
    /// <typeparam name="T1">The type of the first property value.</typeparam>
    /// <typeparam name="T2">The type of the second property value.</typeparam>
    /// <typeparam name="T3">The type of the third property value.</typeparam>
    /// <param name="obj">The object to observe.</param>
    /// <param name="property1">The first property expression.</param>
    /// <param name="property2">The second property expression.</param>
    /// <param name="property3">The third property expression.</param>
    /// <returns>An observable that emits a tuple of property values before any changes.</returns>
    public static IObservable<(T1 Value1, T2 Value2, T3 Value3)> WhenChanging<TObj, T1, T2, T3>(
        TObj obj,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3)
        where TObj : class
    {
        var o1 = WhenChanging(obj, property1);
        var o2 = WhenChanging(obj, property2);
        var o3 = WhenChanging(obj, property3);
        return o1.CombineLatest(o2, o3, (v1, v2, v3) => (v1, v2, v3));
    }

    /// <summary>
    /// Runtime fallback for multi-property WhenAnyValue with 2 properties.
    /// </summary>
    /// <typeparam name="TSender">The type of object being observed.</typeparam>
    /// <typeparam name="T1">The type of the first property value.</typeparam>
    /// <typeparam name="T2">The type of the second property value.</typeparam>
    /// <param name="sender">The object to observe.</param>
    /// <param name="property1">The first property expression.</param>
    /// <param name="property2">The second property expression.</param>
    /// <returns>An observable that emits a tuple of property values when any changes.</returns>
    public static IObservable<(T1 Value1, T2 Value2)> WhenAnyValue<TSender, T1, T2>(
        TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2)
        where TSender : class
    {
        var o1 = WhenAnyValue(sender, property1);
        var o2 = WhenAnyValue(sender, property2);
        return o1.CombineLatest(o2, (v1, v2) => (v1, v2));
    }

    /// <summary>
    /// Runtime fallback for multi-property WhenAnyValue with 3 properties.
    /// </summary>
    /// <typeparam name="TSender">The type of object being observed.</typeparam>
    /// <typeparam name="T1">The type of the first property value.</typeparam>
    /// <typeparam name="T2">The type of the second property value.</typeparam>
    /// <typeparam name="T3">The type of the third property value.</typeparam>
    /// <param name="sender">The object to observe.</param>
    /// <param name="property1">The first property expression.</param>
    /// <param name="property2">The second property expression.</param>
    /// <param name="property3">The third property expression.</param>
    /// <returns>An observable that emits a tuple of property values when any changes.</returns>
    public static IObservable<(T1 Value1, T2 Value2, T3 Value3)> WhenAnyValue<TSender, T1, T2, T3>(
        TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3)
        where TSender : class
    {
        var o1 = WhenAnyValue(sender, property1);
        var o2 = WhenAnyValue(sender, property2);
        var o3 = WhenAnyValue(sender, property3);
        return o1.CombineLatest(o2, o3, (v1, v2, v3) => (v1, v2, v3));
    }
}
