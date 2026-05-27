// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Lightweight Where (filter) operator that forwards only values matching a predicate.
/// Replacement for <c>System.Reactive.Linq.Observable.Where</c>.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class WhereObservable<T> : IObservable<T>
{
    /// <summary>
    /// The upstream source observable.
    /// </summary>
    private readonly IObservable<T> _source;

    /// <summary>
    /// The predicate that determines which values are forwarded.
    /// </summary>
    private readonly Func<T, bool> _predicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="WhereObservable{T}"/> class.
    /// </summary>
    /// <param name="source">The source observable.</param>
    /// <param name="predicate">The predicate that determines which values are forwarded.</param>
    public WhereObservable(IObservable<T> source, Func<T, bool> predicate)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(predicate);
        _source = source;
        _predicate = predicate;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return _source.Subscribe(new WhereObserver(observer, _predicate));
    }

    /// <summary>
    /// Observer that forwards only values matching the predicate.
    /// </summary>
    private sealed class WhereObserver : IObserver<T>
    {
        /// <summary>
        /// The downstream observer.
        /// </summary>
        private readonly IObserver<T> _observer;

        /// <summary>
        /// The predicate that determines which values are forwarded.
        /// </summary>
        private readonly Func<T, bool> _predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="WhereObserver"/> class.
        /// </summary>
        /// <param name="observer">The downstream observer.</param>
        /// <param name="predicate">The predicate that determines which values are forwarded.</param>
        public WhereObserver(IObserver<T> observer, Func<T, bool> predicate)
        {
            _observer = observer;
            _predicate = predicate;
        }

        /// <inheritdoc/>
        public void OnNext(T value)
        {
            bool matches;
            try
            {
                matches = _predicate(value);
            }
            catch (Exception ex)
            {
                _observer.OnError(ex);
                return;
            }

            if (!matches)
            {
                return;
            }

            _observer.OnNext(value);
        }

        /// <inheritdoc/>
        public void OnError(Exception error) => _observer.OnError(error);

        /// <inheritdoc/>
        public void OnCompleted() => _observer.OnCompleted();
    }
}
