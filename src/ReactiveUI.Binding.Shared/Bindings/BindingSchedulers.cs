// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

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
/// A platform package sets <see cref="MainThread"/> during its registration. Where none is set - a console host,
/// a test, a platform with no thread affinity - writes are delivered inline, which is what leaving the value
/// null means and costs nothing.
/// </para>
/// </remarks>
public static class BindingSchedulers
{
    /// <summary>Gets or sets the sequencer view writes are delivered on, or null to deliver them inline.</summary>
    public static ISequencer? MainThread { get; set; }

    /// <summary>Routes an observable onto the view's thread, when one has been established.</summary>
    /// <typeparam name="T">The type of the observed values.</typeparam>
    /// <param name="source">The observable feeding a write to the view.</param>
    /// <returns>The source itself when no sequencer is set; otherwise the source observed on it.</returns>
    /// <remarks>
    /// Generated bindings call this unconditionally so the decision stays here rather than being baked into
    /// each call site at compile time, where it could not know which platform the assembly would run on.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IObservable<T> ObserveOnMainThread<T>(IObservable<T> source)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);

        var scheduler = MainThread;
        return scheduler is null ? source : source.ObserveOn(scheduler);
    }
}
