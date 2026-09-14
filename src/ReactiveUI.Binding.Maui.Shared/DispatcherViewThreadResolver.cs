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
/// <para>
/// Every <see cref="BindableObject"/> carries the dispatcher of the window it belongs to, which is what a
/// multi-window application needs: the write follows the object rather than whichever window happened to be
/// built first.
/// </para>
/// <para>
/// MAUI's own <c>Binding</c> dispatches each change it applies, but a property set directly does not: the change
/// reaches the platform handler on the thread that set it. A generated binding sets the property directly, so it
/// has to move the write itself.
/// </para>
/// </remarks>
public sealed class DispatcherViewThreadResolver : IViewThreadResolver
{
    /// <inheritdoc/>
    public SynchronizationContext? ContextFor(object target) =>
        target is BindableObject bindable ? new DispatcherContext(bindable) : null;

    /// <summary>Runs a callback on the thread one object's dispatcher owns.</summary>
    /// <param name="owner">The object whose dispatcher the callbacks run on.</param>
    /// <remarks>
    /// The object is asked for its dispatcher on every write, which is when MAUI looks one up for an object created
    /// off a dispatcher thread. A caller already on the owning thread runs inline and pays nothing. Only a write from
    /// elsewhere is queued.
    /// </remarks>
    private sealed class DispatcherContext(BindableObject owner) : SynchronizationContext
    {
        /// <inheritdoc/>
        public override void Post(SendOrPostCallback d, object? state)
        {
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
