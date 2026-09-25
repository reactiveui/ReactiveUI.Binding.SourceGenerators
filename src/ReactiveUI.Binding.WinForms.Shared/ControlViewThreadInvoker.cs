// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.WinForms;
#else
namespace ReactiveUI.Binding.WinForms;
#endif

/// <summary>Routes writes to a WinForms <c>Control</c> onto the thread that created its handle.</summary>
public sealed class ControlViewThreadInvoker : IViewThreadInvoker
{
    /// <summary>Gets the shared instance, which generated bindings route their WinForms writes through.</summary>
    public static ControlViewThreadInvoker Instance { get; } = new();

    /// <inheritdoc/>
    public bool Claims(object target) => target is Control;

    /// <summary>Returns whether the calling thread may write to the control; also true while the control has no handle.</summary>
    /// <param name="target">A <c>Control</c>; any other type throws <see cref="InvalidCastException"/>.</param>
    /// <returns><see langword="true"/> when <c>InvokeRequired</c> is false.</returns>
    public bool CheckAccess(object target) =>
        // InvokeRequired is false until the control or a parent has a handle, so those writes run inline.
        !((Control)target).InvokeRequired;

    /// <summary>Queues <paramref name="callback"/> with <c>BeginInvoke</c>, or runs it inline when no invoke is required.</summary>
    /// <param name="target">A <c>Control</c>; any other type throws <see cref="InvalidCastException"/>.</param>
    /// <param name="callback">The callback to run.</param>
    /// <param name="state">The value passed to <paramref name="callback"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="callback"/> is null.</exception>
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
