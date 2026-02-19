// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="PropertyChangingObservable{T}"/>.
/// </summary>
public class PropertyChangingObservableTests
{
    /// <summary>
    /// Verifies that Subscribe throws ArgumentNullException when observer is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var vm = new TestViewModel { Name = "Alice" };
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), x => ((TestViewModel)x).Name);

        var action = () => observable.Subscribe(null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that constructor throws ArgumentNullException when source is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullSource_ThrowsArgumentNullException()
    {
        var action = () => new PropertyChangingObservable<string>(null!, "Name", x => "test");

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that constructor throws ArgumentNullException when propertyName is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullPropertyName_ThrowsArgumentNullException()
    {
        var vm = new TestViewModel();
        var action = () => new PropertyChangingObservable<string>(vm, null!, x => "test");

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that constructor throws ArgumentNullException when getter is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Constructor_NullGetter_ThrowsArgumentNullException()
    {
        var vm = new TestViewModel();
        var action = () => new PropertyChangingObservable<string>(vm, "Name", null!);

        await Assert.That(action).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that Subscribe emits the current value immediately.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_EmitsCurrentValueImmediately()
    {
        var vm = new TestViewModel { Name = "Alice" };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), x => ((TestViewModel)x).Name);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo("Alice");
    }

    /// <summary>
    /// Verifies that PropertyChanging event triggers a value emission (old value).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanging_TriggersEmission()
    {
        var vm = new TestViewModel { Name = "Alice" };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), x => ((TestViewModel)x).Name);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));
        vm.Name = "Bob";

        // Should have 2 values: initial "Alice", and "Alice" again when PropertyChanging fired before it became "Bob"
        await Assert.That(results).Count().IsEqualTo(2);
        await Assert.That(results[0]).IsEqualTo("Alice");
        await Assert.That(results[1]).IsEqualTo("Alice");
    }

    /// <summary>
    /// Verifies that only matching property name triggers emission.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanging_WrongProperty_DoesNotTriggerEmission()
    {
        var vm = new TestViewModel { Name = "Alice", Age = 25 };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), x => ((TestViewModel)x).Name);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));
        vm.Age = 26;

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo("Alice");
    }

    /// <summary>
    /// Verifies that null/empty property name matches all properties.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanging_NullOrEmpty_TriggersEmission()
    {
        var vm = new ManualTestViewModel { Name = "Alice" };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, "Name", x => ((ManualTestViewModel)x).Name);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        vm.RaisePropertyChanging(string.Empty);
        await Assert.That(results).Count().IsEqualTo(2);

        vm.RaisePropertyChanging(null);
        await Assert.That(results).Count().IsEqualTo(3);
    }

    /// <summary>
    /// Verifies that disposal removes the event handler.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_RemovesEventHandler()
    {
        var vm = new TestViewModel { Name = "Alice" };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), x => ((TestViewModel)x).Name);

        var subscription = observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));
        subscription.Dispose();

        vm.Name = "Bob";
        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo("Alice");
    }

    /// <summary>
    /// Verifies that double dispose does not throw.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_CalledTwice_NoException()
    {
        var vm = new TestViewModel { Name = "Alice" };
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), x => ((TestViewModel)x).Name);

        var subscription = observable.Subscribe(new AnonymousObserver<string>(_ => { }, _ => { }, () => { }));
        subscription.Dispose();
        subscription.Dispose();

        await Assert.That(subscription.GetType()).IsNotNull();
    }

    /// <summary>
    /// Verifies that a PropertyChanging event fired after dispose does not throw (observer is null path).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChangingAfterDispose_DoesNotThrow()
    {
        var vm = new ManualTestViewModel { Name = "Alice" };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, "Name", x => ((ManualTestViewModel)x).Name);

        var subscription = observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));
        subscription.Dispose();

        // Fire event after dispose - should be safely ignored
        vm.RaisePropertyChanging("Name");

        // Only the initial value should have been emitted
        await Assert.That(results).Count().IsEqualTo(1);
    }

    private sealed class ManualTestViewModel : INotifyPropertyChanging
    {
        public event PropertyChangingEventHandler? PropertyChanging;

        public string Name { get; set; } = string.Empty;

        public void RaisePropertyChanging(string? propertyName) =>
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
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
