// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Documentation.Infrastructure;

/// <summary>Reports progress on the calling thread, so an example sees each report in order and before the call returns.</summary>
/// <typeparam name="T">The type of the progress report.</typeparam>
/// <param name="handler">The method that receives each report.</param>
public sealed class ImmediateProgress<T>(Action<T> handler) : IProgress<T>
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Report(T value) => handler(value);
}
