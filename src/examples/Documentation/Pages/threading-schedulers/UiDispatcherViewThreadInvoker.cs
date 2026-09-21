// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Documentation.ThreadingSchedulers;

/// <summary>Tells the binding engine that a <see cref="UiDispatcher"/> owns the labels it created, and how to reach that thread.</summary>
/// <param name="dispatcher">The dispatcher of the UI thread.</param>
[System.Diagnostics.DebuggerDisplay("UiDispatcherViewThreadInvoker")]
public sealed class UiDispatcherViewThreadInvoker(UiDispatcher dispatcher) : IViewThreadInvoker
{
    /// <inheritdoc/>
    public bool Claims(object target) => target is DispatcherLabelControl label && ReferenceEquals(label.Dispatcher, dispatcher);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CheckAccess(object target) => dispatcher.CheckAccess();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Post(object target, Action<object?> callback, object? state) => dispatcher.Post(() => callback(state));
}
