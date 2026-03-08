// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Fused property observation observable for <see cref="INotifyPropertyChanged"/> objects.
/// Collapses <c>Observable.Create + StartWith + DistinctUntilChanged</c> into a single allocation.
/// Emits the current value on subscription, then emits new values when the property changes.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PropertyObservable<T> : IObservable<T>
{
    /// <summary>
    /// The source object implementing <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    private readonly INotifyPropertyChanged _source;

    /// <summary>
    /// The name of the property to observe.
    /// </summary>
    private readonly string _propertyName;

    /// <summary>
    /// A delegate that reads the current property value from the source.
    /// </summary>
    private readonly Func<INotifyPropertyChanged, T> _getter;

    /// <summary>
    /// Whether to suppress duplicate consecutive values.
    /// </summary>
    private readonly bool _distinctUntilChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyObservable{T}"/> class.
    /// </summary>
    /// <param name="source">The object implementing <see cref="INotifyPropertyChanged"/>.</param>
    /// <param name="propertyName">The property name to observe.</param>
    /// <param name="getter">A delegate that reads the property value from the source.</param>
    /// <param name="distinctUntilChanged">Whether to suppress duplicate consecutive values.</param>
    public PropertyObservable(
        INotifyPropertyChanged source,
        string propertyName,
        Func<INotifyPropertyChanged, T> getter,
        bool distinctUntilChanged)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(propertyName);
        ArgumentExceptionHelper.ThrowIfNull(getter);
        _source = source;
        _propertyName = propertyName;
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
        /// The parent observable that owns the source and property metadata.
        /// </summary>
        private readonly PropertyObservable<T> _parent;

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
        /// Subscribes to the <see cref="INotifyPropertyChanged.PropertyChanged"/> event
        /// and emits the initial property value.
        /// </summary>
        /// <param name="parent">The parent observable.</param>
        /// <param name="observer">The downstream observer.</param>
        public Subscription(PropertyObservable<T> parent, IObserver<T> observer)
        {
            _parent = parent;
            _observer = observer;
            _comparer = EqualityComparer<T>.Default;

            parent._source.PropertyChanged += OnPropertyChanged;

            // Emit initial (StartWith) value
            var initial = parent._getter(parent._source);
            _lastValue = initial;
            _hasValue = true;
            observer.OnNext(initial);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (TrySetDisposed())
            {
                _parent._source.PropertyChanged -= OnPropertyChanged;
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
        /// Handles the <see cref="INotifyPropertyChanged.PropertyChanged"/> event
        /// and forwards the current property value to the observer if it passes the distinct-until-changed filter.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments containing the property name.</param>
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != _parent._propertyName
                && !string.IsNullOrEmpty(e.PropertyName))
            {
                return;
            }

            var observer = Volatile.Read(ref _observer);
            if (observer is null)
            {
                return;
            }

            var value = _parent._getter(_parent._source);

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
