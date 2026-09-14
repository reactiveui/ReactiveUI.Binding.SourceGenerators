// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>
/// Fused property observation observable for <see cref="INotifyPropertyChanged"/> objects.
/// Collapses <c>Observable.Create + StartWith + DistinctUntilChanged</c> into a single allocation.
/// Emits the current value on subscription, then emits new values when the property changes.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
[DebuggerDisplay("Property = {_propertyName}, Source = {_source}, DistinctUntilChanged = {_distinctUntilChanged}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PropertyObservable<T> : IObservable<T>
{
    /// <summary>The source object implementing <see cref="INotifyPropertyChanged"/>.</summary>
    private readonly INotifyPropertyChanged _source;

    /// <summary>The name of the property to observe.</summary>
    private readonly string _propertyName;

    /// <summary>A delegate that reads the current property value from the source. The value may be <see langword="null"/> for reference-typed properties.</summary>
    private readonly Func<INotifyPropertyChanged, T?> _getter;

    /// <summary>Whether to suppress duplicate consecutive values.</summary>
    private readonly bool _distinctUntilChanged;

    /// <summary>Initializes a new instance of the <see cref="PropertyObservable{T}"/> class.</summary>
    /// <param name="source">The object implementing <see cref="INotifyPropertyChanged"/>.</param>
    /// <param name="propertyName">The property name to observe.</param>
    /// <param name="getter">A delegate that reads the property value from the source.</param>
    /// <param name="distinctUntilChanged">Whether to suppress duplicate consecutive values.</param>
    public PropertyObservable(
        INotifyPropertyChanged source,
        string propertyName,
        Func<INotifyPropertyChanged, T?> getter,
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

    /// <summary>Manages the event subscription for a single observer, with optional distinct-until-changed filtering.</summary>
    internal sealed class Subscription : IDisposable
    {
        /// <summary>The parent observable that owns the source and property metadata.</summary>
        private readonly PropertyObservable<T> _parent;

        /// <summary>The equality comparer used for distinct-until-changed filtering.</summary>
        private readonly EqualityComparer<T> _comparer;

        /// <summary>
        /// Counts the emits asked for and not yet served. The thread that raises it from zero serves every
        /// emit asked for while it runs, so emits never overlap and no thread waits for another.
        /// </summary>
        private int _pendingEmits;

        /// <summary>The downstream observer. Set to <see langword="null"/> on disposal.</summary>
        private IObserver<T>? _observer;

        /// <summary>
        /// The most recently emitted value, used for distinct-until-changed comparison.
        /// May be <see langword="null"/> for reference-typed properties.
        /// </summary>
        private T? _lastValue;

        /// <summary>Whether at least one value has been emitted.</summary>
        private bool _hasValue;

        /// <summary>Initializes a new instance of the <see cref="Subscription"/> class, subscribing and emitting the initial value.</summary>
        /// <param name="parent">The parent observable.</param>
        /// <param name="observer">The downstream observer.</param>
        /// <remarks>A throw from the initial emit detaches the handler before propagating, as the caller never receives a disposable.</remarks>
        public Subscription(PropertyObservable<T> parent, IObserver<T> observer)
        {
            _parent = parent;
            _observer = observer;
            _comparer = EqualityComparer<T>.Default;

            parent._source.PropertyChanged += OnPropertyChanged;

            try
            {
                EmitCurrent();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!TrySetDisposed())
            {
                return;
            }

            _parent._source.PropertyChanged -= OnPropertyChanged;
        }

        /// <summary>Atomically nulls the observer, returning whether it was previously non-null.</summary>
        /// <returns><see langword="true"/> if this is the first disposal; otherwise <see langword="false"/>.</returns>
        [ExcludeFromCodeCoverage]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TrySetDisposed() => Interlocked.Exchange(ref _observer, null) is not null;

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

            EmitCurrent();
        }

        /// <summary>Reads the current property value and forwards it downstream when the distinct gate allows.</summary>
        /// <remarks>
        /// A call made while another emit runs returns at once. The running emit reads the property again
        /// before it stops, so the value is never lost and never stale. A call from inside the downstream
        /// observer is delivered after that observer returns. A throw clears the count, so the next change
        /// still emits.
        /// </remarks>
        private void EmitCurrent()
        {
            if (Interlocked.Increment(ref _pendingEmits) != 1)
            {
                return;
            }

            var unserved = 1;
            try
            {
                do
                {
                    var observer = Volatile.Read(ref _observer);
                    if (observer is null)
                    {
                        return;
                    }

                    var value = _parent._getter(_parent._source);

                    if (!_parent._distinctUntilChanged || !_hasValue || !_comparer.Equals(value!, _lastValue!))
                    {
                        _lastValue = value;
                        _hasValue = true;
                        observer.OnNext(value!);
                    }

                    unserved = Interlocked.Add(ref _pendingEmits, -unserved);
                }
                while (unserved != 0);
            }
            catch
            {
                Volatile.Write(ref _pendingEmits, 0);
                throw;
            }
        }
    }
}
