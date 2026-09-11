// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Maui;
#else
namespace ReactiveUI.Binding.Maui;
#endif

/// <summary>Names the dispatcher that owns a MAUI object, so a binding writes to it on its own thread.</summary>
/// <remarks>
/// Every <see cref="BindableObject"/> carries the dispatcher of the window it belongs to, which is what a
/// multi-window application needs: the write follows the object rather than whichever window happened to be
/// built first.
/// </remarks>
public sealed class DispatcherViewThreadResolver : IViewThreadResolver
{
    /// <inheritdoc/>
    public SynchronizationContext? ContextFor(object target) =>
        target is BindableObject { Dispatcher: { } dispatcher } ? new DispatcherContext(dispatcher) : null;

    /// <summary>Runs a callback on the thread one dispatcher owns.</summary>
    /// <param name="dispatcher">The dispatcher whose thread the callbacks run on.</param>
    /// <remarks>
    /// A caller already on that thread runs inline, so a view model raising on the UI thread keeps its write
    /// synchronous and pays nothing. Only a write from elsewhere is queued.
    /// </remarks>
    private sealed class DispatcherContext(IDispatcher dispatcher) : SynchronizationContext
    {
        /// <inheritdoc/>
        public override void Post(SendOrPostCallback d, object? state)
        {
            if (!dispatcher.IsDispatchRequired)
            {
                d(state);
                return;
            }

            _ = dispatcher.Dispatch(() => d(state));
        }

        /// <inheritdoc/>
        public override void Send(SendOrPostCallback d, object? state) => Post(d, state);
    }
}
