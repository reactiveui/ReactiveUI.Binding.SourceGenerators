// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Dispatching;

namespace ReactiveUI.Binding.Documentation.PlatformsMaui;

/// <summary>
/// A dispatcher for the thread that creates it, standing in for the UI thread a MAUI application supplies. Work
/// dispatched from another thread waits in a queue until the owning thread calls <see cref="RunQueued"/>. It also
/// hands itself to every control that asks MAUI for a dispatcher.
/// </summary>
[System.Diagnostics.DebuggerDisplay("QueuedCount = {QueuedCount}")]
public sealed class QueueDispatcher : IDispatcher, IDispatcherProvider, IDisposable
{
    /// <summary>The work waiting for the owning thread.</summary>
    private readonly Queue<Action> _queue = new();

    /// <summary>The identifier of the owning thread.</summary>
    private readonly int _ownerThreadId = Environment.CurrentManagedThreadId;

    /// <summary>Gets a value indicating whether the calling thread is not the owning thread.</summary>
    public bool IsDispatchRequired => Environment.CurrentManagedThreadId != _ownerThreadId;

    /// <summary>Gets the number of callbacks waiting for the owning thread.</summary>
    public int QueuedCount
    {
        get
        {
            lock (_queue)
            {
                return _queue.Count;
            }
        }
    }

    /// <summary>Makes a new dispatcher the one MAUI hands to every control, until the dispatcher is disposed.</summary>
    /// <returns>The dispatcher, owned by the calling thread.</returns>
    public static QueueDispatcher Install()
    {
        QueueDispatcher dispatcher = new();
        _ = DispatcherProvider.SetCurrent(dispatcher);
        return dispatcher;
    }

    /// <inheritdoc/>
    public IDispatcher? GetForCurrentThread() => this;

    /// <inheritdoc/>
    public bool Dispatch(Action action)
    {
        lock (_queue)
        {
            _queue.Enqueue(action);
        }

        return true;
    }

    /// <inheritdoc/>
    public bool DispatchDelayed(TimeSpan delay, Action action) => throw new NotSupportedException("The examples do not use delayed dispatch.");

    /// <inheritdoc/>
    public IDispatcherTimer CreateTimer() => throw new NotSupportedException("The examples do not use timers.");

    /// <summary>Runs every callback that is waiting. Call it from the owning thread.</summary>
    /// <returns>The number of callbacks that ran.</returns>
    public int RunQueued()
    {
        var ran = 0;

        while (true)
        {
            Action? next;

            lock (_queue)
            {
                next = _queue.Count == 0 ? null : _queue.Dequeue();
            }

            if (next is null)
            {
                return ran;
            }

            next();
            ran++;
        }
    }

    /// <inheritdoc/>
    public void Dispose() => _ = DispatcherProvider.SetCurrent(null);
}
