// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.ThreadingSchedulers;

/// <summary>
/// A sequencer that keeps the work it is given until the example runs it, as a host's main-thread scheduler does
/// between two turns of its message loop.
/// </summary>
/// <param name="clock">The clock the sequencer reports.</param>
[System.Diagnostics.DebuggerDisplay("PendingCount = {PendingCount}")]
public sealed class ManualSequencer(ManualClock clock) : ISequencer
{
    /// <summary>Guards the queue.</summary>
    private readonly Lock _gate = new();

    /// <summary>The work that waits to run.</summary>
    private readonly Queue<IWorkItem> _pending = new();

    /// <inheritdoc/>
    public DateTimeOffset Now => clock.GetUtcNow();

    /// <inheritdoc/>
    public long Timestamp => 0;

    /// <summary>Gets the number of work items that wait to run.</summary>
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

    /// <inheritdoc/>
    public void Schedule(IWorkItem item)
    {
        lock (_gate)
        {
            _pending.Enqueue(item);
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Schedule(IWorkItem item, long dueTimestamp) => Schedule(item);

    /// <summary>Runs every waiting work item on the calling thread, including work scheduled while it runs.</summary>
    /// <returns>The number of work items that ran.</returns>
    public int RunPending()
    {
        var ran = 0;
        while (TryDequeue(out var item))
        {
            item.Execute();
            ran++;
        }

        return ran;
    }

    /// <summary>Takes the next work item off the queue.</summary>
    /// <param name="item">The work item, when the queue is not empty.</param>
    /// <returns><see langword="true"/> when a work item was waiting.</returns>
    private bool TryDequeue(out IWorkItem item)
    {
        lock (_gate)
        {
            return _pending.TryDequeue(out item!);
        }
    }
}
