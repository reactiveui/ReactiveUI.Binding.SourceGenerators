// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows.Threading;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Wpf;
#else
namespace ReactiveUI.Binding.Wpf;
#endif

/// <summary>Writes to a WPF object on the thread its dispatcher owns.</summary>
public sealed class DispatcherViewThreadInvoker : IViewThreadInvoker
{
    /// <inheritdoc/>
    public bool Claims(object target) => target is DispatcherObject;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CheckAccess(object target) => ((DispatcherObject)target).CheckAccess();

    /// <inheritdoc/>
    public void Post(object target, Action<object?> callback, object? state)
    {
        ArgumentExceptionHelper.ThrowIfNull(callback);

        // A frozen Freezable has no dispatcher and belongs to no thread.
        var dispatcher = ((DispatcherObject)target).Dispatcher;
        if (dispatcher is null)
        {
            callback(state);
            return;
        }

        _ = dispatcher.BeginInvoke(DispatcherPriority.Normal, callback, state);
    }
}
