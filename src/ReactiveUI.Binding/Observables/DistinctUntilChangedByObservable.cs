// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// DistinctUntilChanged operator that compares consecutive values by a projected key.
/// Replacement for <c>System.Reactive.Linq.Observable.DistinctUntilChanged(source, keySelector)</c>.
/// </summary>
/// <typeparam name="TSource">The element type.</typeparam>
/// <typeparam name="TKey">The key type used for comparison.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DistinctUntilChangedByObservable<TSource, TKey> : IObservable<TSource>
{
    /// <summary>
    /// The upstream source observable.
    /// </summary>
    private readonly IObservable<TSource> _source;

    /// <summary>
    /// The key selector used to derive the comparison key from each value.
    /// </summary>
    private readonly Func<TSource, TKey> _keySelector;

    /// <summary>
    /// The equality comparer used to detect duplicate consecutive keys.
    /// </summary>
    private readonly IEqualityComparer<TKey> _comparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistinctUntilChangedByObservable{TSource, TKey}"/> class.
    /// </summary>
    /// <param name="source">The source observable.</param>
    /// <param name="keySelector">The key selector used to derive the comparison key from each value.</param>
    public DistinctUntilChangedByObservable(IObservable<TSource> source, Func<TSource, TKey> keySelector)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(keySelector);
        _source = source;
        _keySelector = keySelector;
        _comparer = EqualityComparer<TKey>.Default;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<TSource> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return _source.Subscribe(new DistinctObserver(observer, _keySelector, _comparer));
    }

    /// <summary>
    /// Observer that suppresses consecutive values whose projected keys are equal.
    /// </summary>
    private sealed class DistinctObserver : IObserver<TSource>
    {
        /// <summary>
        /// The downstream observer.
        /// </summary>
        private readonly IObserver<TSource> _observer;

        /// <summary>
        /// The key selector used to derive the comparison key from each value.
        /// </summary>
        private readonly Func<TSource, TKey> _keySelector;

        /// <summary>
        /// The equality comparer used to detect duplicate consecutive keys.
        /// </summary>
        private readonly IEqualityComparer<TKey> _comparer;

        /// <summary>
        /// The key of the most recently emitted value.
        /// </summary>
        private TKey _lastKey = default!;

        /// <summary>
        /// Whether at least one value has been emitted.
        /// </summary>
        private bool _hasValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistinctObserver"/> class.
        /// </summary>
        /// <param name="observer">The downstream observer.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="comparer">The equality comparer.</param>
        public DistinctObserver(IObserver<TSource> observer, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            _observer = observer;
            _keySelector = keySelector;
            _comparer = comparer;
        }

        /// <inheritdoc/>
        public void OnNext(TSource value)
        {
            TKey key;
            try
            {
                key = _keySelector(value);
            }
            catch (Exception ex)
            {
                _observer.OnError(ex);
                return;
            }

            if (_hasValue && _comparer.Equals(key, _lastKey))
            {
                return;
            }

            _lastKey = key;
            _hasValue = true;
            _observer.OnNext(value);
        }

        /// <inheritdoc/>
        public void OnError(Exception error) => _observer.OnError(error);

        /// <inheritdoc/>
        public void OnCompleted() => _observer.OnCompleted();
    }
}
