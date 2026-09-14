// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>Covers the stage that writes on the thread owning a binding's target.</summary>
[NotInParallel]
public class ViewThreadObservableTests
{
    /// <summary>The first value driven through the stage.</summary>
    private const string First = "first";

    /// <summary>The second value driven through the stage.</summary>
    private const string Second = "second";

    /// <summary>The third value driven through the stage.</summary>
    private const string Third = "third";

    /// <summary>The object the writes land on.</summary>
    private static readonly object Target = new();

    /// <summary>A missing observer is rejected.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Subscribe_WithNoObserver_Throws()
    {
        var stage = new ViewThreadObservable<string>(new ManualObservable<string>(), Target, new StubViewThreadInvoker());

        await Assert.That(() => stage.Subscribe(null!)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>A value on the owning thread is delivered inline.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnNext_OnTheOwningThread_DeliversInline()
    {
        var invoker = new StubViewThreadInvoker { HasAccess = true };
        var (source, observer, subscription) = Subscribe(invoker);

        using (subscription)
        {
            source.Observer?.OnNext(First);

            await Assert.That(string.Join(",", observer.Values)).IsEqualTo(First);
            await Assert.That(invoker.PostCount).IsEqualTo(0);
        }
    }

    /// <summary>A value from another thread waits for the invoker to run the drain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnNext_FromAnotherThread_WaitsForTheInvoker()
    {
        var invoker = new StubViewThreadInvoker();
        var (source, observer, subscription) = Subscribe(invoker);

        using (subscription)
        {
            source.Observer?.OnNext(First);

            await Assert.That(observer.Values.Count).IsEqualTo(0);

            invoker.RunPosted();

            await Assert.That(string.Join(",", observer.Values)).IsEqualTo(First);
        }
    }

    /// <summary>A burst from another thread posts one drain and keeps its order.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnNext_ABurstFromAnotherThread_PostsOnceAndKeepsTheOrder()
    {
        var invoker = new StubViewThreadInvoker();
        var (source, observer, subscription) = Subscribe(invoker);

        using (subscription)
        {
            source.Observer?.OnNext(First);
            source.Observer?.OnNext(Second);
            source.Observer?.OnNext(Third);

            invoker.RunPosted();

            await Assert.That(invoker.PostCount).IsEqualTo(1);
            await Assert.That(string.Join(",", observer.Values)).IsEqualTo("first,second,third");
        }
    }

    /// <summary>A value on the owning thread waits behind values still queued from another thread.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnNext_OnTheOwningThreadBehindQueuedValues_WaitsItsTurn()
    {
        var invoker = new StubViewThreadInvoker();
        var (source, observer, subscription) = Subscribe(invoker);

        using (subscription)
        {
            source.Observer?.OnNext(First);
            invoker.HasAccess = true;
            source.Observer?.OnNext(Second);

            await Assert.That(observer.Values.Count).IsEqualTo(0);

            invoker.RunPosted();

            await Assert.That(invoker.PostCount).IsEqualTo(1);
            await Assert.That(string.Join(",", observer.Values)).IsEqualTo("first,second");
        }
    }

    /// <summary>A value raised by a write that is being drained is delivered in the same drain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnNext_RaisedWhileDraining_IsDeliveredInTheSameDrain()
    {
        var invoker = new StubViewThreadInvoker();
        var source = new ManualObservable<string>();
        var observer = new RecordingObserver<string>(value =>
        {
            if (value == First)
            {
                source.Observer?.OnNext(Second);
            }
        });

        using (new ViewThreadObservable<string>(source, Target, invoker).Subscribe(observer))
        {
            source.Observer?.OnNext(First);
            invoker.RunPosted();

            await Assert.That(invoker.PostCount).IsEqualTo(1);
            await Assert.That(string.Join(",", observer.Values)).IsEqualTo("first,second");
        }
    }

    /// <summary>An error on the owning thread is delivered inline.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnError_OnTheOwningThread_DeliversInline()
    {
        var invoker = new StubViewThreadInvoker { HasAccess = true };
        var (source, observer, subscription) = Subscribe(invoker);
        var error = new InvalidOperationException();

        using (subscription)
        {
            source.Observer?.OnError(error);

            await Assert.That(observer.Error).IsSameReferenceAs(error);
        }
    }

    /// <summary>An error from another thread is delivered by the drain, after the values ahead of it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnError_FromAnotherThread_IsDeliveredByTheDrain()
    {
        var invoker = new StubViewThreadInvoker();
        var (source, observer, subscription) = Subscribe(invoker);
        var error = new InvalidOperationException();

        using (subscription)
        {
            source.Observer?.OnNext(First);
            source.Observer?.OnError(error);

            await Assert.That(observer.Error).IsNull();

            invoker.RunPosted();

            await Assert.That(string.Join(",", observer.Values)).IsEqualTo(First);
            await Assert.That(observer.Error).IsSameReferenceAs(error);
        }
    }

    /// <summary>Completion on the owning thread is delivered inline.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnCompleted_OnTheOwningThread_DeliversInline()
    {
        var invoker = new StubViewThreadInvoker { HasAccess = true };
        var (source, observer, subscription) = Subscribe(invoker);

        using (subscription)
        {
            source.Observer?.OnCompleted();

            await Assert.That(observer.IsCompleted).IsTrue();
        }
    }

    /// <summary>Completion from another thread is delivered by the drain.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnCompleted_FromAnotherThread_IsDeliveredByTheDrain()
    {
        var invoker = new StubViewThreadInvoker();
        var (source, observer, subscription) = Subscribe(invoker);

        using (subscription)
        {
            source.Observer?.OnCompleted();

            await Assert.That(observer.IsCompleted).IsFalse();

            invoker.RunPosted();

            await Assert.That(observer.IsCompleted).IsTrue();
        }
    }

    /// <summary>A value from another thread is drained on the main thread the host set, not through the invoker.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnNext_FromAnotherThreadWhenTheHostSetsAMainThread_DrainsThere()
    {
        var invoker = new StubViewThreadInvoker();
        var host = new RecordingSynchronizationContext();
        var (source, observer, subscription) = Subscribe(invoker);

        try
        {
            BindingSchedulers.UseSynchronizationContext(host);

            using (subscription)
            {
                source.Observer?.OnNext(First);

                await Assert.That(host.PostCount).IsEqualTo(1);
                await Assert.That(invoker.PostCount).IsEqualTo(0);
                await Assert.That(string.Join(",", observer.Values)).IsEqualTo(First);
            }
        }
        finally
        {
            BindingSchedulers.UseSynchronizationContext(null);
        }
    }

    /// <summary>Disposing before the drain runs drops what was queued.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_BeforeTheDrain_DropsWhatWasQueued()
    {
        var invoker = new StubViewThreadInvoker();
        var (source, observer, subscription) = Subscribe(invoker);

        source.Observer?.OnNext(First);
        subscription.Dispose();
        invoker.RunPosted();

        await Assert.That(observer.Values.Count).IsEqualTo(0);
    }

    /// <summary>Disposing twice disposes the source subscription once.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_Twice_DisposesTheSourceSubscriptionOnce()
    {
        var source = new CountingObservable();
        var subscription = new ViewThreadObservable<string>(source, Target, new StubViewThreadInvoker()).Subscribe(new RecordingObserver<string>());

        subscription.Dispose();
        subscription.Dispose();

        await Assert.That(source.DisposeCount).IsEqualTo(1);
    }

    /// <summary>A notification after disposal is ignored on any thread.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnNext_AfterDispose_IsIgnored()
    {
        var invoker = new StubViewThreadInvoker { HasAccess = true };
        var (source, observer, subscription) = Subscribe(invoker);

        subscription.Dispose();
        source.Observer?.OnNext(First);
        invoker.HasAccess = false;
        source.Observer?.OnNext(Second);

        await Assert.That(observer.Values.Count).IsEqualTo(0);
        await Assert.That(invoker.PostCount).IsEqualTo(0);
    }

    /// <summary>Subscribes a recording observer to a stage over a manual source.</summary>
    /// <param name="invoker">The invoker the stage routes through.</param>
    /// <returns>The source, the observer and the subscription.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (ManualObservable<string> Source, RecordingObserver<string> Observer, IDisposable Subscription) Subscribe(StubViewThreadInvoker invoker)
    {
        var source = new ManualObservable<string>();
        var observer = new RecordingObserver<string>();

        return (source, observer, new ViewThreadObservable<string>(source, Target, invoker).Subscribe(observer));
    }

    /// <summary>A source that counts how often its subscription is disposed.</summary>
    private sealed class CountingObservable : IObservable<string>
    {
        /// <summary>Gets how often the subscription was disposed.</summary>
        public int DisposeCount { get; private set; }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable Subscribe(IObserver<string> observer) => new Subscription(this);

        /// <summary>Counts its disposal on the owning source.</summary>
        /// <param name="owner">The source to count on.</param>
        private sealed class Subscription(CountingObservable owner) : IDisposable
        {
            /// <inheritdoc/>
            public void Dispose() => owner.DisposeCount++;
        }
    }
}
