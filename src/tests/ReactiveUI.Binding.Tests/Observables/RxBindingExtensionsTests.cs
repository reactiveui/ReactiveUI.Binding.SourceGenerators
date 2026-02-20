// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Observables;
using RxBinding = ReactiveUI.Binding.Observables.RxBindingExtensions;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="RxBindingExtensions"/>.
/// </summary>
public class RxBindingExtensionsTests
{
    /// <summary>
    /// Verifies that Subscribe invokes the action for each value.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_InvokesAction()
    {
        var results = new List<int>();
        var source = new ReturnObservable<int>(42);

        RxBinding.Subscribe(source, results.Add);

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(42);
    }

    /// <summary>
    /// Verifies that Select applies the projection correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Select_AppliesProjection()
    {
        var results = new List<string>();
        var source = new ReturnObservable<int>(42);

        var select = RxBinding.Select(source, x => x.ToString());
        select.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo("42");
    }

    /// <summary>
    /// Verifies that Skip delegates to SkipObservable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Skip_DelegatesToSkipObservable()
    {
        var results = new List<int>();
        var source = new AnonymousObservable<int>(observer =>
        {
            observer.OnNext(1);
            observer.OnNext(2);
            return EmptyDisposable.Instance;
        });

        var skip = RxBinding.Skip(source, 1);
        skip.Subscribe(new AnonymousObserver<int>(results.Add, _ => { }, () => { }));

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies that DistinctUntilChanged delegates to DistinctUntilChangedObservable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DistinctUntilChanged_DelegatesToDistinctUntilChangedObservable()
    {
        var results = new List<int>();
        var source = new AnonymousObservable<int>(observer =>
        {
            observer.OnNext(1);
            observer.OnNext(1);
            observer.OnNext(2);
            return EmptyDisposable.Instance;
        });

        var distinct = RxBinding.DistinctUntilChanged(source);
        distinct.Subscribe(new AnonymousObserver<int>(results.Add, _ => { }, () => { }));

        await Assert.That(results).Count().IsEqualTo(2);
        await Assert.That(results[0]).IsEqualTo(1);
        await Assert.That(results[1]).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies that Merge merges multiple observables.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Merge_MergesMultipleObservables()
    {
        var results = new List<int>();
        var s1 = new ReturnObservable<int>(1);
        var s2 = new ReturnObservable<int>(2);

        var merged = RxBinding.Merge(s1, s2);
        merged.Subscribe(new AnonymousObserver<int>(results.Add, _ => { }, () => { }));

        await Assert.That(results).Count().IsEqualTo(2);
        await Assert.That(results).Contains(1);
        await Assert.That(results).Contains(2);
    }

    /// <summary>
    /// Verifies that Switch switches to the newest inner observable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Switch_SwitchesToNewestObservable()
    {
        var results = new List<int>();
        var inner1 = new AnonymousObservable<int>(observer =>
        {
            observer.OnNext(1);
            return EmptyDisposable.Instance;
        });
        var inner2 = new AnonymousObservable<int>(observer =>
        {
            observer.OnNext(2);
            return EmptyDisposable.Instance;
        });

        var source = new AnonymousObservable<IObservable<int>>(observer =>
        {
            observer.OnNext(inner1);
            observer.OnNext(inner2);
            return EmptyDisposable.Instance;
        });

        var switched = RxBinding.Switch(source);
        switched.Subscribe(new AnonymousObserver<int>(results.Add, _ => { }, () => { }));

        await Assert.That(results).Count().IsEqualTo(2);
        await Assert.That(results[0]).IsEqualTo(1);
        await Assert.That(results[1]).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies that Subscribe throws ArgumentNullException when source is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullSource_ThrowsArgumentNullException()
    {
        var action = () => RxBinding.Subscribe<int>(null!, _ => { });

        await Assert.That(action).Throws<ArgumentNullException>().WithParameterName("source");
    }

    /// <summary>
    /// Verifies that Subscribe throws ArgumentNullException when onNext is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullOnNext_ThrowsArgumentNullException()
    {
        var action = () => RxBinding.Subscribe<int>(EmptyObservable<int>.Instance, null!);

        await Assert.That(action).Throws<ArgumentNullException>().WithParameterName("onNext");
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
