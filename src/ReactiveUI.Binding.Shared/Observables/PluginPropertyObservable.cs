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
/// Observes one property through a registered <see cref="ICreatesObservableForProperty"/> that outranks the
/// mechanism the generator selected, reading the value through a generated accessor.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
/// <remarks>
/// A registration wins the observation but not the read. The plugin says <em>when</em> the property changed;
/// the value is then taken with the accessor the generator emitted for that property, which is the same
/// accessor the non-overridden path uses. Asking the notification for its value instead would walk the
/// expression by reflection - the thing the generated path exists to avoid - and would make every consumer
/// that publishes ahead-of-time carry the expression engine for a branch most of them never take.
/// </remarks>
[DebuggerDisplay("Property = {_propertyName}, Source = {_source}, BeforeChange = {_beforeChange}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PluginPropertyObservable<T> : IObservable<T>
{
    /// <summary>The registration that outranked the generated mechanism.</summary>
    private readonly ICreatesObservableForProperty _plugin;

    /// <summary>The object the property is read from.</summary>
    private readonly object _source;

    /// <summary>The property as the call site named it, handed to the plugin unchanged.</summary>
    private readonly Expression _expression;

    /// <summary>The name of the property being observed.</summary>
    private readonly string _propertyName;

    /// <summary>Reads the current property value from the source.</summary>
    private readonly Func<object, T?> _getter;

    /// <summary>Whether before-change notifications are being observed.</summary>
    private readonly bool _beforeChange;

    /// <summary>Whether to suppress duplicate consecutive values.</summary>
    private readonly bool _distinctUntilChanged;

    /// <summary>Initializes a new instance of the <see cref="PluginPropertyObservable{T}"/> class.</summary>
    /// <param name="plugin">The registration that outranked the generated mechanism.</param>
    /// <param name="source">The object the property is read from.</param>
    /// <param name="expression">The property as the call site named it.</param>
    /// <param name="propertyName">The name of the property being observed.</param>
    /// <param name="getter">Reads the current property value from the source.</param>
    /// <param name="beforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="distinctUntilChanged">Whether to suppress duplicate consecutive values.</param>
    public PluginPropertyObservable(
        ICreatesObservableForProperty plugin,
        object source,
        Expression expression,
        string propertyName,
        Func<object, T?> getter,
        bool beforeChange,
        bool distinctUntilChanged)
    {
        ArgumentExceptionHelper.ThrowIfNull(plugin);
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(expression);
        ArgumentExceptionHelper.ThrowIfNull(propertyName);
        ArgumentExceptionHelper.ThrowIfNull(getter);
        _plugin = plugin;
        _source = source;
        _expression = expression;
        _propertyName = propertyName;
        _getter = getter;
        _beforeChange = beforeChange;
        _distinctUntilChanged = distinctUntilChanged;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return new Subscription(this, observer);
    }

    /// <summary>Relays one observer's subscription to the plugin, with the distinct gate applied inline.</summary>
    internal sealed class Subscription : IDisposable, IObserver<IObservedChange<object, object?>>
    {
        /// <summary>The parent observable that owns the source and property metadata.</summary>
        private readonly PluginPropertyObservable<T> _parent;

        /// <summary>The equality comparer used for distinct-until-changed filtering.</summary>
        private readonly EqualityComparer<T> _comparer;

        /// <summary>
        /// Serializes the initial emit with notifications arriving on other threads, so the handler always
        /// sees a consistent <see cref="_hasValue"/> and <see cref="_lastValue"/> pair whatever the timing.
        /// </summary>
        private readonly Lock _gate = new();

        /// <summary>The plugin's own subscription, dropped when this one is.</summary>
        private readonly IDisposable _inner;

        /// <summary>The downstream observer. Set to <see langword="null"/> on disposal.</summary>
        private IObserver<T>? _observer;

        /// <summary>The most recently emitted value, used for distinct-until-changed comparison.</summary>
        private T? _lastValue;

        /// <summary>Whether at least one value has been emitted.</summary>
        private bool _hasValue;

        /// <summary>Initializes a new instance of the <see cref="Subscription"/> class, subscribing and emitting the initial value.</summary>
        /// <param name="parent">The parent observable.</param>
        /// <param name="observer">The downstream observer.</param>
        public Subscription(PluginPropertyObservable<T> parent, IObserver<T> observer)
        {
            _parent = parent;
            _observer = observer;
            _comparer = EqualityComparer<T>.Default;

            _inner = parent._plugin.GetNotificationForProperty(
                parent._source,
                parent._expression,
                parent._propertyName,
                parent._beforeChange,
                false).Subscribe(this);

            EmitCurrent();
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(IObservedChange<object, object?> value) => EmitCurrent();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnError(Exception error) => Volatile.Read(ref _observer)?.OnError(error);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted() => Volatile.Read(ref _observer)?.OnCompleted();

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _observer, null) is null)
            {
                return;
            }

            _inner.Dispose();
        }

        /// <summary>
        /// Reads the current property value under <see cref="_gate"/> and forwards it downstream when the
        /// distinct gate allows, so the initial emit and a concurrent notification cannot interleave on the
        /// observer or publish a duplicate when both see the same value.
        /// </summary>
        private void EmitCurrent()
        {
            lock (_gate)
            {
                var observer = Volatile.Read(ref _observer);
                if (observer is null)
                {
                    return;
                }

                var value = _parent._getter(_parent._source);

                if (_parent._distinctUntilChanged && _hasValue && _comparer.Equals(value!, _lastValue!))
                {
                    return;
                }

                _lastValue = value;
                _hasValue = true;
                observer.OnNext(value!);
            }
        }
    }
}
