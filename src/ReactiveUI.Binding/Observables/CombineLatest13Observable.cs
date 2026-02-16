// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Threading;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Lightweight CombineLatest observable that combines the latest values from 13 source observables.
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
/// <typeparam name="T10">The type of element 10.</typeparam>
/// <typeparam name="T11">The type of element 11.</typeparam>
/// <typeparam name="T12">The type of element 12.</typeparam>
/// <typeparam name="T13">The type of element 13.</typeparam>
/// <typeparam name="TResult">The result element type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class CombineLatest13Observable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> : IObservable<TResult>
{
    private readonly IObservable<T1> _source1;
    private readonly IObservable<T2> _source2;
    private readonly IObservable<T3> _source3;
    private readonly IObservable<T4> _source4;
    private readonly IObservable<T5> _source5;
    private readonly IObservable<T6> _source6;
    private readonly IObservable<T7> _source7;
    private readonly IObservable<T8> _source8;
    private readonly IObservable<T9> _source9;
    private readonly IObservable<T10> _source10;
    private readonly IObservable<T11> _source11;
    private readonly IObservable<T12> _source12;
    private readonly IObservable<T13> _source13;
    private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> _resultSelector;

    public CombineLatest13Observable(
        IObservable<T1> source1,
        IObservable<T2> source2,
        IObservable<T3> source3,
        IObservable<T4> source4,
        IObservable<T5> source5,
        IObservable<T6> source6,
        IObservable<T7> source7,
        IObservable<T8> source8,
        IObservable<T9> source9,
        IObservable<T10> source10,
        IObservable<T11> source11,
        IObservable<T12> source12,
        IObservable<T13> source13,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> resultSelector)
    {
        _source1 = source1 ?? throw new ArgumentNullException(nameof(source1));
        _source2 = source2 ?? throw new ArgumentNullException(nameof(source2));
        _source3 = source3 ?? throw new ArgumentNullException(nameof(source3));
        _source4 = source4 ?? throw new ArgumentNullException(nameof(source4));
        _source5 = source5 ?? throw new ArgumentNullException(nameof(source5));
        _source6 = source6 ?? throw new ArgumentNullException(nameof(source6));
        _source7 = source7 ?? throw new ArgumentNullException(nameof(source7));
        _source8 = source8 ?? throw new ArgumentNullException(nameof(source8));
        _source9 = source9 ?? throw new ArgumentNullException(nameof(source9));
        _source10 = source10 ?? throw new ArgumentNullException(nameof(source10));
        _source11 = source11 ?? throw new ArgumentNullException(nameof(source11));
        _source12 = source12 ?? throw new ArgumentNullException(nameof(source12));
        _source13 = source13 ?? throw new ArgumentNullException(nameof(source13));
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
        sub.Subscribe6(_source6);
        sub.Subscribe7(_source7);
        sub.Subscribe8(_source8);
        sub.Subscribe9(_source9);
        sub.Subscribe10(_source10);
        sub.Subscribe11(_source11);
        sub.Subscribe12(_source12);
        sub.Subscribe13(_source13);
        return sub;
    }

    private sealed class Subscription : IDisposable
    {
        private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> _resultSelector;
        private readonly IDisposable?[] _subscriptions = new IDisposable?[13];
        private IObserver<TResult>? _observer;
        private T1 _value1 = default!;
        private T2 _value2 = default!;
        private T3 _value3 = default!;
        private T4 _value4 = default!;
        private T5 _value5 = default!;
        private T6 _value6 = default!;
        private T7 _value7 = default!;
        private T8 _value8 = default!;
        private T9 _value9 = default!;
        private T10 _value10 = default!;
        private T11 _value11 = default!;
        private T12 _value12 = default!;
        private T13 _value13 = default!;
        private bool _has1;
        private bool _has2;
        private bool _has3;
        private bool _has4;
        private bool _has5;
        private bool _has6;
        private bool _has7;
        private bool _has8;
        private bool _has9;
        private bool _has10;
        private bool _has11;
        private bool _has12;
        private bool _has13;

        public Subscription(IObserver<TResult> observer, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> resultSelector)
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

        public void Subscribe6(IObservable<T6> source)
        {
            var sub = source.Subscribe(new Observer6(this));
            Volatile.Write(ref _subscriptions[5], sub);
        }

        public void Subscribe7(IObservable<T7> source)
        {
            var sub = source.Subscribe(new Observer7(this));
            Volatile.Write(ref _subscriptions[6], sub);
        }

        public void Subscribe8(IObservable<T8> source)
        {
            var sub = source.Subscribe(new Observer8(this));
            Volatile.Write(ref _subscriptions[7], sub);
        }

        public void Subscribe9(IObservable<T9> source)
        {
            var sub = source.Subscribe(new Observer9(this));
            Volatile.Write(ref _subscriptions[8], sub);
        }

        public void Subscribe10(IObservable<T10> source)
        {
            var sub = source.Subscribe(new Observer10(this));
            Volatile.Write(ref _subscriptions[9], sub);
        }

        public void Subscribe11(IObservable<T11> source)
        {
            var sub = source.Subscribe(new Observer11(this));
            Volatile.Write(ref _subscriptions[10], sub);
        }

        public void Subscribe12(IObservable<T12> source)
        {
            var sub = source.Subscribe(new Observer12(this));
            Volatile.Write(ref _subscriptions[11], sub);
        }

        public void Subscribe13(IObservable<T13> source)
        {
            var sub = source.Subscribe(new Observer13(this));
            Volatile.Write(ref _subscriptions[12], sub);
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
            if (_has1 && _has2 && _has3 && _has4 && _has5 && _has6 && _has7 && _has8 && _has9 && _has10 && _has11 && _has12 && _has13)
            {
                _observer?.OnNext(_resultSelector(_value1, _value2, _value3, _value4, _value5, _value6, _value7, _value8, _value9, _value10, _value11, _value12, _value13));
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

        private sealed class Observer6 : IObserver<T6>
        {
            private readonly Subscription _parent;

            public Observer6(Subscription parent) => _parent = parent;

            public void OnNext(T6 value)
            {
                _parent._value6 = value;
                _parent._has6 = true;
                _parent.TryEmit();
            }

            public void OnError(Exception error) => _parent._observer?.OnError(error);

            public void OnCompleted()
            {
            }
        }

        private sealed class Observer7 : IObserver<T7>
        {
            private readonly Subscription _parent;

            public Observer7(Subscription parent) => _parent = parent;

            public void OnNext(T7 value)
            {
                _parent._value7 = value;
                _parent._has7 = true;
                _parent.TryEmit();
            }

            public void OnError(Exception error) => _parent._observer?.OnError(error);

            public void OnCompleted()
            {
            }
        }

        private sealed class Observer8 : IObserver<T8>
        {
            private readonly Subscription _parent;

            public Observer8(Subscription parent) => _parent = parent;

            public void OnNext(T8 value)
            {
                _parent._value8 = value;
                _parent._has8 = true;
                _parent.TryEmit();
            }

            public void OnError(Exception error) => _parent._observer?.OnError(error);

            public void OnCompleted()
            {
            }
        }

        private sealed class Observer9 : IObserver<T9>
        {
            private readonly Subscription _parent;

            public Observer9(Subscription parent) => _parent = parent;

            public void OnNext(T9 value)
            {
                _parent._value9 = value;
                _parent._has9 = true;
                _parent.TryEmit();
            }

            public void OnError(Exception error) => _parent._observer?.OnError(error);

            public void OnCompleted()
            {
            }
        }

        private sealed class Observer10 : IObserver<T10>
        {
            private readonly Subscription _parent;

            public Observer10(Subscription parent) => _parent = parent;

            public void OnNext(T10 value)
            {
                _parent._value10 = value;
                _parent._has10 = true;
                _parent.TryEmit();
            }

            public void OnError(Exception error) => _parent._observer?.OnError(error);

            public void OnCompleted()
            {
            }
        }

        private sealed class Observer11 : IObserver<T11>
        {
            private readonly Subscription _parent;

            public Observer11(Subscription parent) => _parent = parent;

            public void OnNext(T11 value)
            {
                _parent._value11 = value;
                _parent._has11 = true;
                _parent.TryEmit();
            }

            public void OnError(Exception error) => _parent._observer?.OnError(error);

            public void OnCompleted()
            {
            }
        }

        private sealed class Observer12 : IObserver<T12>
        {
            private readonly Subscription _parent;

            public Observer12(Subscription parent) => _parent = parent;

            public void OnNext(T12 value)
            {
                _parent._value12 = value;
                _parent._has12 = true;
                _parent.TryEmit();
            }

            public void OnError(Exception error) => _parent._observer?.OnError(error);

            public void OnCompleted()
            {
            }
        }

        private sealed class Observer13 : IObserver<T13>
        {
            private readonly Subscription _parent;

            public Observer13(Subscription parent) => _parent = parent;

            public void OnNext(T13 value)
            {
                _parent._value13 = value;
                _parent._has13 = true;
                _parent.TryEmit();
            }

            public void OnError(Exception error) => _parent._observer?.OnError(error);

            public void OnCompleted()
            {
            }
        }
    }
}
