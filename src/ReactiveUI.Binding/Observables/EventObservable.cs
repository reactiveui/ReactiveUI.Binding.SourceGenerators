// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Platform-agnostic event-based observable for property observation.
/// Collapses <c>Observable.Create + StartWith + DistinctUntilChanged</c> into a single allocation.
/// Used for WPF <c>DependencyProperty</c> observation via
/// <c>DependencyPropertyDescriptor.AddValueChanged</c> and WinForms
/// <c>{PropertyName}Changed</c> event observation.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class EventObservable<T> : IObservable<T>
{
    /// <summary>
    /// Delegate that subscribes the handler to the event source.
    /// </summary>
    private readonly Action<EventHandler> _addHandler;

    /// <summary>
    /// Delegate that unsubscribes the handler from the event source.
    /// </summary>
    private readonly Action<EventHandler> _removeHandler;

    /// <summary>
    /// Delegate that reads the current property value from the source.
    /// </summary>
    private readonly Func<T> _getter;

    /// <summary>
    /// Whether to suppress duplicate consecutive values.
    /// </summary>
    private readonly bool _distinctUntilChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventObservable{T}"/> class.
    /// </summary>
    /// <param name="addHandler">A delegate that subscribes an <see cref="EventHandler"/> to the property change event.</param>
    /// <param name="removeHandler">A delegate that unsubscribes an <see cref="EventHandler"/> from the property change event.</param>
    /// <param name="getter">A delegate that reads the current property value.</param>
    /// <param name="distinctUntilChanged">Whether to suppress duplicate consecutive values.</param>
    public EventObservable(
        Action<EventHandler> addHandler,
        Action<EventHandler> removeHandler,
        Func<T> getter,
        bool distinctUntilChanged)
    {
        ArgumentExceptionHelper.ThrowIfNull(addHandler);
        ArgumentExceptionHelper.ThrowIfNull(removeHandler);
        ArgumentExceptionHelper.ThrowIfNull(getter);
        _addHandler = addHandler;
        _removeHandler = removeHandler;
        _getter = getter;
        _distinctUntilChanged = distinctUntilChanged;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return new Subscription(this, observer);
    }

    /// <summary>
    /// Manages the event subscription for a single observer, with optional distinct-until-changed filtering.
    /// </summary>
    private sealed class Subscription : IDisposable
    {
        /// <summary>
        /// The parent observable that owns the add/remove handlers and getter.
        /// </summary>
        private readonly EventObservable<T> _parent;

        /// <summary>
        /// The equality comparer used for distinct-until-changed filtering.
        /// </summary>
        private readonly IEqualityComparer<T> _comparer;

        /// <summary>
        /// The downstream observer. Set to <see langword="null"/> on disposal.
        /// </summary>
        private IObserver<T>? _observer;

        /// <summary>
        /// The most recently emitted value, used for distinct-until-changed comparison.
        /// </summary>
        private T _lastValue;

        /// <summary>
        /// Whether at least one value has been emitted.
        /// </summary>
        private bool _hasValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription"/> class.
        /// Subscribes to the event source and emits the initial property value.
        /// </summary>
        /// <param name="parent">The parent observable.</param>
        /// <param name="observer">The downstream observer.</param>
        public Subscription(EventObservable<T> parent, IObserver<T> observer)
        {
            _parent = parent;
            _observer = observer;
            _comparer = EqualityComparer<T>.Default;

            parent._addHandler(OnValueChanged);

            // Emit initial (StartWith) value
            var initial = parent._getter();
            _lastValue = initial;
            _hasValue = true;
            observer.OnNext(initial);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (TrySetDisposed())
            {
                _parent._removeHandler(OnValueChanged);
            }
        }

        /// <summary>
        /// Atomically nulls the observer, returning whether it was previously non-null.
        /// </summary>
        /// <returns><see langword="true"/> if this is the first disposal; otherwise <see langword="false"/>.</returns>
        [ExcludeFromCodeCoverage]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TrySetDisposed() => Interlocked.Exchange(ref _observer, null) != null;

        /// <summary>
        /// Handles the event and forwards the current property value to the observer
        /// if it passes the distinct-until-changed filter.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnValueChanged(object? sender, EventArgs e)
        {
            var observer = Volatile.Read(ref _observer);
            if (observer is null)
            {
                return;
            }

            var value = _parent._getter();

            if (_parent._distinctUntilChanged && _hasValue && _comparer.Equals(value, _lastValue))
            {
                return;
            }

            _lastValue = value;
            _hasValue = true;
            observer.OnNext(value);
        }
    }
}
