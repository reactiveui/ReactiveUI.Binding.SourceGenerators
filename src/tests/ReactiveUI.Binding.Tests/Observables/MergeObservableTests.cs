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
