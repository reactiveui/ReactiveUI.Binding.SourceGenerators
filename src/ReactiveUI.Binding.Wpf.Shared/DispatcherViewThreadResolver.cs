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
/// <remarks>
/// WPF allows several UI threads, and every <see cref="DependencyObject"/> records the dispatcher that created
/// it. Asking the object rather than the process is what makes a second window on a second UI thread work: a
/// process-wide thread would marshal that window's writes into the first thread and throw exactly as an
/// unmarshalled write does.
/// </remarks>
public sealed class DispatcherViewThreadResolver : IViewThreadResolver
{
    /// <inheritdoc/>
    public SynchronizationContext? ContextFor(object target) =>
        target is DependencyObject dependencyObject ? new DispatcherContext(dependencyObject.Dispatcher) : null;

    /// <summary>Runs a callback on the thread one dispatcher owns.</summary>
    /// <param name="dispatcher">The dispatcher whose thread the callbacks run on.</param>
    /// <remarks>
    /// A caller already on that thread runs inline, so a view model raising on the UI thread keeps its write
    /// synchronous and pays nothing. Only a write from elsewhere is queued.
    /// </remarks>
    private sealed class DispatcherContext(Dispatcher dispatcher) : SynchronizationContext
    {
        /// <inheritdoc/>
        public override void Post(SendOrPostCallback d, object? state)
        {
            if (dispatcher.CheckAccess())
            {
                d(state);
                return;
            }

            _ = dispatcher.BeginInvoke(d, [state]);
        }

        /// <inheritdoc/>
        public override void Send(SendOrPostCallback d, object? state)
        {
            if (dispatcher.CheckAccess())
            {
                d(state);
                return;
            }

            _ = dispatcher.Invoke(d, [state]);
        }
    }
}
