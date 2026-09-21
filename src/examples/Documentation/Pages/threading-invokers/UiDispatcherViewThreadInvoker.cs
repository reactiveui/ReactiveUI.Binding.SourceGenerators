// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.ThreadingInvokers;

/// <summary>Tells the binding engine which thread owns a <see cref="DispatcherLabelControl"/>, and how to reach it.</summary>
[System.Diagnostics.DebuggerDisplay("UiDispatcherViewThreadInvoker")]
public sealed class UiDispatcherViewThreadInvoker : IViewThreadInvoker
{
    /// <inheritdoc/>
    public bool Claims(object target) => target is DispatcherLabelControl;

    /// <inheritdoc/>
    public bool CheckAccess(object target) => target is not DispatcherLabelControl label || label.Dispatcher.CheckAccess();

    /// <inheritdoc/>
    public void Post(object target, Action<object?> callback, object? state)
    {
        if (target is not DispatcherLabelControl label)
        {
            callback(state);
            return;
        }

        label.Dispatcher.Post(() => callback(state));
    }
}
