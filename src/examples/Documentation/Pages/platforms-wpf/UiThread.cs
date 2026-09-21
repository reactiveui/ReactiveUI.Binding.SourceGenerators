// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ReactiveUI.Binding.Documentation.PlatformsWpf;

/// <summary>Runs work on the threads a WPF screen needs: an owning thread for the controls and worker threads that write to them.</summary>
public static class UiThread
{
    /// <summary>Runs work on a new single-threaded apartment thread, which becomes the owning thread of every control the work creates.</summary>
    /// <param name="work">The work to run.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RunOwning(Action work) => RunOn(ApartmentState.STA, work);

    /// <summary>Runs work on a worker thread and waits for it to finish.</summary>
    /// <param name="work">The work to run.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RunOnWorker(Action work) => RunOn(ApartmentState.MTA, work);

    /// <summary>Runs every operation the dispatcher has queued at normal priority.</summary>
    /// <param name="dispatcher">The dispatcher of the owning thread.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RunQueued(Dispatcher dispatcher) => dispatcher.Invoke(static () => { }, DispatcherPriority.Background);

    /// <summary>Presses a button as an automation client would and runs the queued work the press starts.</summary>
    /// <param name="button">The button to press.</param>
    public static void Press(Button button)
    {
        ButtonAutomationPeer peer = new(button);
        var invoke = (IInvokeProvider)peer.GetPattern(PatternInterface.Invoke);

        invoke.Invoke();
        RunQueued(button.Dispatcher);
    }

    /// <summary>Runs work on a new thread, waits for it and rethrows what it threw.</summary>
    /// <param name="apartment">The apartment state of the thread.</param>
    /// <param name="work">The work to run.</param>
    private static void RunOn(ApartmentState apartment, Action work)
    {
        ExceptionDispatchInfo? failure = null;

        Thread thread = new(() =>
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            try
            {
                work();
            }
            catch (Exception ex)
            {
                failure = ExceptionDispatchInfo.Capture(ex);
            }
        });
        thread.SetApartmentState(apartment);
        thread.Start();
        thread.Join();

        failure?.Throw();
    }
}
