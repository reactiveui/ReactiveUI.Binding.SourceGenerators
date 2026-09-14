// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Threading;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Wpf;
#else
namespace ReactiveUI.Binding.Wpf;
#endif

/// <summary>Names the dispatcher that owns a WPF object, so a binding writes to it on its own thread.</summary>
public sealed class DispatcherViewThreadResolver : IViewThreadResolver
{
    /// <inheritdoc/>
    public SynchronizationContext? ContextFor(object target) =>
        target is DependencyObject dependencyObject ? new DispatcherContext(dependencyObject) : null;

    /// <summary>Runs a callback on the thread one object's dispatcher owns.</summary>
    /// <param name="owner">The object whose dispatcher the callbacks run on.</param>
    private sealed class DispatcherContext(DependencyObject owner) : SynchronizationContext
    {
        /// <inheritdoc/>
        public override void Post(SendOrPostCallback d, object? state)
        {
            // A frozen Freezable has no dispatcher and belongs to no thread.
            var dispatcher = owner.Dispatcher;
            if (dispatcher is null || dispatcher.CheckAccess())
            {
                d(state);
                return;
            }

            _ = dispatcher.BeginInvoke(d, [state]);
        }

        /// <inheritdoc/>
        public override void Send(SendOrPostCallback d, object? state)
        {
            var dispatcher = owner.Dispatcher;
            if (dispatcher is null || dispatcher.CheckAccess())
            {
                d(state);
                return;
            }

            _ = dispatcher.Invoke(d, [state]);
        }
    }
}
