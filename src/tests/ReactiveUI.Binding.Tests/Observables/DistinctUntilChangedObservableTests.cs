// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="DistinctUntilChangedObservable{T}"/>.
/// </summary>
public class DistinctUntilChangedObservableTests
{
    /// <summary>
    /// Verifies that the first value is always emitted and consecutive duplicates are suppressed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Distinct_SuppressesConsecutiveDuplicates()
    {
        var results = new List<int>();
        var source = new AnonymousObservable<int>(observer =>
        {
            observer.OnNext(1);
            observer.OnNext(1);
            observer.OnNext(2);
            observer.OnNext(2);
            observer.OnNext(1);
            observer.OnCompleted();
            return EmptyDisposable.Instance;
        });

        var distinct = new DistinctUntilChangedObservable<int>(source);
        distinct.Subscribe(new AnonymousObserver<int>(results.Add, _ => { }, () => { }));

        await Assert.That(results).Count().IsEqualTo(3);
        await Assert.That(results[0]).IsEqualTo(1);
        await Assert.That(results[1]).IsEqualTo(2);
        await Assert.That(results[2]).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that a custom IEqualityComparer is respected.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Distinct_CustomComparer_IsRespected()
    {
        var results = new List<string>();
        var source = new AnonymousObservable<string>(observer =>
        {
            observer.OnNext("a");
            observer.OnNext("A");
            observer.OnNext("b");
            observer.OnCompleted();
            return EmptyDisposable.Instance;
        });

        var distinct = new DistinctUntilChangedObservable<string>(source, StringComparer.OrdinalIgnoreCase);
        distinct.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        await Assert.That(results).Count().IsEqualTo(2);
        await Assert.That(results[0]).IsEqualTo("a");
        await Assert.That(results[1]).IsEqualTo("b");
    }

    /// <summary>
    /// Verifies that errors from the source are forwarded.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Distinct_ForwardsError()
    {
        var errorThrown = false;
        var source = new AnonymousObservable<int>(observer =>
        {
            observer.OnError(new Exception("test"));
            return EmptyDisposable.Instance;
        });

        var distinct = new DistinctUntilChangedObservable<int>(source);
        distinct.Subscribe(new AnonymousObserver<int>(_ => { }, _ => errorThrown = true, () => { }));

        await Assert.That(errorThrown).IsTrue();
    }

    /// <summary>
    /// Verifies that completion from the source is forwarded.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Distinct_ForwardsCompletion()
    {
        var completed = false;
        var source = EmptyObservable<int>.Instance;
        var distinct = new DistinctUntilChangedObservable<int>(source);

        distinct.Subscribe(new AnonymousObserver<int>(_ => { }, _ => { }, () => completed = true));

        await Assert.That(completed).IsTrue();
    }

    /// <summary>
    /// Verifies that the constructor throws <see cref="ArgumentNullException"/> when source is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource_ThrowsArgumentNullException()
    {
        var action = () => new DistinctUntilChangedObservable<int>(null!);

        await Assert.That(action).Throws<ArgumentNullException>().WithParameterName("source");
    }

    /// <summary>
    /// Verifies that the constructor throws <see cref="ArgumentNullException"/> when comparer is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullComparer_ThrowsArgumentNullException()
    {
        var action = () => new DistinctUntilChangedObservable<int>(EmptyObservable<int>.Instance, null!);

        await Assert.That(action).Throws<ArgumentNullException>().WithParameterName("comparer");
    }

    /// <summary>
    /// Verifies that Subscribe throws <see cref="ArgumentNullException"/> when observer is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var distinct = new DistinctUntilChangedObservable<int>(EmptyObservable<int>.Instance);
        var action = () => distinct.Subscribe(null!);

        await Assert.That(action).Throws<ArgumentNullException>().WithParameterName("observer");
    }

    private sealed class AnonymousObservable<T> : IObservable<T>
    {
        private readonly Func<IObserver<T>, IDisposable> _subscribe;

        public AnonymousObservable(Func<IObserver<T>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(IObserver<T> observer) => _subscribe(observer);
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
