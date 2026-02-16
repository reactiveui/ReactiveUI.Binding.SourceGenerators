// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Threading;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Lightweight Switch operator that flattens an <c>IObservable&lt;IObservable&lt;T&gt;&gt;</c>
/// by subscribing to the most recent inner observable and disposing the previous subscription.
/// Replacement for <c>System.Reactive.Linq.Observable.Switch</c>.
/// </summary>
/// <typeparam name="T">The element type of the inner observables.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class SwitchObservable<T> : IObservable<T>
{
    private readonly IObservable<IObservable<T>> _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchObservable{T}"/> class.
    /// </summary>
    /// <param name="source">The outer observable of inner observables.</param>
    public SwitchObservable(IObservable<IObservable<T>> source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (observer is null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        var subscription = new SwitchSubscription(observer);
        subscription.OuterSubscription = _source.Subscribe(subscription);
        return subscription;
    }

    private sealed class SwitchSubscription : IObserver<IObservable<T>>, IDisposable
    {
        private readonly IObserver<T> _observer;
        private IDisposable? _innerSubscription;
        private int _disposed;

        public SwitchSubscription(IObserver<T> observer)
        {
            _observer = observer;
        }

        public IDisposable? OuterSubscription { get; set; }

        public void OnNext(IObservable<T> value)
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                return;
            }

            // Dispose previous inner subscription
            Interlocked.Exchange(ref _innerSubscription, null)?.Dispose();

            if (value is null)
            {
                return;
            }

            var innerObserver = new InnerObserver(this);
            var sub = value.Subscribe(innerObserver);

            // Store new inner subscription (if not yet disposed)
            if (Interlocked.CompareExchange(ref _innerSubscription, sub, null) != null)
            {
                // Race: another OnNext or Dispose happened; clean up
                sub.Dispose();
            }
        }

        public void OnError(Exception error)
        {
            if (Volatile.Read(ref _disposed) == 0)
            {
                _observer.OnError(error);
            }
        }

        public void OnCompleted()
        {
            // Outer completed; inner may still emit.
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                OuterSubscription?.Dispose();
                Interlocked.Exchange(ref _innerSubscription, null)?.Dispose();
            }
        }

        private sealed class InnerObserver : IObserver<T>
        {
            private readonly SwitchSubscription _parent;

            public InnerObserver(SwitchSubscription parent)
            {
                _parent = parent;
            }

            public void OnNext(T value)
            {
                if (Volatile.Read(ref _parent._disposed) == 0)
                {
                    _parent._observer.OnNext(value);
                }
            }

            public void OnError(Exception error)
            {
                if (Volatile.Read(ref _parent._disposed) == 0)
                {
                    _parent._observer.OnError(error);
                }
            }

            public void OnCompleted()
            {
                // Inner completed; do not propagate (outer decides lifetime).
            }
        }
    }
}
