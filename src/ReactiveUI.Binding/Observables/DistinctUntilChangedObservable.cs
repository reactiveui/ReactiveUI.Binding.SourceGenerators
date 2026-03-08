// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
    /// <summary>
    /// The upstream source observable.
    /// </summary>
    private readonly IObservable<T> _source;

    /// <summary>
    /// The equality comparer used to detect duplicate consecutive values.
    /// </summary>
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
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(comparer);
        _source = source;
        _comparer = comparer;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return _source.Subscribe(new DistinctObserver(observer, _comparer));
    }

    /// <summary>
    /// Observer that suppresses consecutive duplicate values.
    /// </summary>
    private sealed class DistinctObserver : IObserver<T>
    {
        /// <summary>
        /// The downstream observer.
        /// </summary>
        private readonly IObserver<T> _observer;

        /// <summary>
        /// The equality comparer used to detect duplicates.
        /// </summary>
        private readonly IEqualityComparer<T> _comparer;

        /// <summary>
        /// The most recently emitted value.
        /// </summary>
        private T _lastValue = default!;

        /// <summary>
        /// Whether at least one value has been emitted.
        /// </summary>
        private bool _hasValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistinctObserver"/> class.
        /// </summary>
        /// <param name="observer">The downstream observer.</param>
        /// <param name="comparer">The equality comparer.</param>
        public DistinctObserver(IObserver<T> observer, IEqualityComparer<T> comparer)
        {
            _observer = observer;
            _comparer = comparer;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void OnError(Exception error) => _observer.OnError(error);

        /// <inheritdoc/>
        public void OnCompleted() => _observer.OnCompleted();
    }
}
