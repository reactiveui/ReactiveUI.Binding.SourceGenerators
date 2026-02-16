// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq.Expressions;

namespace ReactiveUI.Binding;

/// <summary>
/// Represents a binding between a view and a view model property.
/// </summary>
/// <typeparam name="TView">The type of the view.</typeparam>
/// <typeparam name="TValue">The type of the bound value.</typeparam>
public interface IReactiveBinding<out TView, out TValue> : IDisposable
    where TView : IViewFor
{
    /// <summary>
    /// Gets the expression representing the view model property that is bound.
    /// </summary>
    Expression? ViewModelExpression { get; }

    /// <summary>
    /// Gets the view that is bound.
    /// </summary>
    TView View { get; }

    /// <summary>
    /// Gets the expression representing the view property that is bound.
    /// </summary>
    Expression? ViewExpression { get; }

    /// <summary>
    /// Gets an observable that signals when the binding value changes.
    /// </summary>
    IObservable<TValue> Changed { get; }

    /// <summary>
    /// Gets the direction of the binding.
    /// </summary>
    BindingDirection Direction { get; }
}
