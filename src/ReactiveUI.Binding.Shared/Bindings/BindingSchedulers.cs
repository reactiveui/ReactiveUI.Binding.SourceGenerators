// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
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
    /// <summary>Gets or sets the sequencer a write from another thread is delivered on, or null to use the view's own dispatcher.</summary>
    public static ISequencer? MainThread { get; set; }

    /// <summary>Delivers writes from another thread through a synchronization context.</summary>
    /// <param name="context">The context owning the view, or null to use the view's own dispatcher.</param>
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
    /// <returns>The source itself when no invoker claims the object; otherwise the source routed onto its thread.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IObservable<T> ObserveOnViewThread<T>(IObservable<T> source, object? target)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);

        return Route(source, target, ViewThreadInvokers.ForTarget(target));
    }

    /// <summary>Routes an observable onto the thread that owns the object being written to, falling back to a known invoker.</summary>
    /// <typeparam name="T">The type of the observed values.</typeparam>
    /// <param name="source">The observable feeding a write.</param>
    /// <param name="target">The object the write lands on.</param>
    /// <param name="fallback">The invoker for the object's platform, used when no registered invoker claims it.</param>
    /// <returns>The source itself when there is no object to write to; otherwise the source routed onto its thread.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IObservable<T> ObserveOnViewThread<T>(IObservable<T> source, object? target, IViewThreadInvoker fallback)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(fallback);

        return target is null ? source : Route(source, target, ViewThreadInvokers.ForTarget(target) ?? fallback);
    }

    /// <summary>Wraps an observable in the stage that writes on the invoker's thread.</summary>
    /// <typeparam name="T">The type of the observed values.</typeparam>
    /// <param name="source">The observable feeding a write.</param>
    /// <param name="target">The object the write lands on.</param>
    /// <param name="invoker">The invoker that owns the object, or null when nothing does.</param>
    /// <returns>The source itself when no invoker owns the object; otherwise the routed source.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IObservable<T> Route<T>(IObservable<T> source, object? target, IViewThreadInvoker? invoker) =>
        invoker is null ? source : new ViewThreadObservable<T>(source, target!, invoker);
}
