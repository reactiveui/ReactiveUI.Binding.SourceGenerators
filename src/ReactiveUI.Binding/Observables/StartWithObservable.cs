// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Prepends a single value to a source sequence, emitting it synchronously on subscription before
/// relaying the source. Lightweight replacement for <c>System.Reactive.Linq.Observable.StartWith</c>.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class StartWithObservable<T> : IObservable<T>
{
    /// <summary>
    /// The upstream source observable.
    /// </summary>
    private readonly IObservable<T> _source;

    /// <summary>
    /// The value emitted before the source sequence.
    /// </summary>
    private readonly T _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartWithObservable{T}"/> class.
    /// </summary>
    /// <param name="source">The source observable.</param>
    /// <param name="value">The value to emit before the source sequence.</param>
    public StartWithObservable(IObservable<T> source, T value)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        _source = source;
        _value = value;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        observer.OnNext(_value);
        return _source.Subscribe(observer);
    }
}
