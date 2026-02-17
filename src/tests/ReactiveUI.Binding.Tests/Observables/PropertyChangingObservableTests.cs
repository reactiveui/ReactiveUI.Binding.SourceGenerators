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
