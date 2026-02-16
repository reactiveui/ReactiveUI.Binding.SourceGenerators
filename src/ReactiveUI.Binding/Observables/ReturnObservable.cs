// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// An observable that emits a single value and then completes.
/// Lightweight replacement for <c>Observable.Return&lt;T&gt;(value)</c>.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ReturnObservable<T> : IObservable<T>
{
    private readonly T _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReturnObservable{T}"/> class.
    /// </summary>
    /// <param name="value">The value to emit.</param>
    public ReturnObservable(T value)
    {
        _value = value;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (observer is null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        observer.OnNext(_value);
        observer.OnCompleted();
        return EmptyDisposable.Instance;
    }
}
