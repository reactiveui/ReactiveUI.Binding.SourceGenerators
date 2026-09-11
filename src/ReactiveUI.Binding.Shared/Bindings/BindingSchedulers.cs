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
/// <remarks>
/// <para>
/// A view model is free to raise its change notifications from whatever thread did the work, and on the UI
/// frameworks a view may only be touched from the thread that owns it. A binding therefore has to move the
/// write, not the consumer: an application that had this done for it before will not have added the marshalling
/// itself, and without it an ordinary background update throws where it used to work.
/// </para>
/// <para>
/// Which thread that is belongs to the object being written, not to the process, so a platform package
/// registers an <see cref="IViewThreadResolver"/> and the write follows whichever thread owns its target.
/// <see cref="MainThread"/> is a blanket fallback a host may set for targets no resolver claims.
/// </para>
/// <para>
/// Where neither answers - a console host, a test, a platform with no thread affinity - writes are delivered
/// inline, which is what leaving both unset means and costs nothing.
/// </para>
/// </remarks>
public static class BindingSchedulers
{
    /// <summary>Gets or sets the sequencer view writes are delivered on, or null to deliver them inline.</summary>
    public static ISequencer? MainThread { get; set; }

    /// <summary>Establishes the view's thread from a synchronization context.</summary>
    /// <param name="context">The context owning the view, or null to deliver writes inline.</param>
    /// <remarks>
    /// What a platform package calls during its registration. The context has to be the one that owns the
    /// views, so the call belongs on the thread that created them - which is where an application builds its
    /// services. A null context leaves writes inline rather than sending them somewhere arbitrary, so a host
    /// with no thread affinity keeps costing nothing.
    /// </remarks>
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
    /// <returns>The source itself when no thread owns the target; otherwise the source observed on it.</returns>
    /// <remarks>
    /// <para>
    /// The target decides, not the process. WPF allows several UI threads, each owning its own windows, so a
    /// single process-wide thread would marshal a write to a window on the second one into the first and throw
    /// exactly as an unmarshalled write does. <see cref="MainThread"/> is consulted only when no registered
    /// resolver claims the target.
    /// </para>
    /// <para>
    /// A write that is already on the thread owning the target is delivered inline, so a view model raising on
    /// the UI thread keeps the write synchronous. Only a write from elsewhere waits for a turn.
    /// </para>
    /// <para>
    /// Generated bindings call this unconditionally so the decision stays here rather than being baked into
    /// each call site at compile time, where it could not know which platform the assembly would run on.
    /// </para>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IObservable<T> ObserveOnViewThread<T>(IObservable<T> source, object? target)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);

        var context = ViewThreadResolvers.ForTarget(target);
        if (context is not null)
        {
#if REACTIVE_SHIM
            return source.ObserveOn(new SynchronizationContextScheduler(context));
#else
            return source.ObserveOn(new SynchronizationContextSequencer(context));
#endif
        }

        var scheduler = MainThread;
        return scheduler is null ? source : source.ObserveOn(scheduler);
    }
}
