// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.WinForms;
#else
namespace ReactiveUI.Binding.WinForms;
#endif

/// <summary>Names the thread that owns a WinForms control, so a binding writes to it on its own thread.</summary>
/// <remarks>
/// WinForms exposes no context per control, so the posting goes through the control itself. A control whose
/// handle has not been created yet owns no thread to post to, and asking it to make one here would create the
/// handle on whichever thread the binding was made on, so those writes are left inline.
/// </remarks>
public sealed class ControlViewThreadResolver : IViewThreadResolver
{
    /// <inheritdoc/>
    public SynchronizationContext? ContextFor(object target) =>
        target is Control { IsHandleCreated: true } control ? new ControlContext(control) : null;

    /// <summary>Runs a callback on the thread that owns one control.</summary>
    /// <param name="control">The control whose thread the callbacks run on.</param>
    /// <remarks>
    /// A caller already on that thread runs inline, so a view model raising on the UI thread keeps its write
    /// synchronous and pays nothing. Only a write from elsewhere is queued.
    /// </remarks>
    private sealed class ControlContext(Control control) : SynchronizationContext
    {
        /// <inheritdoc/>
        public override void Post(SendOrPostCallback d, object? state)
        {
            if (!control.InvokeRequired)
            {
                d(state);
                return;
            }

            _ = control.BeginInvoke(d, [state]);
        }

        /// <inheritdoc/>
        public override void Send(SendOrPostCallback d, object? state)
        {
            if (!control.InvokeRequired)
            {
                d(state);
                return;
            }

            _ = control.Invoke(d, [state]);
        }
    }
}
