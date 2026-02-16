// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

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
    private readonly INotifyPropertyChanged _source;
    private readonly string _propertyName;
    private readonly Func<INotifyPropertyChanged, T> _getter;
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
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
        _distinctUntilChanged = distinctUntilChanged;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (observer is null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        return new Subscription(this, observer);
    }

    private sealed class Subscription : IDisposable
    {
        private readonly PropertyObservable<T> _parent;
        private readonly IEqualityComparer<T> _comparer;
        private IObserver<T>? _observer;
        private T _lastValue;
        private bool _hasValue;

        public Subscription(PropertyObservable<T> parent, IObserver<T> observer)
        {
            _parent = parent;
            _observer = observer;
            _comparer = EqualityComparer<T>.Default;

            parent._source.PropertyChanged += OnPropertyChanged;

            // Emit initial (StartWith) value
            T initial = parent._getter(parent._source);
            _lastValue = initial;
            _hasValue = true;
            observer.OnNext(initial);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _observer, null) != null)
            {
                _parent._source.PropertyChanged -= OnPropertyChanged;
            }
        }

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

            T value = _parent._getter(_parent._source);

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
