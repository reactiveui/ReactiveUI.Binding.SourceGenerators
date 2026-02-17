// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="SkipObservable{T}"/>.
/// </summary>
public class SkipObservableTests
{
    /// <summary>
    /// Verifies that Skip(0) forwards all items.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Skip_Zero_ForwardsAllItems()
    {
        var results = new List<int>();
        var source = new ReturnObservable<int>(42);
        var skip = new SkipObservable<int>(source, 0);

        skip.Subscribe(new AnonymousObserver<int>(results.Add, _ => { }, () => { }));

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(42);
    }

    /// <summary>
    /// Verifies that Skip(N) skips first N items and forwards the rest.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Skip_N_SkipsFirstNItems()
    {
        var results = new List<int>();
        var source = new AnonymousObservable<int>(observer =>
        {
            observer.OnNext(1);
            observer.OnNext(2);
            observer.OnNext(3);
            observer.OnCompleted();
            return EmptyDisposable.Instance;
        });

        var skip = new SkipObservable<int>(source, 2);
        skip.Subscribe(new AnonymousObserver<int>(results.Add, _ => { }, () => { }));

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(3);
    }

    /// <summary>
    /// Verifies that Skip(N) forwards no items when N is greater than total items.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Skip_NGreaterThanTotal_ForwardsNoItems()
    {
        var results = new List<int>();
        var source = new ReturnObservable<int>(42);
        var skip = new SkipObservable<int>(source, 5);

        skip.Subscribe(new AnonymousObserver<int>(results.Add, _ => { }, () => { }));

        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that Skip(N) forwards errors from the source.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Skip_ForwardsError()
    {
        var errorThrown = false;
        var source = new AnonymousObservable<int>(observer =>
        {
            observer.OnError(new Exception("test"));
            return EmptyDisposable.Instance;
        });

        var skip = new SkipObservable<int>(source, 1);
        skip.Subscribe(new AnonymousObserver<int>(_ => { }, _ => errorThrown = true, () => { }));

        await Assert.That(errorThrown).IsTrue();
    }

    /// <summary>
    /// Verifies that Skip(N) forwards completion from the source.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Skip_ForwardsCompletion()
    {
        var completed = false;
        var source = EmptyObservable<int>.Instance;
        var skip = new SkipObservable<int>(source, 1);

        skip.Subscribe(new AnonymousObserver<int>(_ => { }, _ => { }, () => completed = true));

        await Assert.That(completed).IsTrue();
    }

    /// <summary>
    /// Verifies that the constructor throws <see cref="ArgumentNullException"/> when source is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource_ThrowsArgumentNullException()
    {
        var action = () => new SkipObservable<int>(null!, 1);

        await Assert.That(action).Throws<ArgumentNullException>().WithParameterName("source");
    }

    /// <summary>
    /// Verifies that Subscribe throws <see cref="ArgumentNullException"/> when observer is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var skip = new SkipObservable<int>(EmptyObservable<int>.Instance, 1);
        var action = () => skip.Subscribe(null!);

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
