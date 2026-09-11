// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>Covers which thread a binding delivers its write on.</summary>
/// <remarks>
/// A view may only be touched from the thread that owns it, and which thread that is belongs to the object
/// rather than the process. These tests run serially because the resolved set and the fallback are both
/// process-wide state.
/// </remarks>
[NotInParallel]
public class BindingSchedulerTests
{
    /// <summary>The value driven through a routed binding.</summary>
    private const string Written = "written";

    /// <summary>A target nothing claims, so the fallback decides where its write lands.</summary>
    private static readonly object UnclaimedTarget = new();

    /// <summary>A target a resolver claims is written on the thread that resolver named.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_WhenAResolverClaimsTheTarget_PostsToItsContext()
    {
        var context = new RecordingSynchronizationContext();
        var target = new object();

        using (RegisterResolver(new StubViewThreadResolver(target, context)))
        {
            var source = new ManualObservable<string>();
            var seen = string.Empty;

            using var subscription = BindingSchedulers.ObserveOnViewThread(source, target)
                .Subscribe(new CapturingObserver(value => seen = value));

            source.Observer?.OnNext(Written);

            await Assert.That(context.PostCount).IsGreaterThan(0);
            await Assert.That(seen).IsEqualTo(Written);
        }
    }

    /// <summary>
    /// Two targets owned by different threads are written on their own. This is the case a process-wide
    /// thread gets wrong, and the reason the target is asked rather than the process.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_WithTwoTargetsOnDifferentThreads_PostsEachToItsOwn()
    {
        var firstContext = new RecordingSynchronizationContext();
        var secondContext = new RecordingSynchronizationContext();
        var firstTarget = new object();
        var secondTarget = new object();

        var resolver = new StubViewThreadResolver(firstTarget, firstContext);
        resolver.Add(secondTarget, secondContext);

        using (RegisterResolver(resolver))
        {
            var first = new ManualObservable<string>();
            var second = new ManualObservable<string>();

            using var firstSub = BindingSchedulers.ObserveOnViewThread(first, firstTarget)
                .Subscribe(new CapturingObserver(static _ => { }));
            using var secondSub = BindingSchedulers.ObserveOnViewThread(second, secondTarget)
                .Subscribe(new CapturingObserver(static _ => { }));

            first.Observer?.OnNext(Written);

            await Assert.That(firstContext.PostCount).IsGreaterThan(0);
            await Assert.That(secondContext.PostCount).IsEqualTo(0);
        }
    }

    /// <summary>A target no resolver claims falls back to the blanket thread a host established.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_WhenNoResolverClaimsTheTarget_FallsBackToTheBlanketThread()
    {
        var context = new RecordingSynchronizationContext();

        try
        {
            BindingSchedulers.UseSynchronizationContext(context);

            var source = new ManualObservable<string>();

            using var subscription = BindingSchedulers.ObserveOnViewThread(source, UnclaimedTarget)
                .Subscribe(new CapturingObserver(static _ => { }));

            source.Observer?.OnNext(Written);

            await Assert.That(context.PostCount).IsGreaterThan(0);
        }
        finally
        {
            BindingSchedulers.UseSynchronizationContext(null);
        }
    }

    /// <summary>With nothing established the source is handed back untouched, so writes stay inline.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObserveOnViewThread_WithNothingEstablished_HandsBackTheSource()
    {
        BindingSchedulers.UseSynchronizationContext(null);
        ViewThreadResolvers.Refresh();

        var source = new ManualObservable<string>();

        await Assert.That(BindingSchedulers.ObserveOnViewThread(source, UnclaimedTarget)).IsSameReferenceAs(source);
    }

    /// <summary>A null target claims nothing, which is what the unsuffixed entry point hands in.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ForTarget_WithNoTarget_ClaimsNothing()
    {
        ViewThreadResolvers.Refresh();

        await Assert.That(ViewThreadResolvers.ForTarget(target: null)).IsNull();
    }

    /// <summary>Registers a resolver and drops the resolved set again on dispose.</summary>
    /// <param name="resolver">The resolver to register.</param>
    /// <returns>A scope that drops the resolved set on dispose.</returns>
    private static ResolverScope RegisterResolver(IViewThreadResolver resolver)
    {
        Locator.CurrentMutable.RegisterConstant(resolver);
        ViewThreadResolvers.Refresh();

        return new();
    }

    /// <summary>Drops the resolved set so a later test does not inherit this one's resolver.</summary>
    private sealed class ResolverScope : IDisposable
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => ViewThreadResolvers.Refresh();
    }

    /// <summary>Claims only the targets it was told about, as a platform resolver claims only its own.</summary>
    /// <param name="target">The target this resolver claims.</param>
    /// <param name="context">The context owning it.</param>
    private sealed class StubViewThreadResolver(object target, SynchronizationContext context) : IViewThreadResolver
    {
        /// <summary>The targets this resolver claims, against the thread that owns each.</summary>
        private readonly Dictionary<object, SynchronizationContext> _owners = new() { [target] = context };

        /// <summary>Claims one more target.</summary>
        /// <param name="other">The target to claim.</param>
        /// <param name="owner">The context owning it.</param>
        public void Add(object other, SynchronizationContext owner) => _owners[other] = owner;

        /// <inheritdoc/>
        public SynchronizationContext? ContextFor(object target) =>
            _owners.TryGetValue(target, out var owner) ? owner : null;
    }

    /// <summary>A context that records what was posted to it, standing in for a UI framework's.</summary>
    private sealed class RecordingSynchronizationContext : SynchronizationContext
    {
        /// <summary>Gets how many callbacks were posted.</summary>
        public int PostCount { get; private set; }

        /// <inheritdoc/>
        public override void Post(SendOrPostCallback d, object? state)
        {
            PostCount++;
            d(state);
        }

        /// <inheritdoc/>
        public override void Send(SendOrPostCallback d, object? state) => Post(d, state);
    }

    /// <summary>Hands each value to a callback, so a test can see what arrived.</summary>
    /// <param name="onValue">Receives each value.</param>
    private sealed class CapturingObserver(Action<string> onValue) : IObserver<string>
    {
        /// <inheritdoc/>
        public void OnCompleted()
        {
        }

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(string value) => onValue(value);
    }
}
