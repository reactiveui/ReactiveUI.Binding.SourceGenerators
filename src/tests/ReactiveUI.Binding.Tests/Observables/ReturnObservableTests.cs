// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="ReturnObservable{T}"/>.
/// </summary>
public class ReturnObservableTests
{
    /// <summary>
    /// Verifies that Subscribe emits the single value and then completes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_EmitsValueAndCompletes()
    {
        var nextValue = -1;
        var completed = false;
        var observer = new AnonymousObserver<int>(
            v => nextValue = v,
            _ => { },
            () => completed = true);

        var observable = new ReturnObservable<int>(42);
        var disposable = observable.Subscribe(observer);

        await Assert.That(nextValue).IsEqualTo(42);
        await Assert.That(completed).IsTrue();
        await Assert.That(disposable).IsEqualTo(EmptyDisposable.Instance);
    }

    /// <summary>
    /// Verifies that Subscribe works with null values for reference types.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_WithNullValue_EmitsNullAndCompletes()
    {
        string? nextValue = "not null";
        var completed = false;
        var observer = new AnonymousObserver<string?>(
            v => nextValue = v,
            _ => { },
            () => completed = true);

        var observable = new ReturnObservable<string?>(null);
        observable.Subscribe(observer);

        await Assert.That(nextValue).IsNull();
        await Assert.That(completed).IsTrue();
    }

    /// <summary>
    /// Verifies that Subscribe throws ArgumentNullException when observer is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var observable = new ReturnObservable<int>(42);
        var action = () => observable.Subscribe(null!);

        await Assert.That(action).Throws<ArgumentNullException>().WithParameterName("observer");
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
