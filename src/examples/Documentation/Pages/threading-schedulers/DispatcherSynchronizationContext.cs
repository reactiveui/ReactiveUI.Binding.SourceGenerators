// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.ThreadingSchedulers;

/// <summary>The synchronization context of a <see cref="UiDispatcher"/>, as a UI framework installs on its UI thread.</summary>
/// <param name="dispatcher">The dispatcher that runs the posted callbacks.</param>
[System.Diagnostics.DebuggerDisplay("DispatcherSynchronizationContext")]
public sealed class DispatcherSynchronizationContext(UiDispatcher dispatcher) : SynchronizationContext
{
    /// <inheritdoc/>
    public override void Post(SendOrPostCallback d, object? state) => dispatcher.Post(() => d(state));

    /// <inheritdoc/>
    public override SynchronizationContext CreateCopy() => new DispatcherSynchronizationContext(dispatcher);
}
