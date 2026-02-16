// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Lightweight DistinctUntilChanged operator that suppresses consecutive duplicate values.
/// Replacement for <c>System.Reactive.Linq.Observable.DistinctUntilChanged</c>.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DistinctUntilChangedObservable<T> : IObservable<T>
{
    private readonly IObservable<T> _source;
    private readonly IEqualityComparer<T> _comparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistinctUntilChangedObservable{T}"/> class.
    /// </summary>
    /// <param name="source">The source observable.</param>
    public DistinctUntilChangedObservable(IObservable<T> source)
        : this(source, EqualityComparer<T>.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DistinctUntilChangedObservable{T}"/> class.
    /// </summary>
    /// <param name="source">The source observable.</param>
    /// <param name="comparer">The equality comparer to use.</param>
    public DistinctUntilChangedObservable(IObservable<T> source, IEqualityComparer<T> comparer)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (observer is null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        return _source.Subscribe(new DistinctObserver(observer, _comparer));
    }

    private sealed class DistinctObserver : IObserver<T>
    {
        private readonly IObserver<T> _observer;
        private readonly IEqualityComparer<T> _comparer;
        private T _lastValue = default!;
        private bool _hasValue;

        public DistinctObserver(IObserver<T> observer, IEqualityComparer<T> comparer)
        {
            _observer = observer;
            _comparer = comparer;
        }

        public void OnNext(T value)
        {
            if (_hasValue && _comparer.Equals(value, _lastValue))
            {
                return;
            }

            _lastValue = value;
            _hasValue = true;
            _observer.OnNext(value);
        }

        public void OnError(Exception error) => _observer.OnError(error);

        public void OnCompleted() => _observer.OnCompleted();
    }
}
