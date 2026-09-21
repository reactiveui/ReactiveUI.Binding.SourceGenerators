// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;

namespace ReactiveUI.Binding.Documentation.Infrastructure;

/// <summary>One call waiting at a <see cref="ResponseGate"/>. Its continuation runs on the thread that releases it.</summary>
[System.Diagnostics.DebuggerDisplay("Waiting")]
public sealed class Waiter : IValueTaskSource
{
    /// <summary>The state machine that holds the continuation and the result.</summary>
    private ManualResetValueTaskSourceCore<bool> _core;

    /// <summary>Gets the task that completes when <see cref="Release"/> runs.</summary>
    /// <returns>A task tied to this waiter.</returns>
    public ValueTask WaitAsync() => new(this, _core.Version);

    /// <summary>Completes the wait and runs the continuation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Release() => _core.SetResult(true);

    /// <inheritdoc/>
    void IValueTaskSource.GetResult(short token) => _ = _core.GetResult(token);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    ValueTaskSourceStatus IValueTaskSource.GetStatus(short token) => _core.GetStatus(token);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IValueTaskSource.OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) =>
        _core.OnCompleted(continuation, state, token, flags);
}
