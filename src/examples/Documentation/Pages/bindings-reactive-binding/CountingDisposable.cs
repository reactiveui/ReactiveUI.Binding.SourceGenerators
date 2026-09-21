// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.BindingsReactiveBinding;

/// <summary>A subscription that counts how often it is disposed.</summary>
[System.Diagnostics.DebuggerDisplay("DisposeCount = {DisposeCount}")]
public sealed class CountingDisposable : IDisposable
{
    /// <summary>Gets the number of times <see cref="Dispose"/> ran.</summary>
    public int DisposeCount { get; private set; }

    /// <inheritdoc/>
    public void Dispose() => DisposeCount++;
}
