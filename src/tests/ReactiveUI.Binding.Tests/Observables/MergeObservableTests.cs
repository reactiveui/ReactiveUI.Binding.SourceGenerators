// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="MergeObservable{T}"/> edge cases.
/// </summary>
public class MergeObservableTests
{
    /// <summary>
    /// Verifies that Subscribe throws ArgumentNullException when observer is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var source1 = new Subject<int>();
        var merged = new MergeObservable<int>(source1);

        var action = () => merged.Subscribe(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that constructor throws ArgumentNullException when sources is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSources_ThrowsArgumentNullException()
    {
        var action = () => new MergeObservable<int>(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that an error in any source observable is propagated to the subscriber.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInAnySource_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var merged = new MergeObservable<int>(source1, source2);

        Exception? receivedError = null;
        var results = new List<int>();

        merged.Subscribe(new AnonymousObserver<int>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        source1.OnNext(1);
        source2.OnNext(2);

        var expectedError = new InvalidOperationException("source error");
        source1.OnError(expectedError);

        await Assert.That(results).Count().IsEqualTo(2);
        await Assert.That(results[0]).IsEqualTo(1);
        await Assert.That(results[1]).IsEqualTo(2);
        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
    }

    /// <summary>
    /// Verifies that disposal stops all emissions from merged sources.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Disposal_StopsAllEmissions()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var merged = new MergeObservable<int>(source1, source2);

        var results = new List<int>();

        var subscription = merged.Subscribe(new AnonymousObserver<int>(
            results.Add,
            _ => { },
            () => { }));

        source1.OnNext(1);
        subscription.Dispose();
        source1.OnNext(99);
        source2.OnNext(99);

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that double disposal does not throw.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DoubleDisposal_DoesNotThrow()
    {
        var source1 = new Subject<int>();
        var merged = new MergeObservable<int>(source1);

        var results = new List<int>();

        var subscription = merged.Subscribe(new AnonymousObserver<int>(
            results.Add,
            _ => { },
            () => { }));

        subscription.Dispose();
        subscription.Dispose();

        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that error from second source after first emitted is propagated.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSecondSource_AfterFirstEmitted_PropagatedToSubscriber()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var merged = new MergeObservable<int>(source1, source2);

        Exception? receivedError = null;
        var results = new List<int>();

        merged.Subscribe(new AnonymousObserver<int>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        source1.OnNext(1);
        var expectedError = new InvalidOperationException("source2 error");
        source2.OnError(expectedError);

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
    }

    /// <summary>
    /// A simple observer that delegates to provided actions.
    /// </summary>
    /// <typeparam name="T">The type of elements observed.</typeparam>
    /// <param name="onNext">The action to invoke for each element.</param>
    /// <param name="onError">The action to invoke on error.</param>
    /// <param name="onCompleted">The action to invoke on completion.</param>
    private sealed class AnonymousObserver<T>(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        : IObserver<T>
    {
        /// <inheritdoc/>
        public void OnCompleted() => onCompleted();

        /// <inheritdoc/>
        public void OnError(Exception error) => onError(error);

        /// <inheritdoc/>
        public void OnNext(T value) => onNext(value);
    }
}
