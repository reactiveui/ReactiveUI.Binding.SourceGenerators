// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Observables;

/// <summary>
/// Unit tests for <see cref="PropertyObservable{T}"/>.
/// </summary>
public class PropertyObservableTests
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
        var observable = new PropertyObservable<string>(vm, nameof(vm.Name), x => ((TestViewModel)x).Name, false);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo("Alice");
    }

    /// <summary>
    /// Verifies that PropertyChanged event triggers a new value emission.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanged_TriggersEmission()
    {
        var vm = new TestViewModel { Name = "Alice" };
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, nameof(vm.Name), x => ((TestViewModel)x).Name, false);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));
        vm.Name = "Bob";

        await Assert.That(results).Count().IsEqualTo(2);
        await Assert.That(results[1]).IsEqualTo("Bob");
    }

    /// <summary>
    /// Verifies that only matching property name triggers emission.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanged_WrongProperty_DoesNotTriggerEmission()
    {
        var vm = new TestViewModel { Name = "Alice", Age = 25 };
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, nameof(vm.Name), x => ((TestViewModel)x).Name, false);

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
    public async Task PropertyChanged_NullOrEmpty_TriggersEmission()
    {
        var vm = new ManualTestViewModel { Name = "Alice" };
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, "Name", x => ((ManualTestViewModel)x).Name, false);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        vm.RaisePropertyChanged(string.Empty);
        await Assert.That(results).Count().IsEqualTo(2);

        vm.RaisePropertyChanged(null);
        await Assert.That(results).Count().IsEqualTo(3);
    }

    /// <summary>
    /// Verifies that distinctUntilChanged=true suppresses duplicate values.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DistinctUntilChanged_True_SuppressesDuplicates()
    {
        var vm = new ManualTestViewModel { Name = "Alice" };
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, "Name", x => ((ManualTestViewModel)x).Name, true);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));
        vm.Name = "Alice"; // Same value
        vm.RaisePropertyChanged("Name");

        await Assert.That(results).Count().IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that distinctUntilChanged=false allows duplicate values.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task DistinctUntilChanged_False_AllowsDuplicates()
    {
        var vm = new ManualTestViewModel { Name = "Alice" };
        var results = new List<string>();
        var observable = new PropertyObservable<string>(vm, "Name", x => ((ManualTestViewModel)x).Name, false);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));
        vm.Name = "Alice"; // Same value
        vm.RaisePropertyChanged("Name");

        await Assert.That(results).Count().IsEqualTo(2);
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
        var observable = new PropertyObservable<string>(vm, nameof(vm.Name), x => ((TestViewModel)x).Name, false);

        var subscription = observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));
        subscription.Dispose();

        vm.Name = "Bob";
        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo("Alice");
    }

    private sealed class ManualTestViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; set; } = string.Empty;

        public void RaisePropertyChanged(string? propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
