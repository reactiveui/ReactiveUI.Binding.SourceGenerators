// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace ReactiveUI.Binding.Wpf.Tests;

/// <summary>
/// Tests for the WPF view-thread resolver. Compiled twice: once against ReactiveUI.Binding.Wpf and once,
/// under REACTIVE_SHIM, against ReactiveUI.Binding.Reactive.Wpf, so both leaves are exercised by the same
/// assertions.
/// </summary>
public class DispatcherViewThreadResolverTests
{
    /// <summary>How long a callback posted to another UI thread is given to arrive.</summary>
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(30);

    /// <summary>A target the resolver does not recognise, so it is left to another resolver.</summary>
    private static readonly object UnclaimedTarget = new();

    /// <summary>A dependency object is claimed by the resolver.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ContextFor_WithADependencyObject_ClaimsIt() =>
        await Assert.That(new DispatcherViewThreadResolver().ContextFor(new Fixture())).IsNotNull();

    /// <summary>Anything that is not a dependency object is left to another resolver.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ContextFor_WithSomethingElse_ClaimsNothing() =>
        await Assert.That(new DispatcherViewThreadResolver().ContextFor(UnclaimedTarget)).IsNull();

    /// <summary>
    /// An object owned by a second UI thread is written on that thread, not on the one the write came from.
    /// This is the case a process-wide thread gets wrong: it would marshal the write into the first UI thread,
    /// where touching this object throws exactly as an unmarshalled write does.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ContextFor_WithAnObjectOwnedByASecondUiThread_PostsToThatThread()
    {
        var writingThreadId = Environment.CurrentManagedThreadId;

        using var owner = new UiThread();
        var target = owner.Invoke(static () => new Fixture());

        var context = new DispatcherViewThreadResolver().ContextFor(target);
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
        await Assert.That(ranOnThreadId).IsNotEqualTo(writingThreadId);
    }

    /// <summary>
    /// A write already on the thread that owns the object is applied inline, so an update raised on the UI
    /// thread keeps the write synchronous and a caller reading the view back sees the new value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Post_ToAnObjectOwnedByTheWritingThread_RunsInline()
    {
        var writingThreadId = Environment.CurrentManagedThreadId;
        var context = new DispatcherViewThreadResolver().ContextFor(new Fixture());
        var ranOnThreadId = 0;

        context!.Post(_ => ranOnThreadId = Environment.CurrentManagedThreadId, null);

        await Assert.That(ranOnThreadId).IsEqualTo(writingThreadId);
    }

    /// <summary>A send from the owning thread runs inline as well.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Send_ToAnObjectOwnedByTheSendingThread_RunsInline()
    {
        var sendingThreadId = Environment.CurrentManagedThreadId;
        var context = new DispatcherViewThreadResolver().ContextFor(new Fixture());
        var ranOnThreadId = 0;

        context!.Send(_ => ranOnThreadId = Environment.CurrentManagedThreadId, null);

        await Assert.That(ranOnThreadId).IsEqualTo(sendingThreadId);
    }

    /// <summary>A send to an object owned by another UI thread runs there before it returns.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Send_ToAnObjectOwnedByASecondUiThread_RunsThereBeforeReturning()
    {
        using var owner = new UiThread();
        var target = owner.Invoke(static () => new Fixture());
        var context = new DispatcherViewThreadResolver().ContextFor(target);
        var ranOnThreadId = 0;

        context!.Send(_ => ranOnThreadId = Environment.CurrentManagedThreadId, null);

        await Assert.That(ranOnThreadId).IsEqualTo(owner.ThreadId);
    }

    /// <summary>A UI thread of its own, so an object can be owned by something other than the test's thread.</summary>
    private sealed class UiThread : IDisposable
    {
        /// <summary>Set once the dispatcher is running, which is what owns anything created on it.</summary>
        private readonly ManualResetEventSlim _running = new();

        /// <summary>The dispatcher that owns everything created through <see cref="Invoke"/>.</summary>
        private Dispatcher? _dispatcher;

        /// <summary>Initializes a new instance of the <see cref="UiThread"/> class.</summary>
        public UiThread()
        {
            var thread = new Thread(Run) { IsBackground = true, Name = nameof(UiThread) };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            _ = _running.Wait(Patience);

            ThreadId = thread.ManagedThreadId;
        }

        /// <summary>Gets the managed id of the thread this owns.</summary>
        public int ThreadId { get; }

        /// <summary>Creates something on this thread, so this thread owns it.</summary>
        /// <typeparam name="T">The type of the created object.</typeparam>
        /// <param name="create">Creates the object.</param>
        /// <returns>The created object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Invoke<T>(Func<T> create) => _dispatcher!.Invoke(create);

        /// <inheritdoc/>
        public void Dispose()
        {
            _dispatcher!.InvokeShutdown();
            _running.Dispose();
        }

        /// <summary>Runs the dispatcher this owns until it is shut down.</summary>
        private void Run()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _running.Set();
            Dispatcher.Run();
        }
    }

    /// <summary>A dependency object with no properties, standing in for a view.</summary>
    private sealed class Fixture : DependencyObject;
}
