// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Infrastructure;

/// <summary>
/// Stands in for network or database latency without a clock. While the gate is open a call completes at once.
/// While it is held, each call waits until the example releases it, so the example decides when a response arrives.
/// A released call resumes on the thread that releases it, and it has run up to its next wait, or to its end, when
/// <see cref="ReleaseNext"/> or <see cref="ReleaseAll"/> returns.
/// </summary>
[System.Diagnostics.DebuggerDisplay("IsHeld = {IsHeld}, PendingCount = {PendingCount}")]
public sealed class ResponseGate
{
    /// <summary>The calls waiting for a response, oldest first.</summary>
    private readonly Queue<Waiter> _waiting = new();

    /// <summary>Gets a value indicating whether calls wait for <see cref="ReleaseNext"/> or <see cref="ReleaseAll"/>.</summary>
    public bool IsHeld { get; private set; }

    /// <summary>Gets the number of calls waiting for a response.</summary>
    public int PendingCount => _waiting.Count;

    /// <summary>Makes every following call wait until it is released.</summary>
    public void Hold() => IsHeld = true;

    /// <summary>Opens the gate and releases every waiting call.</summary>
    public void ReleaseAll()
    {
        IsHeld = false;

        while (_waiting.Count > 0)
        {
            _waiting.Dequeue().Release();
        }
    }

    /// <summary>Releases the oldest waiting call and keeps the gate held. It does nothing when no call is waiting.</summary>
    public void ReleaseNext()
    {
        if (_waiting.Count == 0)
        {
            return;
        }

        _waiting.Dequeue().Release();
    }

    /// <summary>Waits for the response to a call.</summary>
    /// <returns>A task that is complete when the gate is open, and otherwise completes when the call is released.</returns>
    public ValueTask WaitAsync()
    {
        if (!IsHeld)
        {
            return ValueTask.CompletedTask;
        }

        Waiter waiter = new();
        _waiting.Enqueue(waiter);
        return waiter.WaitAsync();
    }
}
