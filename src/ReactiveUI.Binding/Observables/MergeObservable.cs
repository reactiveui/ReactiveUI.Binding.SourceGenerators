// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Lightweight Merge operator that combines multiple source observables into one.
/// Replacement for <c>System.Reactive.Linq.Observable.Merge</c>.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class MergeObservable<T> : IObservable<T>
{
    /// <summary>
    /// The array of source observables to merge.
    /// </summary>
    private readonly IObservable<T>[] _sources;

    /// <summary>
    /// Initializes a new instance of the <see cref="MergeObservable{T}"/> class.
    /// </summary>
    /// <param name="sources">The source observables to merge.</param>
    public MergeObservable(params IObservable<T>[] sources)
    {
        ArgumentExceptionHelper.ThrowIfNull(sources);
        _sources = sources;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        var subscriptions = new IDisposable[_sources.Length];
        var mergeObserver = new MergeObserver(observer);

        for (var i = 0; i < _sources.Length; i++)
        {
            subscriptions[i] = _sources[i].Subscribe(mergeObserver);
        }

        return new MergeSubscription(subscriptions);
    }

    /// <summary>
    /// Observer that forwards all values from any source to the downstream observer.
    /// </summary>
    private sealed class MergeObserver : IObserver<T>
    {
        /// <summary>
        /// The downstream observer.
        /// </summary>
        private readonly IObserver<T> _observer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeObserver"/> class.
        /// </summary>
        /// <param name="observer">The downstream observer.</param>
        public MergeObserver(IObserver<T> observer) => _observer = observer;

        /// <inheritdoc/>
        public void OnNext(T value) => _observer.OnNext(value);

        /// <inheritdoc/>
        public void OnError(Exception error) => _observer.OnError(error);

        /// <inheritdoc/>
        public void OnCompleted()
        {
            // Individual source completion is ignored; the merged stream stays alive
            // as long as any source is active. Full completion tracking is not needed
            // for the generated binding scenarios (event-driven, never-completing).
        }
    }

    /// <summary>
    /// Manages the lifetime of all source subscriptions.
    /// </summary>
    private sealed class MergeSubscription : IDisposable
    {
        /// <summary>
        /// The array of source subscriptions. Set to <see langword="null"/> on disposal.
        /// </summary>
        private IDisposable[]? _subscriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeSubscription"/> class.
        /// </summary>
        /// <param name="subscriptions">The source subscriptions to manage.</param>
        public MergeSubscription(IDisposable[] subscriptions) => _subscriptions = subscriptions;

        /// <inheritdoc/>
        public void Dispose()
        {
            var subs = TryTakeSubscriptions();
            if (subs is null)
            {
                return;
            }

            for (var i = 0; i < subs.Length; i++)
            {
                subs[i].Dispose();
            }
        }

        /// <summary>
        /// Atomically takes the subscriptions array, returning it exactly once.
        /// </summary>
        /// <returns>The subscriptions if this is the first call; otherwise <see langword="null"/>.</returns>
        [ExcludeFromCodeCoverage]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IDisposable[]? TryTakeSubscriptions() => Interlocked.Exchange(ref _subscriptions, null);
    }
}
