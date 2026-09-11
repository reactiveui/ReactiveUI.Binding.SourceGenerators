// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.Maui.Dispatching;

namespace ReactiveUI.Binding.Maui.Tests;

/// <summary>
/// Tests for the MAUI view-thread resolver. Compiled twice: once against ReactiveUI.Binding.Maui and once,
/// under REACTIVE_SHIM, against ReactiveUI.Binding.Reactive.Maui, so both leaves are exercised by the same
/// assertions.
/// </summary>
/// <remarks>
/// Serialized: the dispatcher a bindable object picks up comes from a process-wide provider, which these
/// swap for one that records what it was handed.
/// </remarks>
[NotInParallel]
public class DispatcherViewThreadResolverTests
{
    /// <summary>A target the resolver does not recognise, so it is left to another resolver.</summary>
    private static readonly object UnclaimedTarget = new();

    /// <summary>A write from another thread is claimed and goes through the dispatcher the object carries.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Post_FromAnotherThread_GoesThroughTheDispatcher()
    {
        var dispatcher = new RecordingDispatcher();

        using (new DispatcherProviderScope(dispatcher))
        {
            var context = new DispatcherViewThreadResolver().ContextFor(new Label());

            await Assert.That(context).IsNotNull();

            var posted = false;
            context!.Post(_ => posted = true, null);

            await Assert.That(dispatcher.DispatchCount).IsEqualTo(1);
            await Assert.That(posted).IsTrue();
        }
    }

    /// <summary>
    /// A write already on the thread that owns the object runs inline rather than queueing a turn, so an update
    /// raised on the UI thread stays synchronous.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Post_FromTheOwningThread_RunsInline()
    {
        var dispatcher = new RecordingDispatcher { IsDispatchRequired = false };

        using (new DispatcherProviderScope(dispatcher))
        {
            var context = new DispatcherViewThreadResolver().ContextFor(new Label());
            var posted = false;

            context!.Post(_ => posted = true, null);

            await Assert.That(posted).IsTrue();
            await Assert.That(dispatcher.DispatchCount).IsEqualTo(0);
        }
    }

    /// <summary>A send from the thread that owns the object runs inline rather than queueing a turn.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Send_FromTheOwningThread_RunsInline()
    {
        var dispatcher = new RecordingDispatcher { IsDispatchRequired = false };

        using (new DispatcherProviderScope(dispatcher))
        {
            var context = new DispatcherViewThreadResolver().ContextFor(new Label());
            var sent = false;

            context!.Send(_ => sent = true, null);

            await Assert.That(sent).IsTrue();
            await Assert.That(dispatcher.DispatchCount).IsEqualTo(0);
        }
    }

    /// <summary>A send from another thread goes through the dispatcher.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Send_FromAnotherThread_GoesThroughTheDispatcher()
    {
        var dispatcher = new RecordingDispatcher();

        using (new DispatcherProviderScope(dispatcher))
        {
            var context = new DispatcherViewThreadResolver().ContextFor(new Label());
            var sent = false;

            context!.Send(_ => sent = true, null);

            await Assert.That(dispatcher.DispatchCount).IsEqualTo(1);
            await Assert.That(sent).IsTrue();
        }
    }

    /// <summary>Anything that is not a bindable object is left to another resolver.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ContextFor_WithSomethingElse_ClaimsNothing() =>
        await Assert.That(new DispatcherViewThreadResolver().ContextFor(UnclaimedTarget)).IsNull();

    /// <summary>Hands out one dispatcher for the duration of a test, then puts the provider back.</summary>
    private sealed class DispatcherProviderScope : IDisposable
    {
        /// <summary>Initializes a new instance of the <see cref="DispatcherProviderScope"/> class.</summary>
        /// <param name="dispatcher">The dispatcher every bindable object created inside the scope picks up.</param>
        public DispatcherProviderScope(IDispatcher dispatcher) =>
            _ = DispatcherProvider.SetCurrent(new StubProvider(dispatcher));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _ = DispatcherProvider.SetCurrent(null);

        /// <summary>Hands the same dispatcher to every thread that asks.</summary>
        /// <param name="dispatcher">The dispatcher to hand out.</param>
        private sealed class StubProvider(IDispatcher dispatcher) : IDispatcherProvider
        {
            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IDispatcher? GetForCurrentThread() => dispatcher;
        }
    }

    /// <summary>Runs each callback inline and records that it was dispatched, standing in for a UI thread.</summary>
    private sealed class RecordingDispatcher : IDispatcher
    {
        /// <summary>Gets how many callbacks were dispatched.</summary>
        public int DispatchCount { get; private set; }

        /// <inheritdoc/>
        public bool IsDispatchRequired { get; init; } = true;

        /// <inheritdoc/>
        public IDispatcherTimer CreateTimer() => throw new NotSupportedException();

        /// <inheritdoc/>
        public bool Dispatch(Action action)
        {
            DispatchCount++;
            action();
            return true;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DispatchDelayed(TimeSpan delay, Action action) => Dispatch(action);
    }
}
