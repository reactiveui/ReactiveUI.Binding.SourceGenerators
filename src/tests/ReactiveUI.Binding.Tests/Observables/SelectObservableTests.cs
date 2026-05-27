// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="SelectObservable{TSource, TResult}"/> edge cases.
/// </summary>
public class SelectObservableTests
{
    /// <summary>
    /// The first value pushed through the source observable.
    /// </summary>
    private const int FirstInput = 5;

    /// <summary>
    /// The second value pushed through the source observable.
    /// </summary>
    private const int SecondInput = 10;

    /// <summary>
    /// The expected number of projected results.
    /// </summary>
    private const int ExpectedResultCount = 2;

    /// <summary>
    /// The expected first projected result (first input doubled).
    /// </summary>
    private const int FirstExpectedResult = 10;

    /// <summary>
    /// The expected second projected result (second input doubled).
    /// </summary>
    private const int SecondExpectedResult = 20;

    /// <summary>
    /// Verifies that error propagates through Select.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task ErrorInSource_PropagatedToSubscriber()
    {
        var source = new Subject<int>();
        var selectObs = new SelectObservable<int, string>(source, x => x.ToString());

        Exception? receivedError = null;
        var results = new List<string>();

        selectObs.Subscribe(new AnonymousObserver<string>(
            results.Add,
            ex => receivedError = ex,
            () => { }));

        var expectedError = new InvalidOperationException("source error");
        source.OnError(expectedError);

        await Assert.That(receivedError).IsNotNull();
        await Assert.That(receivedError).IsEqualTo(expectedError);
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that completion propagates through Select.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Completion_PropagatedToSubscriber()
    {
        var source = new Subject<int>();
        var selectObs = new SelectObservable<int, string>(source, x => x.ToString());

        var completed = false;
        var results = new List<string>();

        selectObs.Subscribe(new AnonymousObserver<string>(
            results.Add,
            _ => { },
            () => completed = true));

        source.OnNext(1);
        source.OnCompleted();

        await Assert.That(completed).IsTrue();
        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo("1");
    }

    /// <summary>
    /// Verifies that Subscribe throws ArgumentNullException when observer is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var source = new Subject<int>();
        var selectObs = new SelectObservable<int, string>(source, x => x.ToString());

        var action = () => selectObs.Subscribe(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that constructor throws ArgumentNullException when source is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource_ThrowsArgumentNullException()
    {
        var action = () => new SelectObservable<int, string>(null!, x => x.ToString());

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that constructor throws ArgumentNullException when selector is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSelector_ThrowsArgumentNullException()
    {
        var source = new Subject<int>();
        var action = () => new SelectObservable<int, string>(source, null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that a selector function transforms values correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Selector_TransformsValues()
    {
        var source = new Subject<int>();
        var selectObs = new SelectObservable<int, int>(source, x => x * 2);

        var results = new List<int>();

        selectObs.Subscribe(new AnonymousObserver<int>(
            results.Add,
            _ => { },
            () => { }));

        source.OnNext(FirstInput);
        source.OnNext(SecondInput);

        await Assert.That(results).Count().IsEqualTo(ExpectedResultCount);
        await Assert.That(results[0]).IsEqualTo(FirstExpectedResult);
        await Assert.That(results[1]).IsEqualTo(SecondExpectedResult);
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
