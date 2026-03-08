// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Lightweight CombineLatest observable that combines the latest values from 2 source observables.
/// </summary>
/// <typeparam name="T1">The type of element 1.</typeparam>
/// <typeparam name="T2">The type of element 2.</typeparam>
/// <typeparam name="TResult">The result element type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromCodeCoverage]
internal sealed class CombineLatest2Observable<T1, T2, TResult> : IObservable<TResult>
{
    /// <summary>
    /// The 1st source observable sequence.
    /// </summary>
    private readonly IObservable<T1> _source1;

    /// <summary>
    /// The 2nd source observable sequence.
    /// </summary>
    private readonly IObservable<T2> _source2;

    /// <summary>
    /// The function to combine the latest values from all sources into a result.
    /// </summary>
    private readonly Func<T1, T2, TResult> _resultSelector;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombineLatest2Observable{T1, T2, TResult}"/> class.
    /// </summary>
    /// <param name="source1">The 1st source observable.</param>
    /// <param name="source2">The 2nd source observable.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    public CombineLatest2Observable(
        IObservable<T1> source1,
        IObservable<T2> source2,
        Func<T1, T2, TResult> resultSelector)
    {
        ArgumentExceptionHelper.ThrowIfNull(source1);
        ArgumentExceptionHelper.ThrowIfNull(source2);
        ArgumentExceptionHelper.ThrowIfNull(resultSelector);
        _source1 = source1;
        _source2 = source2;
        _resultSelector = resultSelector;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<TResult> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        var sub = new Subscription(observer, _resultSelector);
        sub.Subscribe1(_source1);
        sub.Subscribe2(_source2);
        return sub;
    }

    /// <summary>
    /// Manages the active subscriptions to all two source observables and emits combined results.
    /// </summary>
    private sealed class Subscription : IDisposable
    {
        /// <summary>
        /// The function to combine the latest values from all sources into a result.
        /// </summary>
        private readonly Func<T1, T2, TResult> _resultSelector;

        /// <summary>
        /// The array of inner source subscriptions.
        /// </summary>
        private readonly IDisposable?[] _subscriptions = new IDisposable?[2];

        /// <summary>
        /// The downstream observer receiving combined results. Set to <see langword="null"/> on disposal.
        /// </summary>
        private IObserver<TResult>? _observer;

        /// <summary>
        /// The latest value received from source 1.
        /// </summary>
        private T1 _value1 = default!;

        /// <summary>
        /// The latest value received from source 2.
        /// </summary>
        private T2 _value2 = default!;

        /// <summary>
        /// Whether source 1 has emitted at least one value.
        /// </summary>
        private bool _has1;

        /// <summary>
        /// Whether source 2 has emitted at least one value.
        /// </summary>
        private bool _has2;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription"/> class.
        /// </summary>
        /// <param name="observer">The downstream observer.</param>
        /// <param name="resultSelector">The function to combine the latest values.</param>
        public Subscription(IObserver<TResult> observer, Func<T1, T2, TResult> resultSelector)
        {
            _observer = observer;
            _resultSelector = resultSelector;
        }

        /// <summary>
        /// Subscribes to the 1st source observable.
        /// </summary>
        /// <param name="source">The 1st source observable.</param>
        public void Subscribe1(IObservable<T1> source)
        {
            var sub = source.Subscribe(new Observer1(this));
            Volatile.Write(ref _subscriptions[0], sub);
        }

        /// <summary>
        /// Subscribes to the 2nd source observable.
        /// </summary>
        /// <param name="source">The 2nd source observable.</param>
        public void Subscribe2(IObservable<T2> source)
        {
            var sub = source.Subscribe(new Observer2(this));
            Volatile.Write(ref _subscriptions[1], sub);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _observer, null) != null)
            {
                for (var i = 0; i < _subscriptions.Length; i++)
                {
                    Interlocked.Exchange(ref _subscriptions[i], null)?.Dispose();
                }
            }
        }

        /// <summary>
        /// Emits the combined result if all sources have produced at least one value.
        /// </summary>
        private void TryEmit()
        {
            if (_has1 && _has2)
            {
                _observer?.OnNext(_resultSelector(_value1, _value2));
            }
        }

        /// <summary>
        /// Observer for the 1st source observable.
        /// </summary>
        /// <param name="parent">The parent subscription.</param>
        private sealed class Observer1(Subscription parent) : IObserver<T1>
        {
            /// <inheritdoc/>
            public void OnNext(T1 value)
            {
                parent._value1 = value;
                parent._has1 = true;
                parent.TryEmit();
            }

            /// <inheritdoc/>
            public void OnError(Exception error) => parent._observer?.OnError(error);

            /// <inheritdoc/>
            public void OnCompleted()
            {
            }
        }

        /// <summary>
        /// Observer for the 2nd source observable.
        /// </summary>
        /// <param name="parent">The parent subscription.</param>
        private sealed class Observer2(Subscription parent) : IObserver<T2>
        {
            /// <inheritdoc/>
            public void OnNext(T2 value)
            {
                parent._value2 = value;
                parent._has2 = true;
                parent.TryEmit();
            }

            /// <inheritdoc/>
            public void OnError(Exception error) => parent._observer?.OnError(error);

            /// <inheritdoc/>
            public void OnCompleted()
            {
            }
        }
    }
}
