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

/// <summary>Routes writes to a WPF <c>DispatcherObject</c> onto the thread its dispatcher owns.</summary>
public sealed class DispatcherViewThreadInvoker : IViewThreadInvoker
{
    /// <inheritdoc/>
    public bool Claims(object target) => target is DispatcherObject;

    /// <summary>Returns whether the calling thread owns the target's dispatcher.</summary>
    /// <param name="target">A <c>DispatcherObject</c>; any other type throws <see cref="InvalidCastException"/>.</param>
    /// <returns><see langword="true"/> when the calling thread may touch the target.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CheckAccess(object target) => ((DispatcherObject)target).CheckAccess();

    /// <summary>Queues <paramref name="callback"/> on the target's dispatcher at normal priority, or runs it inline when the target has no dispatcher.</summary>
    /// <param name="target">A <c>DispatcherObject</c>; any other type throws <see cref="InvalidCastException"/>.</param>
    /// <param name="callback">The callback to run.</param>
    /// <param name="state">The value passed to <paramref name="callback"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="callback"/> is null.</exception>
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
