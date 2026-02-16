// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq.Expressions;

namespace ReactiveUI.Binding;

/// <summary>
/// Plugin interface for types that can provide property change observation.
/// Implementations register with Splat and are resolved by affinity scoring.
/// </summary>
/// <remarks>
/// The interfaces are marked safe (no [RequiresUnreferencedCode]). Runtime fallback
/// implementations that use reflection keep [RequiresUnreferencedCode].
/// </remarks>
public interface ICreatesObservableForProperty
{
    /// <summary>
    /// Returns a positive integer for types this implementation can observe,
    /// or zero/negative for unsupported types. Higher values win.
    /// </summary>
    /// <param name="type">The type being observed.</param>
    /// <param name="propertyName">The property name being observed.</param>
    /// <param name="beforeChanged">Whether before-change (PropertyChanging) is requested.</param>
    /// <returns>The affinity score. Positive means supported.</returns>
    int GetAffinityForObject(Type type, string propertyName, bool beforeChanged = false);

    /// <summary>
    /// Creates an observable that fires when the specified property changes.
    /// </summary>
    /// <param name="sender">The object to observe.</param>
    /// <param name="expression">The expression identifying the property.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="beforeChanged">Whether to observe before-change events.</param>
    /// <param name="suppressWarnings">Whether to suppress diagnostic warnings.</param>
    /// <returns>An observable of observed changes.</returns>
    IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName,
        bool beforeChanged = false,
        bool suppressWarnings = false);
}
