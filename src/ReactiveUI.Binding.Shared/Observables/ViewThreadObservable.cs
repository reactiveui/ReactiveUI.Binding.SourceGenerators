// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>Delivers each notification on the thread that owns the object a binding writes to.</summary>
/// <typeparam name="T">The type of the observed values.</typeparam>
/// <param name="source">The observable feeding the write.</param>
/// <param name="target">The object the write lands on.</param>
/// <param name="invoker">The invoker for the thread that owns <paramref name="target"/>.</param>
internal sealed class ViewThreadObservable<T>(IObservable<T> source, object target, IViewThreadInvoker invoker) : IObservable<T>
{
    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        var sink = new Sink(observer, target, invoker);
        sink.Attach(source.Subscribe(sink));
        return sink;
    }

    /// <summary>The subscription that writes inline on the owning thread and holds the latest value for it otherwise.</summary>
    /// <param name="observer">The observer applying the write.</param>
    /// <param name="target">The object the write lands on.</param>
    /// <param name="invoker">The invoker for the thread that owns <paramref name="target"/>.</param>
    private sealed class Sink(IObserver<T> observer, object target, IViewThreadInvoker invoker) : IObserver<T>, IDisposable
    {
        /// <summary>Guards the waiting notifications and the scheduled flag.</summary>
        private readonly Lock _gate = new();

        /// <summary>The upstream subscription, or null once disposed.</summary>
        private IDisposable? _upstream;

        /// <summary>The latest value waiting for the owning thread.</summary>
        private T? _value;

        /// <summary>Whether <see cref="_value"/> holds a value.</summary>
        private bool _hasValue;

        /// <summary>The error waiting for the owning thread, or null.</summary>
        private Exception? _error;

        /// <summary>Whether completion is waiting for the owning thread.</summary>
        private bool _completed;

        /// <summary>Whether a drain is scheduled or running.</summary>
        private bool _scheduled;

        /// <summary>Non-zero once the subscription is disposed.</summary>
        private int _disposed;

        /// <summary>Takes ownership of the upstream subscription.</summary>
        /// <param name="upstream">The subscription to the source.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(IDisposable upstream) => Volatile.Write(ref _upstream, upstream);

        /// <inheritdoc/>
        public void OnNext(T value)
        {
            if (Admit(NotificationKind.Next, value, null))
            {
                observer.OnNext(value);
            }
        }

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
            if (Admit(NotificationKind.Error, default, error))
            {
                observer.OnError(error);
            }
        }

        /// <inheritdoc/>
        public void OnCompleted()
        {
            if (Admit(NotificationKind.Completed, default, null))
            {
                observer.OnCompleted();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            // The caller only holds this subscription once Subscribe has attached the upstream one.
            Interlocked.Exchange(ref _upstream, null)!.Dispose();
        }

        /// <summary>Decides whether a notification runs on the calling thread, and holds it for the owning thread otherwise.</summary>
        /// <param name="kind">Which notification arrived.</param>
        /// <param name="value">The value, for a next notification.</param>
        /// <param name="error">The error, for an error notification.</param>
        /// <returns><see langword="true"/> when the caller should deliver the notification now.</returns>
        private bool Admit(NotificationKind kind, T? value, Exception? error)
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                return false;
            }

            bool startDrain;
            lock (_gate)
            {
                if (!_scheduled && invoker.CheckAccess(target))
                {
                    return true;
                }

                Hold(kind, value, error);
                startDrain = !_scheduled;
                _scheduled = true;
            }

            if (startDrain)
            {
                ScheduleDrain();
            }

            return false;
        }

        /// <summary>Holds a notification until the drain runs.</summary>
        /// <param name="kind">Which notification arrived.</param>
        /// <param name="value">The value, for a next notification.</param>
        /// <param name="error">The error, for an error notification.</param>
        private void Hold(NotificationKind kind, T? value, Exception? error)
        {
            switch (kind)
            {
                case NotificationKind.Next:
                {
                    // A newer value replaces a waiting one, so an echo of an earlier write never writes an old value back.
                    _value = value;
                    _hasValue = true;
                    break;
                }

                case NotificationKind.Error:
                {
                    _error = error;
                    break;
                }

                default:
                {
                    _completed = true;
                    break;
                }
            }
        }

        /// <summary>Schedules the drain on the host's main thread when one is set, and through the invoker otherwise.</summary>
        private void ScheduleDrain()
        {
            var mainThread = BindingSchedulers.MainThread;
            if (mainThread is null)
            {
                invoker.Post(target, static state => ((Sink)state!).Drain(), this);
                return;
            }

            _ = mainThread.Schedule(
                this,
                static (_, sink) =>
                {
                    sink.Drain();
                    return EmptyDisposable.Instance;
                });
        }

        /// <summary>Delivers what is waiting until nothing is left, including anything held while a write runs.</summary>
        private void Drain()
        {
            while (true)
            {
                T? value;
                bool hasValue;
                Exception? error;
                bool completed;

                lock (_gate)
                {
                    if (!_hasValue && _error is null && !_completed)
                    {
                        _scheduled = false;
                        return;
                    }

                    value = _value;
                    hasValue = _hasValue;
                    error = _error;
                    completed = _completed;
                    _value = default;
                    _hasValue = false;
                    _error = null;
                    _completed = false;
                }

                if (Volatile.Read(ref _disposed) != 0)
                {
                    continue;
                }

                Deliver(value, hasValue, error, completed);
            }
        }

        /// <summary>Hands the waiting value, then any terminal notification, to the observer.</summary>
        /// <param name="value">The waiting value.</param>
        /// <param name="hasValue">Whether a value was waiting.</param>
        /// <param name="error">The waiting error, or null.</param>
        /// <param name="completed">Whether completion was waiting.</param>
        private void Deliver(T? value, bool hasValue, Exception? error, bool completed)
        {
            if (hasValue)
            {
                observer.OnNext(value!);
            }

            if (error is not null)
            {
                observer.OnError(error);
            }
            else if (completed)
            {
                observer.OnCompleted();
            }
        }
    }
}
