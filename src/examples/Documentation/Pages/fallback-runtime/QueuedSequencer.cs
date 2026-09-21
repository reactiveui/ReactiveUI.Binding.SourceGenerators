// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.FallbackRuntime;

/// <summary>
/// A sequencer that keeps the work it is given until the example runs it, as a UI thread does between two turns of
/// its message loop. A binding that names it delivers its writes here.
/// </summary>
[System.Diagnostics.DebuggerDisplay("PendingCount = {PendingCount}")]
public sealed class QueuedSequencer : ISequencer
{
    /// <summary>The work that waits to run.</summary>
    private readonly Queue<IWorkItem> _pending = new();

    /// <inheritdoc/>
    public DateTimeOffset Now => ImmediateSequencer.Instance.Now;

    /// <inheritdoc/>
    public long Timestamp => ImmediateSequencer.Instance.Timestamp;

    /// <summary>Gets the number of work items that wait to run.</summary>
    public int PendingCount => _pending.Count;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Schedule(IWorkItem item) => _pending.Enqueue(item);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Schedule(IWorkItem item, long dueTimestamp) => Schedule(item);

    /// <summary>Runs every waiting work item on the calling thread, including work scheduled while it runs.</summary>
    /// <returns>The number of work items that ran.</returns>
    public int RunPending()
    {
        var ran = 0;
        while (_pending.TryDequeue(out var item))
        {
            item.Execute();
            ran++;
        }

        return ran;
    }
}
