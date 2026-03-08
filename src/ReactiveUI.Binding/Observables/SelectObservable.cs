// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Lightweight Select (map/projection) operator.
/// Replacement for <c>System.Reactive.Linq.Observable.Select</c>.
/// </summary>
/// <typeparam name="TSource">The source element type.</typeparam>
/// <typeparam name="TResult">The projected element type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class SelectObservable<TSource, TResult> : IObservable<TResult>
{
    /// <summary>
    /// The upstream source observable.
    /// </summary>
    private readonly IObservable<TSource> _source;

    /// <summary>
    /// The projection function applied to each element.
    /// </summary>
    private readonly Func<TSource, TResult> _selector;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectObservable{TSource, TResult}"/> class.
    /// </summary>
    /// <param name="source">The source observable.</param>
    /// <param name="selector">The projection function.</param>
    public SelectObservable(IObservable<TSource> source, Func<TSource, TResult> selector)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(selector);
        _source = source;
        _selector = selector;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<TResult> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return _source.Subscribe(new SelectObserver(observer, _selector));
    }

    /// <summary>
    /// Observer that applies the projection function to each source element.
    /// </summary>
    private sealed class SelectObserver : IObserver<TSource>
    {
        /// <summary>
        /// The downstream observer receiving projected values.
        /// </summary>
        private readonly IObserver<TResult> _observer;

        /// <summary>
        /// The projection function applied to each element.
        /// </summary>
        private readonly Func<TSource, TResult> _selector;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectObserver"/> class.
        /// </summary>
        /// <param name="observer">The downstream observer.</param>
        /// <param name="selector">The projection function.</param>
        public SelectObserver(IObserver<TResult> observer, Func<TSource, TResult> selector)
        {
            _observer = observer;
            _selector = selector;
        }

        /// <inheritdoc/>
        public void OnNext(TSource value) => _observer.OnNext(_selector(value));

        /// <inheritdoc/>
        public void OnError(Exception error) => _observer.OnError(error);

        /// <inheritdoc/>
        public void OnCompleted() => _observer.OnCompleted();
    }
}
