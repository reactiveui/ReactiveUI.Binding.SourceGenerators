// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Lightweight CombineLatest observable that combines the latest values from 9 source observables.
/// </summary>
/// <typeparam name="T1">The type of element 1.</typeparam>
/// <typeparam name="T2">The type of element 2.</typeparam>
/// <typeparam name="T3">The type of element 3.</typeparam>
/// <typeparam name="T4">The type of element 4.</typeparam>
/// <typeparam name="T5">The type of element 5.</typeparam>
/// <typeparam name="T6">The type of element 6.</typeparam>
/// <typeparam name="T7">The type of element 7.</typeparam>
/// <typeparam name="T8">The type of element 8.</typeparam>
/// <typeparam name="T9">The type of element 9.</typeparam>
/// <typeparam name="TResult">The result element type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
[ExcludeFromCodeCoverage]
internal sealed class CombineLatest9Observable<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> : IObservable<TResult>
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
    /// The 3rd source observable sequence.
    /// </summary>
    private readonly IObservable<T3> _source3;

    /// <summary>
    /// The 4th source observable sequence.
    /// </summary>
    private readonly IObservable<T4> _source4;

    /// <summary>
    /// The 5th source observable sequence.
    /// </summary>
    private readonly IObservable<T5> _source5;

    /// <summary>
    /// The 6th source observable sequence.
    /// </summary>
    private readonly IObservable<T6> _source6;

    /// <summary>
    /// The 7th source observable sequence.
    /// </summary>
    private readonly IObservable<T7> _source7;

    /// <summary>
    /// The 8th source observable sequence.
    /// </summary>
    private readonly IObservable<T8> _source8;

    /// <summary>
    /// The 9th source observable sequence.
    /// </summary>
    private readonly IObservable<T9> _source9;

    /// <summary>
    /// The function to combine the latest values from all sources into a result.
    /// </summary>
    private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> _resultSelector;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombineLatest9Observable{T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult}"/> class.
    /// </summary>
    /// <param name="source1">The 1st source observable.</param>
    /// <param name="source2">The 2nd source observable.</param>
    /// <param name="source3">The 3rd source observable.</param>
    /// <param name="source4">The 4th source observable.</param>
    /// <param name="source5">The 5th source observable.</param>
    /// <param name="source6">The 6th source observable.</param>
    /// <param name="source7">The 7th source observable.</param>
    /// <param name="source8">The 8th source observable.</param>
    /// <param name="source9">The 9th source observable.</param>
    /// <param name="resultSelector">The function to combine the latest values.</param>
    public CombineLatest9Observable(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        IObservable<T6> source6,
        IObservable<T7> source7,
        IObservable<T8> source8,
        IObservable<T9> source9,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> resultSelector)
    {
        ArgumentExceptionHelper.ThrowIfNull(source1);
        ArgumentExceptionHelper.ThrowIfNull(source2);
        ArgumentExceptionHelper.ThrowIfNull(source3);
        ArgumentExceptionHelper.ThrowIfNull(source4);
        ArgumentExceptionHelper.ThrowIfNull(source5);
        ArgumentExceptionHelper.ThrowIfNull(source6);
        ArgumentExceptionHelper.ThrowIfNull(source7);
        ArgumentExceptionHelper.ThrowIfNull(source8);
        ArgumentExceptionHelper.ThrowIfNull(source9);
        ArgumentExceptionHelper.ThrowIfNull(resultSelector);
        _source1 = source1;
        _source2 = source2;
        _source3 = source3;
        _source4 = source4;
        _source5 = source5;
        _source6 = source6;
        _source7 = source7;
        _source8 = source8;
        _source9 = source9;
        _resultSelector = resultSelector;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<TResult> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        var sub = new Subscription(observer, _resultSelector);
        sub.Subscribe1(_source1);
        sub.Subscribe2(_source2);
        sub.Subscribe3(_source3);
        sub.Subscribe4(_source4);
        sub.Subscribe5(_source5);
        sub.Subscribe6(_source6);
        sub.Subscribe7(_source7);
        sub.Subscribe8(_source8);
        sub.Subscribe9(_source9);
        return sub;
    }

    /// <summary>
    /// Manages the active subscriptions to all nine source observables and emits combined results.
    /// </summary>
    private sealed class Subscription : IDisposable
    {
        /// <summary>
        /// The function to combine the latest values from all sources into a result.
        /// </summary>
        private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> _resultSelector;

        /// <summary>
        /// The array of inner source subscriptions.
        /// </summary>
        private readonly IDisposable?[] _subscriptions = new IDisposable?[9];

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
        /// The latest value received from source 3.
        /// </summary>
        private T3 _value3 = default!;

        /// <summary>
        /// The latest value received from source 4.
        /// </summary>
        private T4 _value4 = default!;

        /// <summary>
        /// The latest value received from source 5.
        /// </summary>
        private T5 _value5 = default!;

        /// <summary>
        /// The latest value received from source 6.
        /// </summary>
        private T6 _value6 = default!;

        /// <summary>
        /// The latest value received from source 7.
        /// </summary>
        private T7 _value7 = default!;

        /// <summary>
        /// The latest value received from source 8.
        /// </summary>
        private T8 _value8 = default!;

        /// <summary>
        /// The latest value received from source 9.
        /// </summary>
        private T9 _value9 = default!;

        /// <summary>
        /// Whether source 1 has emitted at least one value.
        /// </summary>
        private bool _has1;

        /// <summary>
        /// Whether source 2 has emitted at least one value.
        /// </summary>
        private bool _has2;

        /// <summary>
        /// Whether source 3 has emitted at least one value.
        /// </summary>
        private bool _has3;

        /// <summary>
        /// Whether source 4 has emitted at least one value.
        /// </summary>
        private bool _has4;

        /// <summary>
        /// Whether source 5 has emitted at least one value.
        /// </summary>
        private bool _has5;

        /// <summary>
        /// Whether source 6 has emitted at least one value.
        /// </summary>
        private bool _has6;

        /// <summary>
        /// Whether source 7 has emitted at least one value.
        /// </summary>
        private bool _has7;

        /// <summary>
        /// Whether source 8 has emitted at least one value.
        /// </summary>
        private bool _has8;

        /// <summary>
        /// Whether source 9 has emitted at least one value.
        /// </summary>
        private bool _has9;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription"/> class.
        /// </summary>
        /// <param name="observer">The downstream observer.</param>
        /// <param name="resultSelector">The function to combine the latest values.</param>
        public Subscription(IObserver<TResult> observer, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> resultSelector)
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

        /// <summary>
        /// Subscribes to the 3rd source observable.
        /// </summary>
        /// <param name="source">The 3rd source observable.</param>
        public void Subscribe3(IObservable<T3> source)
        {
            var sub = source.Subscribe(new Observer3(this));
            Volatile.Write(ref _subscriptions[2], sub);
        }

        /// <summary>
        /// Subscribes to the 4th source observable.
        /// </summary>
        /// <param name="source">The 4th source observable.</param>
        public void Subscribe4(IObservable<T4> source)
        {
            var sub = source.Subscribe(new Observer4(this));
            Volatile.Write(ref _subscriptions[3], sub);
        }

        /// <summary>
        /// Subscribes to the 5th source observable.
        /// </summary>
        /// <param name="source">The 5th source observable.</param>
        public void Subscribe5(IObservable<T5> source)
        {
            var sub = source.Subscribe(new Observer5(this));
            Volatile.Write(ref _subscriptions[4], sub);
        }

        /// <summary>
        /// Subscribes to the 6th source observable.
        /// </summary>
        /// <param name="source">The 6th source observable.</param>
        public void Subscribe6(IObservable<T6> source)
        {
            var sub = source.Subscribe(new Observer6(this));
            Volatile.Write(ref _subscriptions[5], sub);
        }

        /// <summary>
        /// Subscribes to the 7th source observable.
        /// </summary>
        /// <param name="source">The 7th source observable.</param>
        public void Subscribe7(IObservable<T7> source)
        {
            var sub = source.Subscribe(new Observer7(this));
            Volatile.Write(ref _subscriptions[6], sub);
        }

        /// <summary>
        /// Subscribes to the 8th source observable.
        /// </summary>
        /// <param name="source">The 8th source observable.</param>
        public void Subscribe8(IObservable<T8> source)
        {
            var sub = source.Subscribe(new Observer8(this));
            Volatile.Write(ref _subscriptions[7], sub);
        }

        /// <summary>
        /// Subscribes to the 9th source observable.
        /// </summary>
        /// <param name="source">The 9th source observable.</param>
        public void Subscribe9(IObservable<T9> source)
        {
            var sub = source.Subscribe(new Observer9(this));
            Volatile.Write(ref _subscriptions[8], sub);
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
            if (_has1 && _has2 && _has3 && _has4 && _has5 && _has6 && _has7 && _has8 && _has9)
            {
                _observer?.OnNext(_resultSelector(_value1, _value2, _value3, _value4, _value5, _value6, _value7, _value8, _value9));
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

        /// <summary>
        /// Observer for the 3rd source observable.
        /// </summary>
        /// <param name="parent">The parent subscription.</param>
        private sealed class Observer3(Subscription parent) : IObserver<T3>
        {
            /// <inheritdoc/>
            public void OnNext(T3 value)
            {
                parent._value3 = value;
                parent._has3 = true;
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
        /// Observer for the 4th source observable.
        /// </summary>
        /// <param name="parent">The parent subscription.</param>
        private sealed class Observer4(Subscription parent) : IObserver<T4>
        {
            /// <inheritdoc/>
            public void OnNext(T4 value)
            {
                parent._value4 = value;
                parent._has4 = true;
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
        /// Observer for the 5th source observable.
        /// </summary>
        /// <param name="parent">The parent subscription.</param>
        private sealed class Observer5(Subscription parent) : IObserver<T5>
        {
            /// <inheritdoc/>
            public void OnNext(T5 value)
            {
                parent._value5 = value;
                parent._has5 = true;
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
        /// Observer for the 6th source observable.
        /// </summary>
        /// <param name="parent">The parent subscription.</param>
        private sealed class Observer6(Subscription parent) : IObserver<T6>
        {
            /// <inheritdoc/>
            public void OnNext(T6 value)
            {
                parent._value6 = value;
                parent._has6 = true;
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
        /// Observer for the 7th source observable.
        /// </summary>
        /// <param name="parent">The parent subscription.</param>
        private sealed class Observer7(Subscription parent) : IObserver<T7>
        {
            /// <inheritdoc/>
            public void OnNext(T7 value)
            {
                parent._value7 = value;
                parent._has7 = true;
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
        /// Observer for the 8th source observable.
        /// </summary>
        /// <param name="parent">The parent subscription.</param>
        private sealed class Observer8(Subscription parent) : IObserver<T8>
        {
            /// <inheritdoc/>
            public void OnNext(T8 value)
            {
                parent._value8 = value;
                parent._has8 = true;
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
        /// Observer for the 9th source observable.
        /// </summary>
        /// <param name="parent">The parent subscription.</param>
        private sealed class Observer9(Subscription parent) : IObserver<T9>
        {
            /// <inheritdoc/>
            public void OnNext(T9 value)
            {
                parent._value9 = value;
                parent._has9 = true;
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
