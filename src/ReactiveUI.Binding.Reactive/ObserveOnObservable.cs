// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reactive.Concurrency;
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
    private readonly IObservable<T> _source;
    private readonly IScheduler _scheduler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObserveOnObservable{T}"/> class.
    /// </summary>
    /// <param name="source">The source observable to observe on the specified scheduler.</param>
    /// <param name="scheduler">The scheduler to forward notifications on.</param>
    public ObserveOnObservable(IObservable<T> source, IScheduler scheduler)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (observer is null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        var composite = new CompositeDisposable();
        var subscription = _source.Subscribe(new ObserveOnObserver(observer, _scheduler, composite));
        composite.Add(subscription);
        return composite;
    }

    private sealed class ObserveOnObserver : IObserver<T>
    {
        private readonly IObserver<T> _observer;
        private readonly IScheduler _scheduler;
        private readonly CompositeDisposable _disposable;

        public ObserveOnObserver(IObserver<T> observer, IScheduler scheduler, CompositeDisposable disposable)
        {
            _observer = observer;
            _scheduler = scheduler;
            _disposable = disposable;
        }

        public void OnCompleted()
        {
            var d = _scheduler.Schedule(() => _observer.OnCompleted());
            _disposable.Add(d);
        }

        public void OnError(Exception error)
        {
            var d = _scheduler.Schedule(() => _observer.OnError(error));
            _disposable.Add(d);
        }

        public void OnNext(T value)
        {
            var d = _scheduler.Schedule(() => _observer.OnNext(value));
            _disposable.Add(d);
        }
    }
}
