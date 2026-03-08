// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// An observable that completes immediately without emitting any values.
/// Singleton per <typeparamref name="T"/>. Lightweight replacement for <c>Observable.Empty&lt;T&gt;()</c>.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class EmptyObservable<T> : IObservable<T>
{
    /// <summary>
    /// Gets the singleton instance for this element type.
    /// </summary>
    public static readonly EmptyObservable<T> Instance = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyObservable{T}"/> class.
    /// Prevents external instantiation. Use <see cref="Instance"/> instead.
    /// </summary>
    private EmptyObservable()
    {
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        observer.OnCompleted();
        return EmptyDisposable.Instance;
    }
}
