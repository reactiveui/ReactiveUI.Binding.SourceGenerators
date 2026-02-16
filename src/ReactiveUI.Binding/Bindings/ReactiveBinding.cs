// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq.Expressions;
using System.Threading;

namespace ReactiveUI.Binding;

/// <summary>
/// Default implementation of <see cref="IReactiveBinding{TView, TValue}"/> used by generated view-first bindings.
/// </summary>
/// <typeparam name="TView">The type of the view.</typeparam>
/// <typeparam name="TValue">The type of the bound value.</typeparam>
public sealed class ReactiveBinding<TView, TValue> : IReactiveBinding<TView, TValue>
    where TView : IViewFor
{
    private readonly IDisposable _subscription;
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactiveBinding{TView, TValue}"/> class.
    /// </summary>
    /// <param name="view">The view that is bound.</param>
    /// <param name="changed">An observable that signals when the binding value changes.</param>
    /// <param name="direction">The direction of the binding.</param>
    /// <param name="subscription">The underlying subscription to dispose when the binding is disposed.</param>
    public ReactiveBinding(
        TView view,
        IObservable<TValue> changed,
        BindingDirection direction,
        IDisposable subscription)
    {
        View = view;
        Changed = changed;
        Direction = direction;
        _subscription = subscription;
    }

    /// <inheritdoc/>
    public Expression? ViewModelExpression => null;

    /// <inheritdoc/>
    public TView View { get; }

    /// <inheritdoc/>
    public Expression? ViewExpression => null;

    /// <inheritdoc/>
    public IObservable<TValue> Changed { get; }

    /// <inheritdoc/>
    public BindingDirection Direction { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            _subscription.Dispose();
        }
    }
}
