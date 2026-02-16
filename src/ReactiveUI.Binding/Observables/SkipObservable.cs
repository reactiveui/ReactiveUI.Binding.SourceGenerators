// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Threading;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Lightweight Skip operator that suppresses the first N elements.
/// Replacement for <c>System.Reactive.Linq.Observable.Skip</c>.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class SkipObservable<T> : IObservable<T>
{
    private readonly IObservable<T> _source;
    private readonly int _count;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkipObservable{T}"/> class.
    /// </summary>
    /// <param name="source">The source observable.</param>
    /// <param name="count">The number of elements to skip.</param>
    public SkipObservable(IObservable<T> source, int count)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _count = count;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (observer is null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        return _source.Subscribe(new SkipObserver(observer, _count));
    }

    private sealed class SkipObserver : IObserver<T>
    {
        private readonly IObserver<T> _observer;
        private int _remaining;

        public SkipObserver(IObserver<T> observer, int count)
        {
            _observer = observer;
            _remaining = count;
        }

        public void OnNext(T value)
        {
            if (_remaining > 0)
            {
                _remaining--;
                return;
            }

            _observer.OnNext(value);
        }

        public void OnError(Exception error) => _observer.OnError(error);

        public void OnCompleted() => _observer.OnCompleted();
    }
}
