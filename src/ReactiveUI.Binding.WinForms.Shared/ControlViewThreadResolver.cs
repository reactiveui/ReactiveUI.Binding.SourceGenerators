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
/// WinForms exposes no context per control, so the posting goes through the control itself, and the control is
/// asked on every write rather than once. A control whose handle has not been created yet owns no thread, so its
/// writes stay inline; once the handle exists, later writes go to the thread that created it. Creating the handle
/// here instead would tie the control to whichever thread made the binding.
/// </remarks>
public sealed class ControlViewThreadResolver : IViewThreadResolver
{
    /// <inheritdoc/>
    public SynchronizationContext? ContextFor(object target) =>
        target is Control control ? new ControlContext(control) : null;

    /// <summary>Runs a callback on the thread that owns one control.</summary>
    /// <param name="control">The control whose thread the callbacks run on.</param>
    /// <remarks>
    /// <see cref="Control.InvokeRequired"/> answers false while neither the control nor a parent has a handle, and
    /// for a caller already on the owning thread, so both run inline and pay nothing. Only a write from elsewhere
    /// is queued.
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
