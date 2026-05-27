// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.ObservableForProperty;

/// <summary>
/// Walks an expression member chain (<c>x.A.B.C</c>) as a single switching engine: one watcher per link, each
/// observing its link on the value produced by the previous link and re-subscribing the deeper links when an
/// intermediate value changes. Emits the leaf value as an observed change, applying skip-initial, the
/// non-null-parent filter, the cast to <typeparamref name="TValue"/>, and the optional distinct-by-value gate
/// inline — collapsing the nested <c>Select</c>+<c>Switch</c> fold plus
/// <c>Skip</c>/<c>Where</c>/<c>Select</c>/<c>DistinctUntilChanged</c> into one allocation-light sink.
/// </summary>
/// <typeparam name="TSender">The root sender type surfaced on the emitted change.</typeparam>
/// <typeparam name="TValue">The leaf value type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
[RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
public sealed class ExpressionChainSink<TSender, TValue> : IObservable<IObservedChange<TSender, TValue>>
{
    /// <summary>
    /// The root object of the chain.
    /// </summary>
    private readonly TSender? _source;

    /// <summary>
    /// The full expression surfaced on the emitted change.
    /// </summary>
    private readonly Expression? _expression;

    /// <summary>
    /// The member-access links of the chain, in order.
    /// </summary>
    private readonly Expression[] _links;

    /// <summary>
    /// Whether values are observed before they change.
    /// </summary>
    private readonly bool _beforeChange;

    /// <summary>
    /// Whether the initial value is suppressed.
    /// </summary>
    private readonly bool _skipInitial;

    /// <summary>
    /// Whether consecutive equal leaf values are suppressed.
    /// </summary>
    private readonly bool _isDistinct;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionChainSink{TSender, TValue}"/> class.
    /// </summary>
    /// <param name="source">The root object of the chain.</param>
    /// <param name="expression">The full expression surfaced on the emitted change.</param>
    /// <param name="links">The member-access links of the chain, in order.</param>
    /// <param name="beforeChange">Whether values are observed before they change.</param>
    /// <param name="skipInitial">Whether the initial value is suppressed.</param>
    /// <param name="isDistinct">Whether consecutive equal leaf values are suppressed.</param>
    public ExpressionChainSink(
        TSender? source,
        Expression? expression,
        Expression[] links,
        bool beforeChange,
        bool skipInitial,
        bool isDistinct)
    {
        ArgumentExceptionHelper.ThrowIfNull(links);
        _source = source;
        _expression = expression;
        _links = links;
        _beforeChange = beforeChange;
        _skipInitial = skipInitial;
        _isDistinct = isDistinct;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<IObservedChange<TSender, TValue>> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        var sink = new Sink(observer, _source, _expression, _links, _beforeChange, _skipInitial, _isDistinct);
        sink.Run();
        return sink;
    }

    /// <summary>
    /// The running state of one chain subscription.
    /// </summary>
    [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
    private sealed class Sink : IDisposable
    {
        /// <summary>
        /// Serializes chain mutations and emission.
        /// </summary>
        private readonly object _gate = new();

        /// <summary>
        /// The observer receiving the leaf observed changes.
        /// </summary>
        private readonly IObserver<IObservedChange<TSender, TValue>> _downstream;

        /// <summary>
        /// The root object of the chain.
        /// </summary>
        private readonly TSender? _source;

        /// <summary>
        /// The full expression surfaced on the emitted change.
        /// </summary>
        private readonly Expression? _expression;

        /// <summary>
        /// The member-access links of the chain, in order.
        /// </summary>
        private readonly Expression[] _links;

        /// <summary>
        /// Whether values are observed before they change.
        /// </summary>
        private readonly bool _beforeChange;

        /// <summary>
        /// Whether consecutive equal leaf values are suppressed.
        /// </summary>
        private readonly bool _isDistinct;

        /// <summary>
        /// The per-link watchers.
        /// </summary>
        private readonly Level[] _levels;

        /// <summary>
        /// Whether the next raw emission should be skipped (skip-initial).
        /// </summary>
        private bool _skipNext;

        /// <summary>
        /// The last emitted leaf value, used by the distinct gate.
        /// </summary>
        private TValue _last = default!;

        /// <summary>
        /// Whether <see cref="_last"/> holds a value yet.
        /// </summary>
        private bool _hasLast;

        /// <summary>
        /// Latched once this chain subscription has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sink"/> class.
        /// </summary>
        /// <param name="downstream">The observer receiving the leaf observed changes.</param>
        /// <param name="source">The root object of the chain.</param>
        /// <param name="expression">The full expression surfaced on the emitted change.</param>
        /// <param name="links">The member-access links of the chain, in order.</param>
        /// <param name="beforeChange">Whether values are observed before they change.</param>
        /// <param name="skipInitial">Whether the initial value is suppressed.</param>
        /// <param name="isDistinct">Whether consecutive equal leaf values are suppressed.</param>
        public Sink(
            IObserver<IObservedChange<TSender, TValue>> downstream,
            TSender? source,
            Expression? expression,
            Expression[] links,
            bool beforeChange,
            bool skipInitial,
            bool isDistinct)
        {
            _downstream = downstream;
            _source = source;
            _expression = expression;
            _links = links;
            _beforeChange = beforeChange;
            _isDistinct = isDistinct;
            _skipNext = skipInitial;
            _levels = new Level[links.Length];
            for (var i = 0; i < links.Length; i++)
            {
                _levels[i] = new Level(this, i, i == links.Length - 1);
            }
        }

        /// <summary>
        /// Establishes the chain from the root value.
        /// </summary>
        public void Run()
        {
            lock (_gate)
            {
                if (_links.Length == 0)
                {
                    return;
                }

                _levels[0].SetParent(_source);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (_gate)
            {
                _disposed = true;
                for (var i = 0; i < _levels.Length; i++)
                {
                    _levels[i].Dispose();
                }
            }
        }

        /// <summary>
        /// Sets the parent value of the level after <paramref name="level"/>.
        /// </summary>
        /// <param name="level">The level index that produced the value.</param>
        /// <param name="value">The value the link produced (the parent for the next level).</param>
        private void SetNextParent(int level, object? value) => _levels[level + 1].SetParent(value);

        /// <summary>
        /// Handles a leaf raw emission: applies skip-initial, the non-null-parent filter, the cast and the distinct gate.
        /// </summary>
        /// <param name="parentMissing">Whether the leaf's parent was null.</param>
        /// <param name="value">The leaf value when the parent is present.</param>
        private void Emit(bool parentMissing, object? value)
        {
            if (_skipNext)
            {
                _skipNext = false;
                return;
            }

            if (parentMissing)
            {
                return;
            }

            TValue typed;
            if (value is null)
            {
                typed = default!;
            }
            else if (value is TValue cast)
            {
                typed = cast;
            }
            else
            {
                _downstream.OnError(new InvalidCastException($"Unable to cast from {value.GetType()} to {typeof(TValue)}."));
                return;
            }

            if (_isDistinct && _hasLast && EqualityComparer<TValue>.Default.Equals(typed, _last))
            {
                return;
            }

            _last = typed;
            _hasLast = true;
            _downstream.OnNext(new ObservedChange<TSender, TValue>(_source!, _expression, typed));
        }

        /// <summary>
        /// A single chain link's watcher: re-subscribes on parent change and reads the link's value.
        /// </summary>
        [RequiresUnreferencedCode("Evaluates expression-based member chains via reflection; members may be trimmed.")]
        private sealed class Level : IDisposable
        {
            /// <summary>
            /// The owning chain sink.
            /// </summary>
            private readonly Sink _sink;

            /// <summary>
            /// This watcher's position in the chain.
            /// </summary>
            private readonly int _index;

            /// <summary>
            /// Whether this is the final link in the chain.
            /// </summary>
            private readonly bool _isLeaf;

            /// <summary>
            /// The current link-notification subscription; swapped on each re-parent.
            /// </summary>
            private readonly SerialDisposable _subscription = new();

            /// <summary>
            /// This link's value fetcher, compiled once, or <see langword="null"/> for an unsupported member.
            /// </summary>
            private readonly Func<object?, object?[]?, object?>? _getter;

            /// <summary>
            /// This link's index/argument array (non-null only for indexer links), cached once.
            /// </summary>
            private readonly object?[]? _arguments;

            /// <summary>
            /// Initializes a new instance of the <see cref="Level"/> class.
            /// </summary>
            /// <param name="sink">The owning chain sink.</param>
            /// <param name="index">This watcher's position in the chain.</param>
            /// <param name="isLeaf">Whether this is the final link in the chain.</param>
            public Level(Sink sink, int index, bool isLeaf)
            {
                _sink = sink;
                _index = index;
                _isLeaf = isLeaf;
                var member = sink._links[index].GetMemberInfo();
                _getter = member is null ? null : Reflection.GetValueFetcherForProperty(member);
                _arguments = sink._links[index].GetArgumentsArray();
            }

            /// <summary>
            /// Re-establishes this watcher on a new parent value and propagates the current value downward.
            /// </summary>
            /// <param name="parent">The object this link is read from.</param>
            public void SetParent(object? parent)
            {
                if (parent is null)
                {
                    _subscription.Disposable = null;
                    if (_isLeaf)
                    {
                        _sink.Emit(parentMissing: true, null);
                    }
                    else
                    {
                        _sink.SetNextParent(_index, null);
                    }

                    return;
                }

                var link = _sink._links[_index];

                // Kicker: propagate the current value immediately, then subscribe for updates.
                Push(ReadValue(parent));
                _subscription.Disposable = ReactiveNotifyPropertyChangedMixin
                    .NotifyForProperty(parent, link, _sink._beforeChange)
                    .Subscribe(new Observer(this));
            }

            /// <inheritdoc/>
            public void Dispose() => _subscription.Dispose();

            /// <summary>
            /// Handles a notification for this link by re-reading the value and propagating it.
            /// </summary>
            /// <param name="change">The notification (its value is read via reflection).</param>
            public void OnNotification(IObservedChange<object?, object?> change)
            {
                lock (_sink._gate)
                {
                    if (_sink._disposed)
                    {
                        return;
                    }

                    Push(ReadValue(change.Sender));
                }
            }

            /// <summary>
            /// Forwards a link-subscription error to the downstream observer.
            /// </summary>
            /// <param name="error">The error to forward.</param>
            public void ForwardError(Exception error) => _sink._downstream.OnError(error);

            /// <summary>
            /// Reads the current value of this link from a parent using the cached fetcher.
            /// </summary>
            /// <param name="parent">The object the link is read from.</param>
            /// <returns>The link's current value, or the default when the parent is null.</returns>
            private object? ReadValue(object? parent)
            {
                if (parent is null)
                {
                    return null;
                }

                return _getter is not null
                    ? _getter(parent, _arguments)
                    : new ObservedChange<object?, object?>(parent, _sink._links[_index], null).GetValueOrDefault();
            }

            /// <summary>
            /// Forwards this link's value to the next level, or emits it at the leaf.
            /// </summary>
            /// <param name="value">The value this link produced.</param>
            private void Push(object? value)
            {
                if (_isLeaf)
                {
                    _sink.Emit(parentMissing: false, value);
                }
                else
                {
                    _sink.SetNextParent(_index, value);
                }
            }

            /// <summary>
            /// Forwards a link's notifications back into the level.
            /// </summary>
            private sealed class Observer : IObserver<IObservedChange<object?, object?>>
            {
                /// <summary>
                /// The owning level.
                /// </summary>
                private readonly Level _level;

                /// <summary>
                /// Initializes a new instance of the <see cref="Observer"/> class.
                /// </summary>
                /// <param name="level">The owning level.</param>
                public Observer(Level level) => _level = level;

                /// <inheritdoc/>
                public void OnNext(IObservedChange<object?, object?> value) => _level.OnNotification(value);

                /// <inheritdoc/>
                public void OnError(Exception error) => _level.ForwardError(error);

                /// <inheritdoc/>
                public void OnCompleted()
                {
                }
            }
        }
    }
}
