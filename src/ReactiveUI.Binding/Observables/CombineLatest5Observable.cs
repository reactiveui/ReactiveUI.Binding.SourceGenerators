// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Threading;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Lightweight CombineLatest observable that combines the latest values from 5 source observables.
/// </summary>
/// <typeparam name="T1">The type of element 1.</typeparam>
/// <typeparam name="T2">The type of element 2.</typeparam>
/// <typeparam name="T3">The type of element 3.</typeparam>
/// <typeparam name="T4">The type of element 4.</typeparam>
/// <typeparam name="T5">The type of element 5.</typeparam>
/// <typeparam name="TResult">The result element type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class CombineLatest5Observable<T1, T2, T3, T4, T5, TResult> : IObservable<TResult>
{
    private readonly IObservable<T1> _source1;
    private readonly IObservable<T2> _source2;
    private readonly IObservable<T3> _source3;
    private readonly IObservable<T4> _source4;
    private readonly IObservable<T5> _source5;
    private readonly Func<T1, T2, T3, T4, T5, TResult> _resultSelector;

    public CombineLatest5Observable(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        Func<T1, T2, T3, T4, T5, TResult> resultSelector)
    {
        _source1 = source1 ?? throw new ArgumentNullException(nameof(source1));
        _source2 = source2 ?? throw new ArgumentNullException(nameof(source2));
        _source3 = source3 ?? throw new ArgumentNullException(nameof(source3));
        _source4 = source4 ?? throw new ArgumentNullException(nameof(source4));
        _source5 = source5 ?? throw new ArgumentNullException(nameof(source5));
        _resultSelector = resultSelector ?? throw new ArgumentNullException(nameof(resultSelector));
    }

    public IDisposable Subscribe(IObserver<TResult> observer)
    {
        if (observer is null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        var sub = new Subscription(observer, _resultSelector);
        sub.Subscribe1(_source1);
        sub.Subscribe2(_source2);
        sub.Subscribe3(_source3);
        sub.Subscribe4(_source4);
        sub.Subscribe5(_source5);
        return sub;
    }

    private sealed class Subscription : IDisposable
    {
        private readonly Func<T1, T2, T3, T4, T5, TResult> _resultSelector;
        private readonly IDisposable?[] _subscriptions = new IDisposable?[5];
        private IObserver<TResult>? _observer;
        private T1 _value1 = default!;
        private T2 _value2 = default!;
        private T3 _value3 = default!;
        private T4 _value4 = default!;
        private T5 _value5 = default!;
        private bool _has1;
        private bool _has2;
        private bool _has3;
        private bool _has4;
        private bool _has5;

        public Subscription(IObserver<TResult> observer, Func<T1, T2, T3, T4, T5, TResult> resultSelector)
        {
            _observer = observer;
            _resultSelector = resultSelector;
        }

        public void Subscribe1(IObservable<T1> source)
        {
            var sub = source.Subscribe(new Observer1(this));
            Volatile.Write(ref _subscriptions[0], sub);
        }

        public void Subscribe2(IObservable<T2> source)
        {
            var sub = source.Subscribe(new Observer2(this));
            Volatile.Write(ref _subscriptions[1], sub);
        }

        public void Subscribe3(IObservable<T3> source)
        {
            var sub = source.Subscribe(new Observer3(this));
            Volatile.Write(ref _subscriptions[2], sub);
        }

        public void Subscribe4(IObservable<T4> source)
        {
            var sub = source.Subscribe(new Observer4(this));
            Volatile.Write(ref _subscriptions[3], sub);
        }

        public void Subscribe5(IObservable<T5> source)
        {
            var sub = source.Subscribe(new Observer5(this));
            Volatile.Write(ref _subscriptions[4], sub);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _observer, null) != null)
            {
                for (int i = 0; i < _subscriptions.Length; i++)
                {
                    Interlocked.Exchange(ref _subscriptions[i], null)?.Dispose();
                }
            }
        }

        private void TryEmit()
        {
            if (_has1 && _has2 && _has3 && _has4 && _has5)
            {
                _observer?.OnNext(_resultSelector(_value1, _value2, _value3, _value4, _value5));
            }
        }

        private sealed class Observer1 : IObserver<T1>
        {
            private readonly Subscription _parent;

            public Observer1(Subscription parent) => _parent = parent;

            public void OnNext(T1 value)
            {
                _parent._value1 = value;
                _parent._has1 = true;
                _parent.TryEmit();
            }

            public void OnError(Exception error) => _parent._observer?.OnError(error);

            public void OnCompleted()
            {
            }
        }

        private sealed class Observer2 : IObserver<T2>
        {
            private readonly Subscription _parent;

            public Observer2(Subscription parent) => _parent = parent;

            public void OnNext(T2 value)
            {
                _parent._value2 = value;
                _parent._has2 = true;
                _parent.TryEmit();
            }

            public void OnError(Exception error) => _parent._observer?.OnError(error);

            public void OnCompleted()
            {
            }
        }

        private sealed class Observer3 : IObserver<T3>
        {
            private readonly Subscription _parent;

            public Observer3(Subscription parent) => _parent = parent;

            public void OnNext(T3 value)
            {
                _parent._value3 = value;
                _parent._has3 = true;
                _parent.TryEmit();
            }

            public void OnError(Exception error) => _parent._observer?.OnError(error);

            public void OnCompleted()
            {
            }
        }

        private sealed class Observer4 : IObserver<T4>
        {
            private readonly Subscription _parent;

            public Observer4(Subscription parent) => _parent = parent;

            public void OnNext(T4 value)
            {
                _parent._value4 = value;
                _parent._has4 = true;
                _parent.TryEmit();
            }

            public void OnError(Exception error) => _parent._observer?.OnError(error);

            public void OnCompleted()
            {
            }
        }

        private sealed class Observer5 : IObserver<T5>
        {
            private readonly Subscription _parent;

            public Observer5(Subscription parent) => _parent = parent;

            public void OnNext(T5 value)
            {
                _parent._value5 = value;
                _parent._has5 = true;
                _parent.TryEmit();
            }

            public void OnError(Exception error) => _parent._observer?.OnError(error);

            public void OnCompleted()
            {
            }
        }
    }
}
