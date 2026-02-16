// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding;

/// <summary>
/// Extension methods for observing property changes with a conversion function (WhenChanged with selector).
/// </summary>
public static partial class ReactiveUIBindingExtensions
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 2 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Func<T1, T2, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 2 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Func<T1, T2, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2));

#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 3 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Func<T1, T2, T3, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerArgumentExpression("property3")] string property3Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 3 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Func<T1, T2, T3, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, property3, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2, t.property3));

#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 4 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Func<T1, T2, T3, T4, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerArgumentExpression("property3")] string property3Expression = "",
        [CallerArgumentExpression("property4")] string property4Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 4 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Func<T1, T2, T3, T4, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, property3, property4, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2, t.property3, t.property4));

#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 5 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Func<T1, T2, T3, T4, T5, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerArgumentExpression("property3")] string property3Expression = "",
        [CallerArgumentExpression("property4")] string property4Expression = "",
        [CallerArgumentExpression("property5")] string property5Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 5 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Func<T1, T2, T3, T4, T5, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, property3, property4, property5, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2, t.property3, t.property4, t.property5));

#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 6 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Func<T1, T2, T3, T4, T5, T6, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerArgumentExpression("property3")] string property3Expression = "",
        [CallerArgumentExpression("property4")] string property4Expression = "",
        [CallerArgumentExpression("property5")] string property5Expression = "",
        [CallerArgumentExpression("property6")] string property6Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 6 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Func<T1, T2, T3, T4, T5, T6, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, property3, property4, property5, property6, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2, t.property3, t.property4, t.property5, t.property6));

#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 7 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Func<T1, T2, T3, T4, T5, T6, T7, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerArgumentExpression("property3")] string property3Expression = "",
        [CallerArgumentExpression("property4")] string property4Expression = "",
        [CallerArgumentExpression("property5")] string property5Expression = "",
        [CallerArgumentExpression("property6")] string property6Expression = "",
        [CallerArgumentExpression("property7")] string property7Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 7 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Func<T1, T2, T3, T4, T5, T6, T7, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, property3, property4, property5, property6, property7, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2, t.property3, t.property4, t.property5, t.property6, t.property7));

#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 8 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerArgumentExpression("property3")] string property3Expression = "",
        [CallerArgumentExpression("property4")] string property4Expression = "",
        [CallerArgumentExpression("property5")] string property5Expression = "",
        [CallerArgumentExpression("property6")] string property6Expression = "",
        [CallerArgumentExpression("property7")] string property7Expression = "",
        [CallerArgumentExpression("property8")] string property8Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 8 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, property3, property4, property5, property6, property7, property8, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2, t.property3, t.property4, t.property5, t.property6, t.property7, t.property8));

#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 9 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerArgumentExpression("property3")] string property3Expression = "",
        [CallerArgumentExpression("property4")] string property4Expression = "",
        [CallerArgumentExpression("property5")] string property5Expression = "",
        [CallerArgumentExpression("property6")] string property6Expression = "",
        [CallerArgumentExpression("property7")] string property7Expression = "",
        [CallerArgumentExpression("property8")] string property8Expression = "",
        [CallerArgumentExpression("property9")] string property9Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 9 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, property3, property4, property5, property6, property7, property8, property9, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2, t.property3, t.property4, t.property5, t.property6, t.property7, t.property8, t.property9));

#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 10 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="property10Expression">The caller argument expression for <paramref name="property10"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerArgumentExpression("property3")] string property3Expression = "",
        [CallerArgumentExpression("property4")] string property4Expression = "",
        [CallerArgumentExpression("property5")] string property5Expression = "",
        [CallerArgumentExpression("property6")] string property6Expression = "",
        [CallerArgumentExpression("property7")] string property7Expression = "",
        [CallerArgumentExpression("property8")] string property8Expression = "",
        [CallerArgumentExpression("property9")] string property9Expression = "",
        [CallerArgumentExpression("property10")] string property10Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 10 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2, t.property3, t.property4, t.property5, t.property6, t.property7, t.property8, t.property9, t.property10));

#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 11 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="property10Expression">The caller argument expression for <paramref name="property10"/>. Auto-populated by the compiler.</param>
    /// <param name="property11Expression">The caller argument expression for <paramref name="property11"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerArgumentExpression("property3")] string property3Expression = "",
        [CallerArgumentExpression("property4")] string property4Expression = "",
        [CallerArgumentExpression("property5")] string property5Expression = "",
        [CallerArgumentExpression("property6")] string property6Expression = "",
        [CallerArgumentExpression("property7")] string property7Expression = "",
        [CallerArgumentExpression("property8")] string property8Expression = "",
        [CallerArgumentExpression("property9")] string property9Expression = "",
        [CallerArgumentExpression("property10")] string property10Expression = "",
        [CallerArgumentExpression("property11")] string property11Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 11 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, property11, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2, t.property3, t.property4, t.property5, t.property6, t.property7, t.property8, t.property9, t.property10, t.property11));

#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 12 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="property10Expression">The caller argument expression for <paramref name="property10"/>. Auto-populated by the compiler.</param>
    /// <param name="property11Expression">The caller argument expression for <paramref name="property11"/>. Auto-populated by the compiler.</param>
    /// <param name="property12Expression">The caller argument expression for <paramref name="property12"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerArgumentExpression("property3")] string property3Expression = "",
        [CallerArgumentExpression("property4")] string property4Expression = "",
        [CallerArgumentExpression("property5")] string property5Expression = "",
        [CallerArgumentExpression("property6")] string property6Expression = "",
        [CallerArgumentExpression("property7")] string property7Expression = "",
        [CallerArgumentExpression("property8")] string property8Expression = "",
        [CallerArgumentExpression("property9")] string property9Expression = "",
        [CallerArgumentExpression("property10")] string property10Expression = "",
        [CallerArgumentExpression("property11")] string property11Expression = "",
        [CallerArgumentExpression("property12")] string property12Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 12 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, property11, property12, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2, t.property3, t.property4, t.property5, t.property6, t.property7, t.property8, t.property9, t.property10, t.property11, t.property12));

#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 13 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="property10Expression">The caller argument expression for <paramref name="property10"/>. Auto-populated by the compiler.</param>
    /// <param name="property11Expression">The caller argument expression for <paramref name="property11"/>. Auto-populated by the compiler.</param>
    /// <param name="property12Expression">The caller argument expression for <paramref name="property12"/>. Auto-populated by the compiler.</param>
    /// <param name="property13Expression">The caller argument expression for <paramref name="property13"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Expression<Func<TObj, T13>> property13,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerArgumentExpression("property3")] string property3Expression = "",
        [CallerArgumentExpression("property4")] string property4Expression = "",
        [CallerArgumentExpression("property5")] string property5Expression = "",
        [CallerArgumentExpression("property6")] string property6Expression = "",
        [CallerArgumentExpression("property7")] string property7Expression = "",
        [CallerArgumentExpression("property8")] string property8Expression = "",
        [CallerArgumentExpression("property9")] string property9Expression = "",
        [CallerArgumentExpression("property10")] string property10Expression = "",
        [CallerArgumentExpression("property11")] string property11Expression = "",
        [CallerArgumentExpression("property12")] string property12Expression = "",
        [CallerArgumentExpression("property13")] string property13Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 13 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Expression<Func<TObj, T13>> property13,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, property11, property12, property13, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2, t.property3, t.property4, t.property5, t.property6, t.property7, t.property8, t.property9, t.property10, t.property11, t.property12, t.property13));

#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 14 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="property14">An expression that selects the fourteenth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="property10Expression">The caller argument expression for <paramref name="property10"/>. Auto-populated by the compiler.</param>
    /// <param name="property11Expression">The caller argument expression for <paramref name="property11"/>. Auto-populated by the compiler.</param>
    /// <param name="property12Expression">The caller argument expression for <paramref name="property12"/>. Auto-populated by the compiler.</param>
    /// <param name="property13Expression">The caller argument expression for <paramref name="property13"/>. Auto-populated by the compiler.</param>
    /// <param name="property14Expression">The caller argument expression for <paramref name="property14"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Expression<Func<TObj, T13>> property13,
        Expression<Func<TObj, T14>> property14,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerArgumentExpression("property3")] string property3Expression = "",
        [CallerArgumentExpression("property4")] string property4Expression = "",
        [CallerArgumentExpression("property5")] string property5Expression = "",
        [CallerArgumentExpression("property6")] string property6Expression = "",
        [CallerArgumentExpression("property7")] string property7Expression = "",
        [CallerArgumentExpression("property8")] string property8Expression = "",
        [CallerArgumentExpression("property9")] string property9Expression = "",
        [CallerArgumentExpression("property10")] string property10Expression = "",
        [CallerArgumentExpression("property11")] string property11Expression = "",
        [CallerArgumentExpression("property12")] string property12Expression = "",
        [CallerArgumentExpression("property13")] string property13Expression = "",
        [CallerArgumentExpression("property14")] string property14Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 14 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="property14">An expression that selects the fourteenth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Expression<Func<TObj, T13>> property13,
        Expression<Func<TObj, T14>> property14,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, property11, property12, property13, property14, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2, t.property3, t.property4, t.property5, t.property6, t.property7, t.property8, t.property9, t.property10, t.property11, t.property12, t.property13, t.property14));

#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 15 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth observed property value.</typeparam>
    /// <typeparam name="T15">The type of the fifteenth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="property14">An expression that selects the fourteenth property to observe.</param>
    /// <param name="property15">An expression that selects the fifteenth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="property10Expression">The caller argument expression for <paramref name="property10"/>. Auto-populated by the compiler.</param>
    /// <param name="property11Expression">The caller argument expression for <paramref name="property11"/>. Auto-populated by the compiler.</param>
    /// <param name="property12Expression">The caller argument expression for <paramref name="property12"/>. Auto-populated by the compiler.</param>
    /// <param name="property13Expression">The caller argument expression for <paramref name="property13"/>. Auto-populated by the compiler.</param>
    /// <param name="property14Expression">The caller argument expression for <paramref name="property14"/>. Auto-populated by the compiler.</param>
    /// <param name="property15Expression">The caller argument expression for <paramref name="property15"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Expression<Func<TObj, T13>> property13,
        Expression<Func<TObj, T14>> property14,
        Expression<Func<TObj, T15>> property15,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerArgumentExpression("property3")] string property3Expression = "",
        [CallerArgumentExpression("property4")] string property4Expression = "",
        [CallerArgumentExpression("property5")] string property5Expression = "",
        [CallerArgumentExpression("property6")] string property6Expression = "",
        [CallerArgumentExpression("property7")] string property7Expression = "",
        [CallerArgumentExpression("property8")] string property8Expression = "",
        [CallerArgumentExpression("property9")] string property9Expression = "",
        [CallerArgumentExpression("property10")] string property10Expression = "",
        [CallerArgumentExpression("property11")] string property11Expression = "",
        [CallerArgumentExpression("property12")] string property12Expression = "",
        [CallerArgumentExpression("property13")] string property13Expression = "",
        [CallerArgumentExpression("property14")] string property14Expression = "",
        [CallerArgumentExpression("property15")] string property15Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 15 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth observed property value.</typeparam>
    /// <typeparam name="T15">The type of the fifteenth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="property14">An expression that selects the fourteenth property to observe.</param>
    /// <param name="property15">An expression that selects the fifteenth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Expression<Func<TObj, T13>> property13,
        Expression<Func<TObj, T14>> property14,
        Expression<Func<TObj, T15>> property15,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, property11, property12, property13, property14, property15, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2, t.property3, t.property4, t.property5, t.property6, t.property7, t.property8, t.property9, t.property10, t.property11, t.property12, t.property13, t.property14, t.property15));

#if NET8_0_OR_GREATER
    /// <summary>
    /// Observes changes on 16 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth observed property value.</typeparam>
    /// <typeparam name="T15">The type of the fifteenth observed property value.</typeparam>
    /// <typeparam name="T16">The type of the sixteenth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="property14">An expression that selects the fourteenth property to observe.</param>
    /// <param name="property15">An expression that selects the fifteenth property to observe.</param>
    /// <param name="property16">An expression that selects the sixteenth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="property1Expression">The caller argument expression for <paramref name="property1"/>. Auto-populated by the compiler.</param>
    /// <param name="property2Expression">The caller argument expression for <paramref name="property2"/>. Auto-populated by the compiler.</param>
    /// <param name="property3Expression">The caller argument expression for <paramref name="property3"/>. Auto-populated by the compiler.</param>
    /// <param name="property4Expression">The caller argument expression for <paramref name="property4"/>. Auto-populated by the compiler.</param>
    /// <param name="property5Expression">The caller argument expression for <paramref name="property5"/>. Auto-populated by the compiler.</param>
    /// <param name="property6Expression">The caller argument expression for <paramref name="property6"/>. Auto-populated by the compiler.</param>
    /// <param name="property7Expression">The caller argument expression for <paramref name="property7"/>. Auto-populated by the compiler.</param>
    /// <param name="property8Expression">The caller argument expression for <paramref name="property8"/>. Auto-populated by the compiler.</param>
    /// <param name="property9Expression">The caller argument expression for <paramref name="property9"/>. Auto-populated by the compiler.</param>
    /// <param name="property10Expression">The caller argument expression for <paramref name="property10"/>. Auto-populated by the compiler.</param>
    /// <param name="property11Expression">The caller argument expression for <paramref name="property11"/>. Auto-populated by the compiler.</param>
    /// <param name="property12Expression">The caller argument expression for <paramref name="property12"/>. Auto-populated by the compiler.</param>
    /// <param name="property13Expression">The caller argument expression for <paramref name="property13"/>. Auto-populated by the compiler.</param>
    /// <param name="property14Expression">The caller argument expression for <paramref name="property14"/>. Auto-populated by the compiler.</param>
    /// <param name="property15Expression">The caller argument expression for <paramref name="property15"/>. Auto-populated by the compiler.</param>
    /// <param name="property16Expression">The caller argument expression for <paramref name="property16"/>. Auto-populated by the compiler.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Expression<Func<TObj, T13>> property13,
        Expression<Func<TObj, T14>> property14,
        Expression<Func<TObj, T15>> property15,
        Expression<Func<TObj, T16>> property16,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn> conversionFunc,
        [CallerArgumentExpression("property1")] string property1Expression = "",
        [CallerArgumentExpression("property2")] string property2Expression = "",
        [CallerArgumentExpression("property3")] string property3Expression = "",
        [CallerArgumentExpression("property4")] string property4Expression = "",
        [CallerArgumentExpression("property5")] string property5Expression = "",
        [CallerArgumentExpression("property6")] string property6Expression = "",
        [CallerArgumentExpression("property7")] string property7Expression = "",
        [CallerArgumentExpression("property8")] string property8Expression = "",
        [CallerArgumentExpression("property9")] string property9Expression = "",
        [CallerArgumentExpression("property10")] string property10Expression = "",
        [CallerArgumentExpression("property11")] string property11Expression = "",
        [CallerArgumentExpression("property12")] string property12Expression = "",
        [CallerArgumentExpression("property13")] string property13Expression = "",
        [CallerArgumentExpression("property14")] string property14Expression = "",
        [CallerArgumentExpression("property15")] string property15Expression = "",
        [CallerArgumentExpression("property16")] string property16Expression = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#else
    /// <summary>
    /// Observes changes on 16 properties on the specified object and applies a conversion function to produce a result after any property changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth observed property value.</typeparam>
    /// <typeparam name="T15">The type of the fifteenth observed property value.</typeparam>
    /// <typeparam name="T16">The type of the sixteenth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="property14">An expression that selects the fourteenth property to observe.</param>
    /// <param name="property15">An expression that selects the fifteenth property to observe.</param>
    /// <param name="property16">An expression that selects the sixteenth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <param name="callerFilePath">The source file path of the caller. Auto-populated by the compiler.</param>
    /// <param name="callerLineNumber">The source line number of the caller. Auto-populated by the compiler.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    public static IObservable<TReturn> WhenChanged<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Expression<Func<TObj, T13>> property13,
        Expression<Func<TObj, T14>> property14,
        Expression<Func<TObj, T15>> property15,
        Expression<Func<TObj, T16>> property16,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn> conversionFunc,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
        where TObj : class
#endif
        => objectToMonitor.WhenChanged(property1, property2, property3, property4, property5, property6, property7, property8, property9, property10, property11, property12, property13, property14, property15, property16, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber)
            .Select(t => conversionFunc(t.property1, t.property2, t.property3, t.property4, t.property5, t.property6, t.property7, t.property8, t.property9, t.property10, t.property11, t.property12, t.property13, t.property14, t.property15, t.property16));
}
