// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Single fused observable for <see cref="INotifyPropertyChanged"/> / <see cref="INotifyPropertyChanging"/>
/// property observation. Each subscription attaches one handler that filters by property name and emits a
/// single prebuilt <see cref="IObservedChange{TSender, TValue}"/> directly — replacing a
/// <c>FromEvent + Where + Select</c> operator chain with one allocation-light class. Before-change versus
/// after-change is selected per instance, so a single type serves the whole purpose.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class NotifyPropertyChangedObservable : IObservable<IObservedChange<object, object?>>
{
    /// <summary>
    /// The source object raising the notifications.
    /// </summary>
    private readonly object _sender;

    /// <summary>
    /// The expression surfaced on the emitted observed change.
    /// </summary>
    private readonly Expression _expression;

    /// <summary>
    /// The observed property name (with the <c>[]</c> suffix already applied for indexers).
    /// </summary>
    private readonly string _expectedName;

    /// <summary>
    /// Whether to observe before-change (<see cref="INotifyPropertyChanging"/>) notifications.
    /// </summary>
    private readonly bool _beforeChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotifyPropertyChangedObservable"/> class.
    /// </summary>
    /// <param name="sender">The source object raising the notifications.</param>
    /// <param name="expression">The expression surfaced on the emitted observed change.</param>
    /// <param name="expectedName">The observed property name (with the <c>[]</c> suffix already applied for indexers).</param>
    /// <param name="beforeChanged">Whether to observe before-change notifications.</param>
    public NotifyPropertyChangedObservable(object sender, Expression expression, string expectedName, bool beforeChanged)
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(expression);
        ArgumentExceptionHelper.ThrowIfNull(expectedName);
        _sender = sender;
        _expression = expression;
        _expectedName = expectedName;
        _beforeChanged = beforeChanged;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<IObservedChange<object, object?>> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return new Subscription(_sender, _expression, _expectedName, _beforeChanged, observer);
    }

    /// <summary>
    /// Manages the event subscription for a single observer: wires the appropriate change event,
    /// filters notifications by property name, and forwards a single prebuilt observed change.
    /// </summary>
    private sealed class Subscription : IDisposable
    {
        /// <summary>
        /// The before-change source, or <see langword="null"/> when observing after-change.
        /// </summary>
        private readonly INotifyPropertyChanging? _changing;

        /// <summary>
        /// The after-change source, or <see langword="null"/> when observing before-change.
        /// </summary>
        private readonly INotifyPropertyChanged? _changed;

        /// <summary>
        /// The observed property name (an empty notified name means "all properties").
        /// </summary>
        private readonly string _expectedName;

        /// <summary>
        /// The change forwarded on every matching notification. It is constant for the subscription
        /// (fixed sender and expression, lazily-read value), so it is built once and reused.
        /// </summary>
        private readonly IObservedChange<object, object?> _change;

        /// <summary>
        /// The downstream observer. Set to <see langword="null"/> on disposal.
        /// </summary>
        private IObserver<IObservedChange<object, object?>>? _observer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription"/> class and wires the change event.
        /// </summary>
        /// <param name="sender">The source object raising the notifications.</param>
        /// <param name="expression">The expression surfaced on the emitted observed change.</param>
        /// <param name="expectedName">The observed property name.</param>
        /// <param name="beforeChanged">Whether to observe before-change notifications.</param>
        /// <param name="observer">The downstream observer.</param>
        public Subscription(
            object sender,
            Expression expression,
            string expectedName,
            bool beforeChanged,
            IObserver<IObservedChange<object, object?>> observer)
        {
            _expectedName = expectedName;
            _observer = observer;
            _change = new ObservedChange<object, object?>(sender, expression, default);

            if (beforeChanged && sender is INotifyPropertyChanging changing)
            {
                _changing = changing;
                changing.PropertyChanging += OnPropertyChanging;
            }
            else if (sender is INotifyPropertyChanged changed)
            {
                _changed = changed;
                changed.PropertyChanged += OnPropertyChanged;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _observer, null) is null)
            {
                return;
            }

            if (_changing is not null)
            {
                _changing.PropertyChanging -= OnPropertyChanging;
            }

            if (_changed is null)
            {
                return;
            }

            _changed.PropertyChanged -= OnPropertyChanged;
        }

        /// <summary>
        /// Determines whether a notified property name matches the observed property
        /// (an empty notified name means "all properties").
        /// </summary>
        /// <param name="notifiedName">The property name carried by the notification.</param>
        /// <returns><see langword="true"/> if the notification applies to the observed property.</returns>
        private bool Matches(string? notifiedName) =>
            string.IsNullOrEmpty(notifiedName)
            || string.Equals(notifiedName, _expectedName, StringComparison.InvariantCulture);

        /// <summary>
        /// Handles <see cref="INotifyPropertyChanged.PropertyChanged"/>, forwarding the change when the name matches.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The property-changed event arguments.</param>
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!Matches(e.PropertyName))
            {
                return;
            }

            Volatile.Read(ref _observer)?.OnNext(_change);
        }

        /// <summary>
        /// Handles <see cref="INotifyPropertyChanging.PropertyChanging"/>, forwarding the change when the name matches.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The property-changing event arguments.</param>
        private void OnPropertyChanging(object? sender, PropertyChangingEventArgs e)
        {
            if (!Matches(e.PropertyName))
            {
                return;
            }

            Volatile.Read(ref _observer)?.OnNext(_change);
        }
    }
}
