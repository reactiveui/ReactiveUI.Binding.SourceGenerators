// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="SwitchObservable{T}"/> edge cases.
/// </summary>
public class SwitchObservableTests
{
    /// <summary>
    /// Verifies that Subscribe throws ArgumentNullException when observer is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var outerSubject = new Subject<IObservable<int>>();
        var switchObs = new SwitchObservable<int>(outerSubject);

        var action = () => switchObs.Subscribe(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that constructor throws ArgumentNullException when source is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource_ThrowsArgumentNullException()
    {
        var action = () => new SwitchObservable<int>(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that an error in the outer observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task OuterError_PropagatedToSubscriber()
    {
        var outerSubject = new Subject<IObservable<int>>();
        var switchObs = new SwitchObservable<int>(outerSubject);

        Exception? receivedError = null;
        var results = new List<int>();

        switchObs.Subscribe(new AnonymousObserver<int>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("outer error");
        outerSubject.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the inner observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task InnerError_PropagatedToSubscriber()
    {
        var outerSubject = new Subject<IObservable<int>>();
        var innerSubject = new Subject<int>();
        var switchObs = new SwitchObservable<int>(outerSubject);

        Exception? receivedError = null;
        var results = new List<int>();

        switchObs.Subscribe(new AnonymousObserver<int>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        outerSubject.OnNext(innerSubject);
        innerSubject.OnNext(42);

        var expectedError = new InvalidOperationException("inner error");
        innerSubject.OnError(expectedError);

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(42);
        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
    }

    /// <summary>
    /// Verifies that rapid switching (new inner before old completes) causes only the latest inner to emit.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RapidSwitching_OnlyLatestInnerEmits()
    {
        var outerSubject = new Subject<IObservable<int>>();
        var inner1 = new Subject<int>();
        var inner2 = new Subject<int>();
        var switchObs = new SwitchObservable<int>(outerSubject);

        var results = new List<int>();

        switchObs.Subscribe(new AnonymousObserver<int>(
            results.Add,
            _ => { },
            () => { }));

        // Subscribe to first inner
        outerSubject.OnNext(inner1);
        inner1.OnNext(1);

        // Switch to second inner before first completes
        outerSubject.OnNext(inner2);

        // Emit from old inner - should be ignored because it was disposed
        inner1.OnNext(99);

        // Emit from new inner - should be received
        inner2.OnNext(2);

        await Assert.That(results).Count().IsEqualTo(2);
        await Assert.That(results[0]).IsEqualTo(1);
        await Assert.That(results[1]).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies that disposal during an active inner subscription stops all emissions.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Disposal_DuringActiveInner_NoMoreEmissions()
    {
        var outerSubject = new Subject<IObservable<int>>();
        var innerSubject = new Subject<int>();
        var switchObs = new SwitchObservable<int>(outerSubject);

        var results = new List<int>();

        var subscription = switchObs.Subscribe(new AnonymousObserver<int>(
            results.Add,
            _ => { },
            () => { }));

        outerSubject.OnNext(innerSubject);
        innerSubject.OnNext(1);

        // Dispose the subscription
        subscription.Dispose();

        // Emit after disposal - should not be received
        innerSubject.OnNext(2);
        outerSubject.OnNext(new Subject<int>());

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that null inner observable is handled gracefully (no subscription).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task NullInnerObservable_IsIgnored()
    {
        var outerSubject = new Subject<IObservable<int>>();
        var switchObs = new SwitchObservable<int>(outerSubject);

        var results = new List<int>();

        switchObs.Subscribe(new AnonymousObserver<int>(
            results.Add,
            _ => { },
            () => { }));

        outerSubject.OnNext(null!);

        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that double disposal does not throw.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DoubleDisposal_DoesNotThrow()
    {
        var outerSubject = new Subject<IObservable<int>>();
        var switchObs = new SwitchObservable<int>(outerSubject);

        var results = new List<int>();

        var subscription = switchObs.Subscribe(new AnonymousObserver<int>(
            results.Add,
            _ => { },
            () => { }));

        subscription.Dispose();
        subscription.Dispose();

        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that outer OnCompleted does not terminate the subscription.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task OuterCompleted_InnerStillEmits()
    {
        var outerSubject = new Subject<IObservable<int>>();
        var innerSubject = new Subject<int>();
        var switchObs = new SwitchObservable<int>(outerSubject);

        var results = new List<int>();

        switchObs.Subscribe(new AnonymousObserver<int>(
            results.Add,
            _ => { },
            () => { }));

        outerSubject.OnNext(innerSubject);
        outerSubject.OnCompleted();

        // Inner should still emit after outer completes
        innerSubject.OnNext(42);

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(42);
    }

    /// <summary>
    /// Verifies that outer OnNext after dispose is ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task OuterOnNextAfterDispose_IsIgnored()
    {
        var outerSubject = new Subject<IObservable<int>>();
        var switchObs = new SwitchObservable<int>(outerSubject);

        var results = new List<int>();

        var subscription = switchObs.Subscribe(new AnonymousObserver<int>(
            results.Add,
            _ => { },
            () => { }));

        subscription.Dispose();

        // Emit on outer after disposal - should be ignored
        outerSubject.OnNext(new Subject<int>());

        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that outer error after dispose is ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task OuterErrorAfterDispose_IsIgnored()
    {
        var outerSubject = new Subject<IObservable<int>>();
        var switchObs = new SwitchObservable<int>(outerSubject);

        Exception? receivedError = null;

        var subscription = switchObs.Subscribe(new AnonymousObserver<int>(
            _ => { },
            ex => receivedError = ex,
            () => { }));

        subscription.Dispose();

        // Error on outer after disposal - should be ignored
        outerSubject.OnError(new InvalidOperationException("should be ignored"));

        await Assert.That(receivedError).IsNull();
    }

    /// <summary>
    /// Verifies that inner error after parent dispose is ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task InnerErrorAfterDispose_IsIgnored()
    {
        var outerSubject = new Subject<IObservable<int>>();
        var innerSubject = new Subject<int>();
        var switchObs = new SwitchObservable<int>(outerSubject);

        Exception? receivedError = null;

        var subscription = switchObs.Subscribe(new AnonymousObserver<int>(
            _ => { },
            ex => receivedError = ex,
            () => { }));

        outerSubject.OnNext(innerSubject);
        subscription.Dispose();

        // Error on inner after parent disposal - should be ignored
        innerSubject.OnError(new InvalidOperationException("should be ignored"));

        await Assert.That(receivedError).IsNull();
    }

    /// <summary>
    /// Verifies that inner OnCompleted does not propagate to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task InnerCompleted_DoesNotPropagateToSubscriber()
    {
        var outerSubject = new Subject<IObservable<int>>();
        var innerSubject = new Subject<int>();
        var switchObs = new SwitchObservable<int>(outerSubject);

        var completed = false;
        var results = new List<int>();

        switchObs.Subscribe(new AnonymousObserver<int>(
            results.Add,
            _ => { },
            () => completed = true));

        outerSubject.OnNext(innerSubject);
        innerSubject.OnNext(1);
        innerSubject.OnCompleted();

        await Assert.That(completed).IsFalse();
        await Assert.That(results).Count().IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that OnNext on the outer observable after dispose is ignored when the outer
    /// observable does not respect disposal (ManualObservable pattern). This covers
    /// SwitchObservable line 59 (_disposed != 0 TRUE branch).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task OnNext_AfterDispose_ViaManualObservable_IsIgnored()
    {
        var manualOuter = new ReactiveUI.Binding.Tests.TestModels.ManualObservable<IObservable<int>>();
        var switchObs = new SwitchObservable<int>(manualOuter);

        var results = new List<int>();

        var subscription = switchObs.Subscribe(new AnonymousObserver<int>(
            results.Add,
            _ => { },
            () => { }));

        // Dispose the subscription
        subscription.Dispose();

        // Now call OnNext on the outer observer even after dispose
        // ManualObservable retains the observer reference regardless of disposal
        manualOuter.Observer?.OnNext(System.Reactive.Linq.Observable.Return(42));

        // The OnNext should have been ignored due to _disposed != 0
        await Assert.That(results).IsEmpty();
    }

    private sealed class AnonymousObserver<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;

        public AnonymousObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public void OnCompleted() => _onCompleted();

        public void OnError(Exception error) => _onError(error);

        public void OnNext(T value) => _onNext(value);
    }
}
