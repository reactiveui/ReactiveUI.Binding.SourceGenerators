// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.ObservableForProperty;

/// <summary>
/// Single fused observable for one named property: optionally emits the current value on subscribe, then re-reads
/// and emits the property value on each notification from the underlying change source, applying the optional
/// distinct gate inline. Replaces an <c>Observable.Create</c> + <c>DistinctUntilChanged</c> pair with one
/// allocation-light sink.
/// </summary>
/// <typeparam name="TSender">The type of the observed object surfaced on the emitted change.</typeparam>
/// <typeparam name="TValue">The property value type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ObservableForPropertySink<TSender, TValue> : IObservable<IObservedChange<TSender, TValue>>
{
    /// <summary>
    /// The observed object surfaced on the emitted change.
    /// </summary>
    private readonly TSender _sender;

    /// <summary>
    /// The expression surfaced on the emitted change.
    /// </summary>
    private readonly Expression _expression;

    /// <summary>
    /// The underlying change-notification source (a notification re-reads the value).
    /// </summary>
    private readonly IObservable<IObservedChange<object, object?>> _notifications;

    /// <summary>
    /// Reads the current property value from the sender.
    /// </summary>
    private readonly Func<TValue> _readValue;

    /// <summary>
    /// When <see langword="true"/>, the current value is not emitted on subscribe.
    /// </summary>
    private readonly bool _skipInitial;

    /// <summary>
    /// When <see langword="true"/>, consecutive equal values are suppressed.
    /// </summary>
    private readonly bool _isDistinct;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableForPropertySink{TSender, TValue}"/> class.
    /// </summary>
    /// <param name="sender">The observed object surfaced on the emitted change.</param>
    /// <param name="expression">The expression surfaced on the emitted change.</param>
    /// <param name="notifications">The underlying change-notification source.</param>
    /// <param name="readValue">Reads the current property value from the sender.</param>
    /// <param name="skipInitial">When <see langword="true"/>, the current value is not emitted on subscribe.</param>
    /// <param name="isDistinct">When <see langword="true"/>, consecutive equal values are suppressed.</param>
    public ObservableForPropertySink(
        TSender sender,
        Expression expression,
        IObservable<IObservedChange<object, object?>> notifications,
        Func<TValue> readValue,
        bool skipInitial,
        bool isDistinct)
    {
        ArgumentExceptionHelper.ThrowIfNull(expression);
        ArgumentExceptionHelper.ThrowIfNull(notifications);
        ArgumentExceptionHelper.ThrowIfNull(readValue);
        _sender = sender;
        _expression = expression;
        _notifications = notifications;
        _readValue = readValue;
        _skipInitial = skipInitial;
        _isDistinct = isDistinct;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<IObservedChange<TSender, TValue>> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        var sink = new Sink(observer, _sender, _expression, _readValue, _isDistinct);
        return sink.Run(_notifications, _skipInitial);
    }

    /// <summary>
    /// Reads the property value on each notification and forwards it as an observed change.
    /// </summary>
    private sealed class Sink : IObserver<IObservedChange<object, object?>>
    {
        /// <summary>
        /// The observer receiving observed changes.
        /// </summary>
        private readonly IObserver<IObservedChange<TSender, TValue>> _downstream;

        /// <summary>
        /// The observed object.
        /// </summary>
        private readonly TSender _sender;

        /// <summary>
        /// The expression surfaced on the observed change.
        /// </summary>
        private readonly Expression _expression;

        /// <summary>
        /// Reads the current property value from the sender.
        /// </summary>
        private readonly Func<TValue> _readValue;

        /// <summary>
        /// Whether consecutive equal values are suppressed.
        /// </summary>
        private readonly bool _isDistinct;

        /// <summary>
        /// The last emitted value, used by the distinct gate.
        /// </summary>
        private TValue _last = default!;

        /// <summary>
        /// Whether <see cref="_last"/> holds a value yet.
        /// </summary>
        private bool _hasLast;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sink"/> class.
        /// </summary>
        /// <param name="downstream">The observer receiving observed changes.</param>
        /// <param name="sender">The observed object.</param>
        /// <param name="expression">The expression surfaced on the observed change.</param>
        /// <param name="readValue">Reads the current property value.</param>
        /// <param name="isDistinct">Whether consecutive equal values are suppressed.</param>
        public Sink(
            IObserver<IObservedChange<TSender, TValue>> downstream,
            TSender sender,
            Expression expression,
            Func<TValue> readValue,
            bool isDistinct)
        {
            _downstream = downstream;
            _sender = sender;
            _expression = expression;
            _readValue = readValue;
            _isDistinct = isDistinct;
        }

        /// <summary>
        /// Optionally emits the current value, then subscribes to the notification source.
        /// </summary>
        /// <param name="notifications">The change-notification source.</param>
        /// <param name="skipInitial">When <see langword="true"/>, the current value is not emitted on subscribe.</param>
        /// <returns>The notification-source subscription.</returns>
        public IDisposable Run(IObservable<IObservedChange<object, object?>> notifications, bool skipInitial)
        {
            if (!skipInitial)
            {
                Emit();
            }

            return notifications.Subscribe(this);
        }

        /// <inheritdoc/>
        public void OnNext(IObservedChange<object, object?> value) => Emit();

        /// <inheritdoc/>
        public void OnError(Exception error) => _downstream.OnError(error);

        /// <inheritdoc/>
        public void OnCompleted() => _downstream.OnCompleted();

        /// <summary>
        /// Reads the current property value and forwards it as an observed change, honoring the distinct gate.
        /// </summary>
        private void Emit()
        {
            TValue current;
            try
            {
                current = _readValue();
            }
            catch (Exception ex)
            {
                _downstream.OnError(ex);
                return;
            }

            if (_isDistinct && _hasLast && EqualityComparer<TValue>.Default.Equals(current, _last))
            {
                return;
            }

            _last = current;
            _hasLast = true;
            _downstream.OnNext(new ObservedChange<TSender, TValue>(_sender, _expression, current));
        }
    }
}
