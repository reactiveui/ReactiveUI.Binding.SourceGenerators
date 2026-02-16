// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Threading;

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
    private readonly IObservable<TSource> _source;
    private readonly Func<TSource, TResult> _selector;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectObservable{TSource, TResult}"/> class.
    /// </summary>
    /// <param name="source">The source observable.</param>
    /// <param name="selector">The projection function.</param>
    public SelectObservable(IObservable<TSource> source, Func<TSource, TResult> selector)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _selector = selector ?? throw new ArgumentNullException(nameof(selector));
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<TResult> observer)
    {
        if (observer is null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        return _source.Subscribe(new SelectObserver(observer, _selector));
    }

    private sealed class SelectObserver : IObserver<TSource>
    {
        private readonly IObserver<TResult> _observer;
        private readonly Func<TSource, TResult> _selector;

        public SelectObserver(IObserver<TResult> observer, Func<TSource, TResult> selector)
        {
            _observer = observer;
            _selector = selector;
        }

        public void OnNext(TSource value)
        {
            _observer.OnNext(_selector(value));
        }

        public void OnError(Exception error) => _observer.OnError(error);

        public void OnCompleted() => _observer.OnCompleted();
    }
}
