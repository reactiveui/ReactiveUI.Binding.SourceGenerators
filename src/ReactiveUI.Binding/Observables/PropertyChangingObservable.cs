// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Threading;

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
    private readonly INotifyPropertyChanging _source;
    private readonly string _propertyName;
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
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
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
        private readonly PropertyChangingObservable<T> _parent;
        private IObserver<T>? _observer;

        public Subscription(PropertyChangingObservable<T> parent, IObserver<T> observer)
        {
            _parent = parent;
            _observer = observer;

            parent._source.PropertyChanging += OnPropertyChanging;

            // Emit initial (StartWith) value
            T initial = parent._getter(parent._source);
            observer.OnNext(initial);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _observer, null) != null)
            {
                _parent._source.PropertyChanging -= OnPropertyChanging;
            }
        }

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

            T value = _parent._getter(_parent._source);
            observer.OnNext(value);
        }
    }
}
