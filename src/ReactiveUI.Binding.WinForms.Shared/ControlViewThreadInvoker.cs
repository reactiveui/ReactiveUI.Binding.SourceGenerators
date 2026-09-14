// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.WinForms;
#else
namespace ReactiveUI.Binding.WinForms;
#endif

/// <summary>Writes to a WinForms control on the thread that created its handle.</summary>
public sealed class ControlViewThreadInvoker : IViewThreadInvoker
{
    /// <inheritdoc/>
    public bool Claims(object target) => target is Control;

    /// <inheritdoc/>
    public bool CheckAccess(object target) =>
        // InvokeRequired is false until the control or a parent has a handle, so those writes run inline.
        !((Control)target).InvokeRequired;

    /// <inheritdoc/>
    public void Post(object target, Action<object?> callback, object? state)
    {
        ArgumentExceptionHelper.ThrowIfNull(callback);

        var control = (Control)target;
        if (!control.InvokeRequired)
        {
            callback(state);
            return;
        }

        _ = control.BeginInvoke(callback, [state]);
    }
}
