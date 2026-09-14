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
/// frameworks a view may only be touched from the thread that owns it. A binding therefore moves the write, not
/// the consumer, and every binding API does so whether its call site was generated or resolved by reflection.
/// </para>
/// <para>
/// Which thread that is belongs to the object being written, not to the process, so a platform package
/// registers an <see cref="IViewThreadResolver"/> and the write follows whichever thread owns its target.
/// <see cref="MainThread"/> takes the choice over when a host sets it.
/// </para>
/// <para>
/// Where neither answers - a console host, a test, a platform with no thread affinity - writes are delivered
/// inline, which is what leaving both unset means and costs nothing.
/// </para>
/// </remarks>
public static class BindingSchedulers
{
    /// <summary>Gets or sets the sequencer every view write is delivered on, or null to let the resolvers decide.</summary>
    /// <remarks>
    /// Setting it hands the choice to the host outright, ahead of any registered resolver: an adapter delivers
    /// through its own scheduler, and a test substitutes one of its own. Whether a write already on the right
    /// thread still runs inline is the sequencer's decision.
    /// </remarks>
    public static ISequencer? MainThread { get; set; }

    /// <summary>Establishes the view's thread from a synchronization context.</summary>
    /// <param name="context">The context owning the view, or null to let the resolvers decide.</param>
    /// <remarks>
    /// The context has to be the one that owns the views, so the call belongs on the thread that created them.
    /// A null context clears <see cref="MainThread"/> rather than sending writes somewhere arbitrary.
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
    /// <returns>The source itself when nothing names a thread; otherwise the source observed on it.</returns>
    /// <remarks>
    /// <para>
    /// A host that set <see cref="MainThread"/> decides first. Otherwise the target decides, not the process:
    /// WPF allows several UI threads, each owning its own windows, so a single process-wide thread would marshal
    /// a write to a window on the second one into the first and throw exactly as an unmarshalled write does.
    /// </para>
    /// <para>
    /// A resolver's context asks its object on every write rather than once, so an object that has no owning
    /// thread yet - a WinForms control whose handle is not created - is written inline until it has one, and on
    /// that thread afterwards. A write already on the owning thread is delivered inline.
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

        var scheduler = MainThread;
        if (scheduler is not null)
        {
            return source.ObserveOn(scheduler);
        }

        var context = ViewThreadResolvers.ForTarget(target);

#if REACTIVE_SHIM
        return context is null ? source : source.ObserveOn(new SynchronizationContextScheduler(context));
#else
        return context is null ? source : source.ObserveOn(new SynchronizationContextSequencer(context));
#endif
    }
}
