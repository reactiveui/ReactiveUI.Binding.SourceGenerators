// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="CombineLatest2Observable{T1, T2, TResult}"/> edge cases.
/// </summary>
public class CombineLatest2ObservableTests
{
    /// <summary>
    /// Verifies that a null first source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource1_Throws()
    {
        var act = () => new CombineLatest2Observable<int, int, int>(
            null!,
            new Subject<int>(),
            (a, b) => a + b);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null second source throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource2_Throws()
    {
        var act = () => new CombineLatest2Observable<int, int, int>(
            new Subject<int>(),
            null!,
            (a, b) => a + b);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a null result selector throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullResultSelector_Throws()
    {
        var act = () => new CombineLatest2Observable<int, int, int>(
            new Subject<int>(),
            new Subject<int>(),
            null!);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that subscribing with a null observer throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_Throws()
    {
        var combined = new CombineLatest2Observable<int, int, int>(
            new Subject<int>(),
            new Subject<int>(),
            (a, b) => a + b);

        var act = () => combined.Subscribe(null!);

        await Assert.That(act).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that disposing twice does not throw an exception.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_CalledTwice_NoException()
    {
        var combined = new CombineLatest2Observable<int, int, int>(
            new Subject<int>(),
            new Subject<int>(),
            (a, b) => a + b);

        var subscription = combined.Subscribe(new AnonymousObserver<int>(_ => { }, _ => { }, () => { }));
        subscription.Dispose();
        subscription.Dispose();

        await Assert.That(subscription.GetType()).IsNotNull();
    }

    /// <summary>
    /// Verifies that a source emitting after dispose does not throw.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task SourceEmitsAfterDispose_NoException()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var combined = new CombineLatest2Observable<int, int, int>(
            source1,
            source2,
            (a, b) => a + b);

        var results = new List<int>();
        var subscription = combined.Subscribe(new AnonymousObserver<int>(results.Add, _ => { }, () => { }));

        source1.OnNext(1);
        source2.OnNext(2);
        subscription.Dispose();

        source1.OnNext(10);
        source2.OnNext(20);

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(3);
    }

    /// <summary>
    /// Verifies that an error after dispose is ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorAfterDispose_IsIgnored()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var combined = new CombineLatest2Observable<int, int, int>(
            source1,
            source2,
            (a, b) => a + b);

        Exception? receivedError = null;
        var subscription = combined.Subscribe(new AnonymousObserver<int>(
            _ => { },
            ex => receivedError = ex,
            () => { }));

        subscription.Dispose();
        source1.OnError(new InvalidOperationException("should be ignored"));

        await Assert.That(receivedError).IsNull();
    }

    /// <summary>
    /// Verifies that an error in the first source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInFirstSource_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<string>();
        var combined = new CombineLatest2Observable<int, string, string>(
            source1,
            source2,
            (a, b) => $"{a}-{b}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source1 error");
        source1.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that an error in the second source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSecondSource_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<string>();
        var combined = new CombineLatest2Observable<int, string, string>(
            source1,
            source2,
            (a, b) => $"{a}-{b}");

        Exception? receivedError = null;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source2 error");
        source2.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that OnCompleted from a single source does not propagate completion.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task CompletedInSource_DoesNotPropagateCompletion()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var combined = new CombineLatest2Observable<int, int, string>(
            source1,
            source2,
            (a, b) => $"{a}-{b}");

        var completed = false;
        var results = new List<string>();

        combined.Subscribe(new AnonymousObserver<string>(
            results.Add,
            _ => { },
            () => completed = true));

        source1.OnNext(1);
        source2.OnNext(2);

        source1.OnCompleted();
        source2.OnCompleted();

        await Assert.That(completed).IsFalse();
        await Assert.That(results).Count().IsEqualTo(1);
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
