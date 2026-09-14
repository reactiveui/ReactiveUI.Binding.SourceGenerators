// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;
#if REACTIVE_SHIM
using System.Reactive.Concurrency;
#endif

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Where a binding delivers its writes to the view.</summary>
public static class BindingSchedulers
{
    /// <summary>Gets or sets the sequencer every view write is delivered on, or null to let the resolvers decide.</summary>
    public static ISequencer? MainThread { get; set; }

    /// <summary>Establishes the view's thread from a synchronization context.</summary>
    /// <param name="context">The context owning the view, or null to let the resolvers decide.</param>
    public static void UseSynchronizationContext(SynchronizationContext? context)
    {
        if (context is null)
        {
            MainThread = null;
            return;
        }

#if REACTIVE_SHIM
        MainThread = new SynchronizationContextScheduler(context);
#else
        MainThread = new SynchronizationContextSequencer(context);
#endif
    }

    /// <summary>Routes an observable onto the thread that owns the object being written to.</summary>
    /// <typeparam name="T">The type of the observed values.</typeparam>
    /// <param name="source">The observable feeding a write.</param>
    /// <param name="target">The object the write lands on.</param>
    /// <returns>The source itself when nothing names a thread; otherwise the source observed on it.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IObservable<T> ObserveOnViewThread<T>(IObservable<T> source, object? target)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);

        var scheduler = MainThread;
        if (scheduler is not null)
        {
            return source.ObserveOn(scheduler);
        }

        // Resolved per target: WPF can run several UI threads, each owning its own windows.
        var context = ViewThreadResolvers.ForTarget(target);

#if REACTIVE_SHIM
        return context is null ? source : source.ObserveOn(new SynchronizationContextScheduler(context));
#else
        return context is null ? source : source.ObserveOn(new SynchronizationContextSequencer(context));
#endif
    }
}
