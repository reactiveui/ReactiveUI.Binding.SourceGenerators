// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="EmptyObservable{T}"/>.
/// </summary>
public class EmptyObservableTests
{
    /// <summary>
    /// Verifies that EmptyObservable completes immediately on subscribe.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_CompletesImmediately()
    {
        var completed = false;
        var results = new List<int>();

        EmptyObservable<int>.Instance.Subscribe(new AnonymousObserver<int>(
            results.Add,
            _ => { },
            () => completed = true));

        await Assert.That(completed).IsTrue();
        await Assert.That(results).IsEmpty();
    }

    /// <summary>
    /// Verifies that EmptyObservable throws ArgumentNullException for null observer.
    /// </summary>
    [Test]
    public void Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => EmptyObservable<int>.Instance.Subscribe(null!));
    }

    /// <summary>
    /// Verifies that the singleton instance is reused.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Instance_IsSingleton()
    {
        var instance1 = EmptyObservable<int>.Instance;
        var instance2 = EmptyObservable<int>.Instance;

        await Assert.That(ReferenceEquals(instance1, instance2)).IsTrue();
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
