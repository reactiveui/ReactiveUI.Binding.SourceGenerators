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
public sealed class ControlViewThreadResolver : IViewThreadResolver
{
    /// <inheritdoc/>
    public SynchronizationContext? ContextFor(object target) =>
        target is Control control ? new ControlContext(control) : null;

    /// <summary>Runs a callback on the thread that owns one control.</summary>
    /// <param name="control">The control whose thread the callbacks run on.</param>
    private sealed class ControlContext(Control control) : SynchronizationContext
    {
        /// <inheritdoc/>
        public override void Post(SendOrPostCallback d, object? state)
        {
            // InvokeRequired is false until the control or a parent has a handle, so those writes run inline.
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
