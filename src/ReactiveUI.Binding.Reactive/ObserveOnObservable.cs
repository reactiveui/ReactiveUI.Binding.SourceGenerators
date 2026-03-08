// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reactive.Disposables;

namespace ReactiveUI.Binding.Reactive;

/// <summary>
/// A lightweight observable that decorates a source observable with scheduler-based observation.
/// Forwards all notifications from the source to observers on the specified scheduler.
/// </summary>
/// <typeparam name="T">The type of elements in the observable sequence.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ObserveOnObservable<T> : IObservable<T>
{
    /// <summary>
    /// The source observable to observe on the specified scheduler.
    /// </summary>
    private readonly IObservable<T> _source;

    /// <summary>
    /// The scheduler to forward notifications on.
    /// </summary>
    private readonly IScheduler _scheduler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObserveOnObservable{T}"/> class.
    /// </summary>
    /// <param name="source">The source observable to observe on the specified scheduler.</param>
    /// <param name="scheduler">The scheduler to forward notifications on.</param>
    public ObserveOnObservable(IObservable<T> source, IScheduler scheduler)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(scheduler);
        _source = source;
        _scheduler = scheduler;
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        var composite = new CompositeDisposable();
        var subscription = _source.Subscribe(new ObserveOnObserver(observer, _scheduler, composite));
        composite.Add(subscription);
        return composite;
    }

    /// <summary>
    /// Observer that forwards notifications to the downstream observer on the specified scheduler.
    /// </summary>
    private sealed class ObserveOnObserver : IObserver<T>
    {
        /// <summary>
        /// The downstream observer.
        /// </summary>
        private readonly IObserver<T> _observer;

        /// <summary>
        /// The scheduler to forward notifications on.
        /// </summary>
        private readonly IScheduler _scheduler;

        /// <summary>
        /// The composite disposable tracking scheduled work.
        /// </summary>
        private readonly CompositeDisposable _disposable;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObserveOnObserver"/> class.
        /// </summary>
        /// <param name="observer">The downstream observer.</param>
        /// <param name="scheduler">The scheduler to forward notifications on.</param>
        /// <param name="disposable">The composite disposable tracking scheduled work.</param>
        public ObserveOnObserver(IObserver<T> observer, IScheduler scheduler, CompositeDisposable disposable)
        {
            _observer = observer;
            _scheduler = scheduler;
            _disposable = disposable;
        }

        /// <inheritdoc/>
        public void OnCompleted()
        {
            var d = _scheduler.Schedule(() => _observer.OnCompleted());
            _disposable.Add(d);
        }

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
            var d = _scheduler.Schedule(() => _observer.OnError(error));
            _disposable.Add(d);
        }

        /// <inheritdoc/>
        public void OnNext(T value)
        {
            var d = _scheduler.Schedule(() => _observer.OnNext(value));
            _disposable.Add(d);
        }
    }
}
