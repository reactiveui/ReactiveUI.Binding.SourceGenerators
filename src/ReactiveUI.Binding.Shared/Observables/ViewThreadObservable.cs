// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;

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

    /// <summary>One notification waiting for the owning thread.</summary>
    /// <param name="kind">Which of the three notifications this is.</param>
    /// <param name="value">The value, for a next notification.</param>
    /// <param name="error">The error, for an error notification.</param>
    private readonly struct Notification(NotificationKind kind, T? value, Exception? error)
    {
        /// <summary>Gets which of the three notifications this is.</summary>
        public NotificationKind Kind { get; } = kind;

        /// <summary>Gets the value, for a next notification.</summary>
        public T? Value { get; } = value;

        /// <summary>Gets the error, for an error notification.</summary>
        public Exception? Error { get; } = error;
    }

    /// <summary>The subscription that writes inline on the owning thread and queues everything else.</summary>
    /// <param name="observer">The observer applying the write.</param>
    /// <param name="target">The object the write lands on.</param>
    /// <param name="invoker">The invoker for the thread that owns <paramref name="target"/>.</param>
    private sealed class Sink(IObserver<T> observer, object target, IViewThreadInvoker invoker) : IObserver<T>, IDisposable
    {
        /// <summary>The notifications waiting for the owning thread, created on the first write that has to wait.</summary>
        private ConcurrentQueue<Notification>? _queue;

        /// <summary>The upstream subscription, or null once disposed.</summary>
        private IDisposable? _upstream;

        /// <summary>How many notifications are queued and not yet delivered.</summary>
        private int _pending;

        /// <summary>Non-zero once the subscription is disposed.</summary>
        private int _disposed;

        /// <summary>Takes ownership of the upstream subscription.</summary>
        /// <param name="upstream">The subscription to the source.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(IDisposable upstream) => Volatile.Write(ref _upstream, upstream);

        /// <inheritdoc/>
        public void OnNext(T value)
        {
            if (TryDeliverInline())
            {
                observer.OnNext(value);
                return;
            }

            Enqueue(new(NotificationKind.Next, value, null));
        }

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
            if (TryDeliverInline())
            {
                observer.OnError(error);
                return;
            }

            Enqueue(new(NotificationKind.Error, default, error));
        }

        /// <inheritdoc/>
        public void OnCompleted()
        {
            if (TryDeliverInline())
            {
                observer.OnCompleted();
                return;
            }

            Enqueue(new(NotificationKind.Completed, default, null));
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

        /// <summary>Delivers the queued notifications in order, on whichever thread the invoker or main thread runs this.</summary>
        private void Drain()
        {
            var queue = Volatile.Read(ref _queue)!;

            do
            {
                // The pending count is only raised after an enqueue, so there is always a notification to take.
                _ = queue.TryDequeue(out var notification);

                if (Volatile.Read(ref _disposed) == 0)
                {
                    Deliver(notification);
                }
            }
            while (Interlocked.Decrement(ref _pending) != 0);
        }

        /// <summary>Determines whether a notification can be delivered on the calling thread now.</summary>
        /// <returns><see langword="false"/> when disposed, when earlier notifications are still queued, or when the caller is not on the owning thread.</returns>
        private bool TryDeliverInline() =>
            Volatile.Read(ref _disposed) == 0
            && Volatile.Read(ref _pending) == 0
            && invoker.CheckAccess(target);

        /// <summary>Queues a notification and, when nothing is draining, schedules a drain.</summary>
        /// <param name="notification">The notification to queue.</param>
        private void Enqueue(in Notification notification)
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                return;
            }

            var queue = Volatile.Read(ref _queue);
            if (queue is null)
            {
                // The source delivers one notification at a time, so only this thread ever creates the queue.
                queue = new();
                Volatile.Write(ref _queue, queue);
            }

            queue.Enqueue(notification);

            if (Interlocked.Increment(ref _pending) != 1)
            {
                return;
            }

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

        /// <summary>Hands one notification to the observer.</summary>
        /// <param name="notification">The notification to deliver.</param>
        private void Deliver(in Notification notification)
        {
            switch (notification.Kind)
            {
                case NotificationKind.Next:
                {
                    observer.OnNext(notification.Value!);
                    break;
                }

                case NotificationKind.Error:
                {
                    observer.OnError(notification.Error!);
                    break;
                }

                default:
                {
                    observer.OnCompleted();
                    break;
                }
            }
        }
    }
}
