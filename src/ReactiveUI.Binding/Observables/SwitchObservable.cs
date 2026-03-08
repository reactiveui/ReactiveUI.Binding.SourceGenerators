// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

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
    /// <summary>
    /// The outer observable of inner observables.
    /// </summary>
    private readonly IObservable<IObservable<T>> _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchObservable{T}"/> class.
    /// </summary>
    /// <param name="source">The outer observable of inner observables.</param>
    public SwitchObservable(IObservable<IObservable<T>> source)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        _source = source;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        var subscription = new SwitchSubscription(observer);
        subscription.OuterSubscription = _source.Subscribe(subscription);
        return subscription;
    }

    /// <summary>
    /// Manages the outer subscription and switches inner subscriptions as new inner observables arrive.
    /// </summary>
    private sealed class SwitchSubscription : IObserver<IObservable<T>>, IDisposable
    {
        /// <summary>
        /// The downstream observer receiving values from the current inner observable.
        /// </summary>
        private readonly IObserver<T> _observer;

        /// <summary>
        /// The current inner subscription. Disposed and replaced when a new inner observable arrives.
        /// </summary>
        private IDisposable? _innerSubscription;

        /// <summary>
        /// Guard flag to ensure disposal occurs exactly once (0 = not disposed, 1 = disposed).
        /// </summary>
        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchSubscription"/> class.
        /// </summary>
        /// <param name="observer">The downstream observer.</param>
        public SwitchSubscription(IObserver<T> observer) => _observer = observer;

        /// <summary>
        /// Gets or sets the outer subscription. Set after construction to avoid passing through the constructor.
        /// </summary>
        public IDisposable? OuterSubscription { get; set; }

        /// <inheritdoc/>
        public void OnNext(IObservable<T> value)
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                return;
            }

            // Dispose previous inner subscription
            TakeInnerSubscription()?.Dispose();

            if (value is null)
            {
                return;
            }

            var innerObserver = new InnerObserver(this);
            var sub = value.Subscribe(innerObserver);

            // Store new inner subscription (if not yet disposed)
            DisposeIfRace(sub);
        }

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
            if (Volatile.Read(ref _disposed) == 0)
            {
                _observer.OnError(error);
            }
        }

        /// <inheritdoc/>
        public void OnCompleted()
        {
            // Outer completed; inner may still emit.
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (TrySetDisposed())
            {
                DisposeSubscriptions();
            }
        }

        /// <summary>
        /// Atomically marks this instance as disposed.
        /// </summary>
        /// <returns><see langword="true"/> if this is the first disposal; otherwise <see langword="false"/>.</returns>
        [ExcludeFromCodeCoverage]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TrySetDisposed() => Interlocked.Exchange(ref _disposed, 1) == 0;

        /// <summary>
        /// Atomically takes the inner subscription, returning it exactly once.
        /// </summary>
        /// <returns>The inner subscription if present; otherwise <see langword="null"/>.</returns>
        [ExcludeFromCodeCoverage]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IDisposable? TakeInnerSubscription() => Interlocked.Exchange(ref _innerSubscription, null);

        /// <summary>
        /// Atomically sets the inner subscription if the slot is currently empty.
        /// </summary>
        /// <param name="subscription">The subscription to store.</param>
        /// <returns><see langword="true"/> if the slot was empty and the value was stored; otherwise <see langword="false"/>.</returns>
        [ExcludeFromCodeCoverage]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TrySetInnerSubscription(IDisposable subscription) => Interlocked.CompareExchange(ref _innerSubscription, subscription, null) == null;

        /// <summary>
        /// Disposes a newly created subscription if a concurrent OnNext or Dispose has already
        /// claimed the inner subscription slot. This race path occurs when <see cref="TrySetInnerSubscription"/>
        /// fails because another thread took the slot or disposed the parent between the subscribe call
        /// and the compare-exchange.
        /// </summary>
        /// <param name="sub">The subscription to store or dispose.</param>
        [ExcludeFromCodeCoverage]
        private void DisposeIfRace(IDisposable sub)
        {
            if (!TrySetInnerSubscription(sub))
            {
                sub.Dispose();
            }
        }

        /// <summary>
        /// Disposes both the outer and inner subscriptions during disposal.
        /// The <c>OuterSubscription?.Dispose()</c> null-conditional handles the race where
        /// <see cref="OuterSubscription"/> has not yet been set when <see cref="Dispose"/> is called.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void DisposeSubscriptions()
        {
            OuterSubscription?.Dispose();
            TakeInnerSubscription()?.Dispose();
        }

        /// <summary>
        /// Observer for the current inner observable that forwards values to the downstream observer.
        /// </summary>
        private sealed class InnerObserver : IObserver<T>
        {
            /// <summary>
            /// The parent switch subscription.
            /// </summary>
            private readonly SwitchSubscription _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="InnerObserver"/> class.
            /// </summary>
            /// <param name="parent">The parent switch subscription.</param>
            public InnerObserver(SwitchSubscription parent) => _parent = parent;

            /// <inheritdoc/>
            public void OnNext(T value)
            {
                if (Volatile.Read(ref _parent._disposed) == 0)
                {
                    _parent._observer.OnNext(value);
                }
            }

            /// <inheritdoc/>
            public void OnError(Exception error)
            {
                if (Volatile.Read(ref _parent._disposed) == 0)
                {
                    _parent._observer.OnError(error);
                }
            }

            /// <inheritdoc/>
            public void OnCompleted()
            {
                // Inner completed; do not propagate (outer decides lifetime).
            }
        }
    }
}
