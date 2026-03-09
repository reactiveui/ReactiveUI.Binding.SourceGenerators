// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="EventObservable{T}"/>.
/// </summary>
public class EventObservableTests
{
    /// <summary>
    /// Verifies that constructor throws ArgumentNullException when addHandler is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullAddHandler_ThrowsArgumentNullException()
    {
        var action = () => new EventObservable<string>(
            null!,
            _ => { },
            () => "test",
            true);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that constructor throws ArgumentNullException when removeHandler is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullRemoveHandler_ThrowsArgumentNullException()
    {
        var action = () => new EventObservable<string>(
            _ => { },
            null!,
            () => "test",
            true);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that constructor throws ArgumentNullException when getter is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullGetter_ThrowsArgumentNullException()
    {
        var action = () => new EventObservable<string>(
            _ => { },
            _ => { },
            null!,
            true);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that Subscribe throws ArgumentNullException when observer is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var observable = new EventObservable<string>(
            _ => { },
            _ => { },
            () => "test",
            true);

        var action = () => observable.Subscribe(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that Subscribe emits the current value immediately.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_EmitsInitialValue()
    {
        var value = "initial";
        var results = new List<string>();

        var observable = new EventObservable<string>(
            _ => { },
            _ => { },
            () => value,
            false);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo("initial");
    }

    /// <summary>
    /// Verifies that event handler invocation emits updated values.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_EventFired_EmitsNewValue()
    {
        EventHandler? handler = null;
        var value = "initial";
        var results = new List<string>();

        var observable = new EventObservable<string>(
            h => handler = h,
            h => handler = null,
            () => value,
            false);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        value = "updated";
        handler?.Invoke(this, EventArgs.Empty);

        await Assert.That(results).Count().IsEqualTo(2);
        await Assert.That(results[1]).IsEqualTo("updated");
    }

    /// <summary>
    /// Verifies that distinct-until-changed suppresses duplicate values.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_DistinctUntilChanged_SuppressesDuplicates()
    {
        EventHandler? handler = null;
        var value = "same";
        var results = new List<string>();

        var observable = new EventObservable<string>(
            h => handler = h,
            h => handler = null,
            () => value,
            true);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        // Fire event with same value — should be suppressed
        handler?.Invoke(this, EventArgs.Empty);

        await Assert.That(results).Count().IsEqualTo(1);

        // Fire event with different value — should emit
        value = "different";
        handler?.Invoke(this, EventArgs.Empty);

        await Assert.That(results).Count().IsEqualTo(2);
        await Assert.That(results[1]).IsEqualTo("different");
    }

    /// <summary>
    /// Verifies that without distinct-until-changed, duplicate values are emitted.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NoDistinct_EmitsDuplicates()
    {
        EventHandler? handler = null;
        var value = "same";
        var results = new List<string>();

        var observable = new EventObservable<string>(
            h => handler = h,
            h => handler = null,
            () => value,
            false);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        // Fire event with same value — should emit
        handler?.Invoke(this, EventArgs.Empty);

        await Assert.That(results).Count().IsEqualTo(2);
    }

    /// <summary>
    /// Verifies that Dispose unsubscribes the event handler.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_UnsubscribesHandler()
    {
        EventHandler? handler = null;
        var value = "initial";
        var results = new List<string>();

        var observable = new EventObservable<string>(
            h => handler = h,
            h =>
            {
                if (h == handler)
                {
                    handler = null;
                }
            },
            () => value,
            false);

        var sub = observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        sub.Dispose();

        await Assert.That(handler).IsNull();
    }

    /// <summary>
    /// Verifies that events fired after dispose are ignored.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_EventAfterDispose_Ignored()
    {
        EventHandler? handler = null;
        var value = "initial";
        var results = new List<string>();

        var observable = new EventObservable<string>(
            h => handler = h,
            _ => { },
            () => value,
            false);

        var sub = observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        // Keep reference before dispose clears it
        var savedHandler = handler;
        sub.Dispose();

        value = "after-dispose";
        savedHandler?.Invoke(this, EventArgs.Empty);

        await Assert.That(results).Count().IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that double dispose is safe.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_CalledTwice_IsSafe()
    {
        var removeCount = 0;
        var observable = new EventObservable<string>(
            _ => { },
            _ => removeCount++,
            () => "test",
            false);

        var sub = observable.Subscribe(new AnonymousObserver<string>(_ => { }, _ => { }, () => { }));

        sub.Dispose();
        sub.Dispose();

        await Assert.That(removeCount).IsEqualTo(1);
    }

    /// <summary>
    /// Anonymous observer for testing.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
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
