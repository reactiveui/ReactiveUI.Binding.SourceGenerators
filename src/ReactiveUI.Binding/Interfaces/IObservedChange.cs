// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;

namespace ReactiveUI.Binding;

/// <summary>
/// Represents the result of a property change observation.
/// </summary>
/// <typeparam name="TSender">The type of the object that raised the change.</typeparam>
/// <typeparam name="TValue">The type of the property value.</typeparam>
public interface IObservedChange<out TSender, out TValue>
{
    /// <summary>
    /// Gets the object that raised the change.
    /// </summary>
    TSender Sender { get; }

    /// <summary>
    /// Gets the expression of the member that changed.
    /// </summary>
    Expression? Expression { get; }

    /// <summary>
    /// Gets the current value of the property.
    /// </summary>
    TValue Value { get; }
}
