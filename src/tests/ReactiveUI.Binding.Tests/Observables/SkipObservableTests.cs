// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>Unit tests for <see cref="SkipObservable{T}"/>.</summary>
public class SkipObservableTests
{
    /// <summary>The second value emitted by the source sequence.</summary>
    private const int SecondValue = 2;

    /// <summary>A skip count larger than the number of items the source emits.</summary>
    private const int SkipBeyondCount = 5;

    /// <summary>The number of leading items to skip.</summary>
    private const int SkipCount = 2;

    /// <summary>The single value produced by the source observable in single-item tests.</summary>
    private const int SingleValue = 42;

    /// <summary>The last value emitted after skipping the first items in the multi-item test.</summary>
    private const int LastEmittedValue = 3;

    /// <summary>Verifies that Skip(0) forwards all items.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Skip_Zero_ForwardsAllItems()
    {
        var results = new List<int>();
        var source = new ReturnObservable<int>(SingleValue);
        var skip = new SkipObservable<int>(source, 0);

        _ = skip.Subscribe(new AnonymousObserver<int>(results.Add, static _ => { }, static () => { }));

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(SingleValue);
    }

    /// <summary>Verifies that Skip(N) skips first N items and forwards the rest.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Skip_N_SkipsFirstNItems()
    {
        var results = new List<int>();
        var source = new AnonymousObservable<int>(static observer =>
        {
            observer.OnNext(1);
            observer.OnNext(SecondValue);
            observer.OnNext(LastEmittedValue);
            observer.OnCompleted();
            return EmptyDisposable.Instance;
        });

        var skip = new SkipObservable<int>(source, SkipCount);
        _ = skip.Subscribe(new AnonymousObserver<int>(results.Add, static _ => { }, static () => { }));

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(LastEmittedValue);
    }

    /// <summary>Verifies that Skip(N) forwards no items when N is greater than total items.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Skip_NGreaterThanTotal_ForwardsNoItems()
    {
        var results = new List<int>();
        var source = new ReturnObservable<int>(SingleValue);
        var skip = new SkipObservable<int>(source, SkipBeyondCount);

        _ = skip.Subscribe(new AnonymousObserver<int>(results.Add, static _ => { }, static () => { }));

        await Assert.That(results).IsEmpty();
    }

    /// <summary>Verifies that Skip(N) forwards errors from the source.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Skip_ForwardsError()
    {
        var errorThrown = false;
        var source = new AnonymousObservable<int>(static observer =>
        {
            observer.OnError(new InvalidOperationException("test"));
            return EmptyDisposable.Instance;
        });

        var skip = new SkipObservable<int>(source, 1);
        _ = skip.Subscribe(new AnonymousObserver<int>(static _ => { }, _ => errorThrown = true, static () => { }));

        await Assert.That(errorThrown).IsTrue();
    }

    /// <summary>Verifies that Skip(N) forwards completion from the source.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Skip_ForwardsCompletion()
    {
        var completed = false;
        var source = EmptyObservable<int>.Instance;
        var skip = new SkipObservable<int>(source, 1);

        _ = skip.Subscribe(new AnonymousObserver<int>(static _ => { }, static _ => { }, () => completed = true));

        await Assert.That(completed).IsTrue();
    }

    /// <summary>Verifies that the constructor throws <see cref="ArgumentNullException"/> when source is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource_ThrowsArgumentNullException()
    {
        var action = static () => new SkipObservable<int>(null!, 1);

        await Assert.That(action).Throws<ArgumentNullException>().WithParameterName("source");
    }

    /// <summary>Verifies that Subscribe throws <see cref="ArgumentNullException"/> when observer is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var skip = new SkipObservable<int>(EmptyObservable<int>.Instance, 1);
        var action = () => skip.Subscribe(null!);

        await Assert.That(action).Throws<ArgumentNullException>().WithParameterName("observer");
    }

    /// <summary>A simple observable that delegates subscription to a provided function.</summary>
    /// <typeparam name="T">The type of elements produced.</typeparam>
    /// <param name="subscribe">The function to invoke when an observer subscribes.</param>
    private sealed class AnonymousObservable<T>(Func<IObserver<T>, IDisposable> subscribe) : IObservable<T>
    {
        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<T> observer) => subscribe(observer);
    }

    /// <summary>A simple observer that delegates to provided actions.</summary>
    /// <typeparam name="T">The type of elements observed.</typeparam>
    /// <param name="onNext">The action to invoke for each element.</param>
    /// <param name="onError">The action to invoke on error.</param>
    /// <param name="onCompleted">The action to invoke on completion.</param>
    private sealed class AnonymousObserver<T>(Action<T> onNext, Action<Exception> onError, Action onCompleted) : IObserver<T>
    {
        /// <inheritdoc/>
        public void OnCompleted() => onCompleted();

        /// <inheritdoc/>
        public void OnError(Exception error) => onError(error);

        /// <inheritdoc/>
        public void OnNext(T value) => onNext(value);
    }
}
