// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;

namespace ReactiveUI.Binding.WinForms.Tests;

/// <summary>
/// Tests for the WinForms view-thread resolver. Compiled twice: once against ReactiveUI.Binding.WinForms and
/// once, under REACTIVE_SHIM, against ReactiveUI.Binding.Reactive.WinForms, so both leaves are exercised by
/// the same assertions.
/// </summary>
public class ControlViewThreadResolverTests
{
    /// <summary>How long a callback posted to a control's thread is given to arrive.</summary>
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(30);

    /// <summary>A target the resolver does not recognise, so it is left to another resolver.</summary>
    private static readonly object UnclaimedTarget = new();

    /// <summary>A control whose handle exists is claimed by the resolver.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ContextFor_WithAControlThatHasAHandle_ClaimsIt()
    {
        using var owner = new ControlThread();

        await Assert.That(new ControlViewThreadResolver().ContextFor(owner.Control)).IsNotNull();
    }

    /// <summary>
    /// A control with no handle owns no thread yet. Creating one here would bind the control to whichever
    /// thread made the binding, so the write is left where the caller put it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ContextFor_WithAControlThatHasNoHandle_ClaimsNothing()
    {
        using var control = new Control();

        await Assert.That(new ControlViewThreadResolver().ContextFor(control)).IsNull();
    }

    /// <summary>Anything that is not a control is left to another resolver.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ContextFor_WithSomethingElse_ClaimsNothing() =>
        await Assert.That(new ControlViewThreadResolver().ContextFor(UnclaimedTarget)).IsNull();

    /// <summary>A posted callback runs on the thread that owns the control rather than the posting one.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Post_RunsOnTheThreadThatOwnsTheControl()
    {
        var postingThreadId = Environment.CurrentManagedThreadId;

        using var owner = new ControlThread();
        var context = new ControlViewThreadResolver().ContextFor(owner.Control);
        using var arrived = new ManualResetEventSlim();
        var ranOnThreadId = 0;

        context!.Post(
            _ =>
            {
                ranOnThreadId = Environment.CurrentManagedThreadId;
                arrived.Set();
            },
            null);

        await Assert.That(arrived.Wait(Patience)).IsTrue();
        await Assert.That(ranOnThreadId).IsEqualTo(owner.ThreadId);
        await Assert.That(ranOnThreadId).IsNotEqualTo(postingThreadId);
    }

    /// <summary>A sent callback runs on the owning thread and has finished by the time the send returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Send_RunsOnTheOwningThreadBeforeReturning()
    {
        using var owner = new ControlThread();
        var context = new ControlViewThreadResolver().ContextFor(owner.Control);
        var ranOnThreadId = 0;

        context!.Send(_ => ranOnThreadId = Environment.CurrentManagedThreadId, null);

        await Assert.That(ranOnThreadId).IsEqualTo(owner.ThreadId);
    }

    /// <summary>
    /// A write already on the thread that owns the control is applied inline, so an update raised on the UI
    /// thread keeps the write synchronous and a caller reading the control back sees the new value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Post_ToAControlOwnedByTheWritingThread_RunsInline()
    {
        var writingThreadId = Environment.CurrentManagedThreadId;
        using var control = new Control();

        // Reading the handle creates it on this thread, which is what gives the control its owner.
        _ = control.Handle;

        var context = new ControlViewThreadResolver().ContextFor(control);
        var ranOnThreadId = 0;

        context!.Post(_ => ranOnThreadId = Environment.CurrentManagedThreadId, null);

        await Assert.That(ranOnThreadId).IsEqualTo(writingThreadId);
    }

    /// <summary>A send from the owning thread runs inline as well.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Send_ToAControlOwnedByTheSendingThread_RunsInline()
    {
        var sendingThreadId = Environment.CurrentManagedThreadId;
        using var control = new Control();

        _ = control.Handle;

        var context = new ControlViewThreadResolver().ContextFor(control);
        var ranOnThreadId = 0;

        context!.Send(_ => ranOnThreadId = Environment.CurrentManagedThreadId, null);

        await Assert.That(ranOnThreadId).IsEqualTo(sendingThreadId);
    }

    /// <summary>A control owned by a thread that pumps messages, as a WinForms application's controls are.</summary>
    /// <remarks>
    /// The thread is handed back only once its message loop has gone idle, because a control posts and sends
    /// through that loop: handing it back at handle creation would leave a send waiting on a loop that had not
    /// started.
    /// </remarks>
    private sealed class ControlThread : IDisposable
    {
        /// <summary>Keeps the message loop running until the thread is told to stop.</summary>
        private readonly ApplicationContext _context = new();

        /// <summary>Set once the message loop is running.</summary>
        private readonly ManualResetEventSlim _pumping = new();

        /// <summary>The control created on this thread, so this thread owns it.</summary>
        private Control? _control;

        /// <summary>Initializes a new instance of the <see cref="ControlThread"/> class.</summary>
        public ControlThread()
        {
            var thread = new Thread(Run) { IsBackground = true, Name = nameof(ControlThread) };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            _ = _pumping.Wait(Patience);

            ThreadId = thread.ManagedThreadId;
        }

        /// <summary>Gets the control this thread owns.</summary>
        public Control Control => _control!;

        /// <summary>Gets the managed id of the thread that owns the control.</summary>
        public int ThreadId { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            _context.ExitThread();
            _context.Dispose();
            _pumping.Dispose();
        }

        /// <summary>Creates the control and pumps its messages until the thread is told to stop.</summary>
        private void Run()
        {
            _control = new();

            // Reading the handle creates it on this thread, which is what gives the control its owner.
            _ = _control.Handle;
            Application.Idle += SignalPumping;
            Application.Run(_context);
        }

        /// <summary>Reports that the message loop is running, which is where posts and sends land.</summary>
        /// <param name="sender">The application raising the event.</param>
        /// <param name="e">The event arguments.</param>
        private void SignalPumping(object? sender, EventArgs e)
        {
            Application.Idle -= SignalPumping;
            _pumping.Set();
        }
    }
}
