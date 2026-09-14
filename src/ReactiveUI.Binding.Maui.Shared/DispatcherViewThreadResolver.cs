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
public sealed class DispatcherViewThreadResolver : IViewThreadResolver
{
    /// <inheritdoc/>
    public SynchronizationContext? ContextFor(object target) =>
        target is BindableObject bindable ? new DispatcherContext(bindable) : null;

    /// <summary>Runs a callback on the thread one object's dispatcher owns.</summary>
    /// <param name="owner">The object whose dispatcher the callbacks run on.</param>
    private sealed class DispatcherContext(BindableObject owner) : SynchronizationContext
    {
        /// <inheritdoc/>
        public override void Post(SendOrPostCallback d, object? state)
        {
            // Only MAUI's own Binding dispatches; a property set directly reaches the handler on the calling thread.
            var dispatcher = owner.Dispatcher;
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
