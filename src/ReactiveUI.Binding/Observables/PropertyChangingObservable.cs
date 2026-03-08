// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Fused property observation observable for <see cref="INotifyPropertyChanging"/> objects.
/// Collapses <c>Observable.Create + StartWith</c> into a single allocation.
/// Does not apply DistinctUntilChanged because the value has not yet changed
/// when <see cref="INotifyPropertyChanging.PropertyChanging"/> fires.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PropertyChangingObservable<T> : IObservable<T>
{
    /// <summary>
    /// The source object implementing <see cref="INotifyPropertyChanging"/>.
    /// </summary>
    private readonly INotifyPropertyChanging _source;

    /// <summary>
    /// The name of the property to observe.
    /// </summary>
    private readonly string _propertyName;

    /// <summary>
    /// A delegate that reads the current property value from the source.
    /// </summary>
    private readonly Func<INotifyPropertyChanging, T> _getter;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyChangingObservable{T}"/> class.
    /// </summary>
    /// <param name="source">The object implementing <see cref="INotifyPropertyChanging"/>.</param>
    /// <param name="propertyName">The property name to observe.</param>
    /// <param name="getter">A delegate that reads the property value from the source.</param>
    public PropertyChangingObservable(
        INotifyPropertyChanging source,
        string propertyName,
        Func<INotifyPropertyChanging, T> getter)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(propertyName);
        ArgumentExceptionHelper.ThrowIfNull(getter);
        _source = source;
        _propertyName = propertyName;
        _getter = getter;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return new Subscription(this, observer);
    }

    /// <summary>
    /// Manages the event subscription for a single observer.
    /// </summary>
    private sealed class Subscription : IDisposable
    {
        /// <summary>
        /// The parent observable that owns the source and property metadata.
        /// </summary>
        private readonly PropertyChangingObservable<T> _parent;

        /// <summary>
        /// The downstream observer. Set to <see langword="null"/> on disposal.
        /// </summary>
        private IObserver<T>? _observer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription"/> class.
        /// Subscribes to the <see cref="INotifyPropertyChanging.PropertyChanging"/> event
        /// and emits the initial property value.
        /// </summary>
        /// <param name="parent">The parent observable.</param>
        /// <param name="observer">The downstream observer.</param>
        public Subscription(PropertyChangingObservable<T> parent, IObserver<T> observer)
        {
            _parent = parent;
            _observer = observer;

            parent._source.PropertyChanging += OnPropertyChanging;

            // Emit initial (StartWith) value
            var initial = parent._getter(parent._source);
            observer.OnNext(initial);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (TrySetDisposed())
            {
                _parent._source.PropertyChanging -= OnPropertyChanging;
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
        /// Handles the <see cref="INotifyPropertyChanging.PropertyChanging"/> event
        /// and forwards the current property value to the observer.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments containing the property name.</param>
        private void OnPropertyChanging(object? sender, PropertyChangingEventArgs e)
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
            observer.OnNext(value);
        }
    }
}
