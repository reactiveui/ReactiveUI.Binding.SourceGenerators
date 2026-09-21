// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.ThreadingInvokers;

/// <summary>
/// The message queue of a single-thread UI. The thread that creates the dispatcher owns it. Other threads post
/// callbacks, and the owner runs them when it calls <see cref="RunPending"/>, as a UI framework does in its message loop.
/// </summary>
[System.Diagnostics.DebuggerDisplay("PendingCount = {PendingCount}")]
public sealed class UiDispatcher
{
    /// <summary>The message shown when another thread touches an object the UI thread owns.</summary>
    private const string WrongThreadMessage = "The calling thread cannot access this object because a different thread owns it.";

    /// <summary>Guards the queue.</summary>
    private readonly Lock _gate = new();

    /// <summary>The callbacks that wait for the owning thread.</summary>
    private readonly Queue<Action> _pending = new();

    /// <summary>The managed identifier of the owning thread.</summary>
    private readonly int _ownerThreadId = Environment.CurrentManagedThreadId;

    /// <summary>Gets the number of callbacks that wait for the owning thread.</summary>
    public int PendingCount
    {
        get
        {
            lock (_gate)
            {
                return _pending.Count;
            }
        }
    }

    /// <summary>Determines whether the calling thread is the owning thread.</summary>
    /// <returns><see langword="true"/> on the owning thread; otherwise <see langword="false"/>.</returns>
    public bool CheckAccess() => Environment.CurrentManagedThreadId == _ownerThreadId;

    /// <summary>Requires the calling thread to be the owning thread.</summary>
    /// <exception cref="InvalidOperationException">The calling thread is not the owning thread.</exception>
    public void VerifyAccess()
    {
        if (!CheckAccess())
        {
            throw new InvalidOperationException(WrongThreadMessage);
        }
    }

    /// <summary>Queues a callback for the owning thread.</summary>
    /// <param name="callback">The callback to run on the owning thread.</param>
    public void Post(Action callback)
    {
        lock (_gate)
        {
            _pending.Enqueue(callback);
        }
    }

    /// <summary>Runs every queued callback on the owning thread, including callbacks queued while it runs.</summary>
    /// <returns>The number of callbacks that ran.</returns>
    public int RunPending()
    {
        VerifyAccess();

        var ran = 0;
        while (TryDequeue(out var callback))
        {
            callback();
            ran++;
        }

        return ran;
    }

    /// <summary>Takes the next callback off the queue.</summary>
    /// <param name="callback">The callback, when the queue is not empty.</param>
    /// <returns><see langword="true"/> when a callback was waiting.</returns>
    private bool TryDequeue(out Action callback)
    {
        lock (_gate)
        {
            return _pending.TryDequeue(out callback!);
        }
    }
}
