// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Threading;

namespace ReactiveUI.Binding.Observables;

/// <summary>
/// Lightweight Merge operator that combines multiple source observables into one.
/// Replacement for <c>System.Reactive.Linq.Observable.Merge</c>.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class MergeObservable<T> : IObservable<T>
{
    private readonly IObservable<T>[] _sources;

    /// <summary>
    /// Initializes a new instance of the <see cref="MergeObservable{T}"/> class.
    /// </summary>
    /// <param name="sources">The source observables to merge.</param>
    public MergeObservable(params IObservable<T>[] sources)
    {
        _sources = sources ?? throw new ArgumentNullException(nameof(sources));
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (observer is null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        var subscriptions = new IDisposable[_sources.Length];
        var mergeObserver = new MergeObserver(observer);

        for (int i = 0; i < _sources.Length; i++)
        {
            subscriptions[i] = _sources[i].Subscribe(mergeObserver);
        }

        return new MergeSubscription(subscriptions);
    }

    private sealed class MergeObserver : IObserver<T>
    {
        private readonly IObserver<T> _observer;

        public MergeObserver(IObserver<T> observer)
        {
            _observer = observer;
        }

        public void OnNext(T value) => _observer.OnNext(value);

        public void OnError(Exception error) => _observer.OnError(error);

        public void OnCompleted()
        {
            // Individual source completion is ignored; the merged stream stays alive
            // as long as any source is active. Full completion tracking is not needed
            // for the generated binding scenarios (event-driven, never-completing).
        }
    }

    private sealed class MergeSubscription : IDisposable
    {
        private IDisposable[]? _subscriptions;

        public MergeSubscription(IDisposable[] subscriptions)
        {
            _subscriptions = subscriptions;
        }

        public void Dispose()
        {
            var subs = Interlocked.Exchange(ref _subscriptions, null);
            if (subs is null)
            {
                return;
            }

            for (int i = 0; i < subs.Length; i++)
            {
                subs[i].Dispose();
            }
        }
    }
}
