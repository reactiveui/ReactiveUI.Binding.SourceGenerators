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
/// A lightweight observable that decorates a source observable with scheduler-based observation.
/// Forwards all notifications from the source to observers on the specified scheduler.
/// </summary>
/// <typeparam name="T">The type of elements in the observable sequence.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ObserveOnObservable<T> : IObservable<T>
{
    /// <summary>The source observable to observe on the specified scheduler.</summary>
    private readonly IObservable<T> _source;

    /// <summary>The scheduler to forward notifications on.</summary>
    private readonly ISequencer _scheduler;

    /// <summary>Initializes a new instance of the <see cref="ObserveOnObservable{T}"/> class.</summary>
    /// <param name="source">The source observable to observe on the specified scheduler.</param>
    /// <param name="scheduler">The scheduler to forward notifications on.</param>
    public ObserveOnObservable(IObservable<T> source, ISequencer scheduler)
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

        var composite = new GrowableCompositeDisposable();
        var subscription = _source.Subscribe(new ObserveOnObserver(observer, _scheduler, composite));
        composite.Add(subscription);
        return composite;
    }

    /// <summary>Observer that forwards notifications to the downstream observer on the specified scheduler.</summary>
    /// <param name="observer">The downstream observer.</param>
    /// <param name="scheduler">The scheduler to forward notifications on.</param>
    /// <param name="disposable">The composite disposable tracking scheduled work.</param>
    private sealed class ObserveOnObserver(IObserver<T> observer, ISequencer scheduler, GrowableCompositeDisposable disposable) : IObserver<T>
    {
        /// <summary>The downstream observer.</summary>
        private readonly IObserver<T> _observer = observer;

        /// <summary>The scheduler to forward notifications on.</summary>
        private readonly ISequencer _scheduler = scheduler;

        /// <summary>The composite disposable tracking scheduled work.</summary>
        private readonly GrowableCompositeDisposable _disposable = disposable;

        /// <inheritdoc/>
        public void OnCompleted()
        {
            var d = _scheduler.Schedule(_observer.OnCompleted);
            _disposable.Add(d);
        }

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
            // The state-carrying overload is the one shape both scheduler flavours share, and it keeps
            // the notification out of a closure. Its return value is the handle for any recursive work
            // the callback schedules; there is none, so it hands back the empty disposable.
            var d = _scheduler.Schedule(
                (observer: _observer, error),
                static (_, state) =>
                {
                    state.observer.OnError(state.error);
                    return EmptyDisposable.Instance;
                });
            _disposable.Add(d);
        }

        /// <inheritdoc/>
        public void OnNext(T value)
        {
            var d = _scheduler.Schedule(
                (observer: _observer, value),
                static (_, state) =>
                {
                    state.observer.OnNext(state.value);
                    return EmptyDisposable.Instance;
                });
            _disposable.Add(d);
        }
    }
}
