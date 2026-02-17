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
