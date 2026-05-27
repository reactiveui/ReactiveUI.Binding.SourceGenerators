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
    /// The initial name value assigned to the view model under test.
    /// </summary>
    private const string InitialName = "Alice";

    /// <summary>
    /// The expected number of emissions when the initial value plus one change are observed.
    /// </summary>
    private const int ExpectedTwoEmissions = 2;

    /// <summary>
    /// The expected number of emissions after a third notification.
    /// </summary>
    private const int ExpectedThreeEmissions = 3;

    /// <summary>
    /// A sample age value assigned to an unrelated property.
    /// </summary>
    private const int SampleAge = 26;

    /// <summary>
    /// The number of rapid property-changing iterations in the concurrency test.
    /// </summary>
    private const int ConcurrencyIterations = 100;

    /// <summary>
    /// Verifies that Subscribe throws ArgumentNullException when observer is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Subscribe_NullObserver_ThrowsArgumentNullException()
    {
        var vm = new TestViewModel { Name = InitialName };
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
        var vm = new TestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), x => ((TestViewModel)x).Name);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(InitialName);
    }

    /// <summary>
    /// Verifies that PropertyChanging event triggers a value emission (old value).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanging_TriggersEmission()
    {
        var vm = new TestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), x => ((TestViewModel)x).Name);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));
        vm.Name = "Bob";

        // Should have 2 values: initial "Alice", and "Alice" again when PropertyChanging fired before it became "Bob"
        await Assert.That(results).Count().IsEqualTo(ExpectedTwoEmissions);
        await Assert.That(results[0]).IsEqualTo(InitialName);
        await Assert.That(results[1]).IsEqualTo(InitialName);
    }

    /// <summary>
    /// Verifies that only matching property name triggers emission.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanging_WrongProperty_DoesNotTriggerEmission()
    {
        var vm = new TestViewModel { Name = InitialName, Age = 25 };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), x => ((TestViewModel)x).Name);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));
        vm.Age = SampleAge;

        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(InitialName);
    }

    /// <summary>
    /// Verifies that null/empty property name matches all properties.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanging_NullOrEmpty_TriggersEmission()
    {
        var vm = new ManualTestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, "Name", x => ((ManualTestViewModel)x).Name);

        observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        vm.RaisePropertyChanging(string.Empty);
        await Assert.That(results).Count().IsEqualTo(ExpectedTwoEmissions);

        vm.RaisePropertyChanging(null);
        await Assert.That(results).Count().IsEqualTo(ExpectedThreeEmissions);
    }

    /// <summary>
    /// Verifies that disposal removes the event handler.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_RemovesEventHandler()
    {
        var vm = new TestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, nameof(vm.Name), x => ((TestViewModel)x).Name);

        var subscription = observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));
        subscription.Dispose();

        vm.Name = "Bob";
        await Assert.That(results).Count().IsEqualTo(1);
        await Assert.That(results[0]).IsEqualTo(InitialName);
    }

    /// <summary>
    /// Verifies that double dispose does not throw.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Dispose_CalledTwice_NoException()
    {
        var vm = new TestViewModel { Name = InitialName };
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
        var vm = new ManualTestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, "Name", x => ((ManualTestViewModel)x).Name);

        var subscription = observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));
        subscription.Dispose();

        // Fire event after dispose - should be safely ignored
        vm.RaisePropertyChanging("Name");

        // Only the initial value should have been emitted
        await Assert.That(results).Count().IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that the <c>if (observer is null) { return; }</c> guard in OnPropertyChanging
    /// is exercised by nulling the observer via <see cref="PropertyChangingObservable{T}.Subscription.TrySetDisposed"/>
    /// without calling Dispose (which would unregister the handler). This deterministically covers the
    /// race-condition guard path.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task OnPropertyChanging_ObserverNulledWithoutUnregistering_DoesNotThrow()
    {
        var vm = new ManualTestViewModel { Name = InitialName };
        var results = new List<string>();
        var observable = new PropertyChangingObservable<string>(vm, "Name", x => ((ManualTestViewModel)x).Name);

        var subscription = observable.Subscribe(new AnonymousObserver<string>(results.Add, _ => { }, () => { }));

        // Null the observer WITHOUT calling Dispose so the PropertyChanging event handler remains
        // registered — when the event fires, OnPropertyChanging runs with a null observer,
        // deterministically hitting the null-guard branch. TrySetDisposed performs exactly the
        // atomic null-out that Dispose's first step does, minus the handler unregistration.
        ((PropertyChangingObservable<string>.Subscription)subscription).TrySetDisposed();

        var action = () => vm.RaisePropertyChanging("Name");
        await Assert.That(action).ThrowsNothing();

        // Only the initial value should have been emitted; no second value after null-observer.
        await Assert.That(results).Count().IsEqualTo(1);

        // Cleanup: Dispose will see _observer already null and skip unregistration.
        subscription.Dispose();
    }

    /// <summary>
    /// Verifies that the observer-is-null branch in OnPropertyChanging is safely handled when
    /// PropertyChanging fires concurrently with disposal on another thread.
    /// Covers PropertyChangingObservable line 86 (observer is null race-condition guard).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task PropertyChanging_ConcurrentDispose_ObserverNullGuardDoesNotThrow()
    {
        var vm = new ConcurrentChangingTestViewModel();
        var observable =
            new PropertyChangingObservable<string>(vm, "Name", x => ((ConcurrentChangingTestViewModel)x).Name);
        var results = new List<string>();

        var subscription = observable.Subscribe(new AnonymousObserver<string>(
            results.Add,
            _ => { },
            () => { }));

        // Start a background task that rapidly disposes while property changes occur
        var disposed = false;
        var disposeTask = Task.Run(async () =>
        {
            await Task.Delay(1);
            subscription.Dispose();
            disposed = true;
        });

        // Rapidly raise property changing events
        for (var i = 0; i < ConcurrencyIterations && !disposed; i++)
        {
            vm.Name = $"Value{i}";
            vm.RaisePropertyChanging("Name");
        }

        await disposeTask;

        // The test succeeds if no exception was thrown during concurrent dispose + property change
        await Assert.That(results).Count().IsGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// A test view model that implements <see cref="INotifyPropertyChanging"/> for concurrent disposal tests.
    /// </summary>
    private sealed class ConcurrentChangingTestViewModel : INotifyPropertyChanging
    {
        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = InitialName;

        /// <summary>
        /// Raises the <see cref="PropertyChanging"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that is changing.</param>
        public void RaisePropertyChanging(string? propertyName) =>
            PropertyChanging?.Invoke(this, new(propertyName));
    }

    /// <summary>
    /// A test view model that implements <see cref="INotifyPropertyChanging"/> with manual event raising.
    /// </summary>
    private sealed class ManualTestViewModel : INotifyPropertyChanging
    {
        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Raises the <see cref="PropertyChanging"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that is changing.</param>
        public void RaisePropertyChanging(string? propertyName) =>
            PropertyChanging?.Invoke(this, new(propertyName));
    }

    /// <summary>
    /// A simple observer that delegates to provided actions.
    /// </summary>
    /// <typeparam name="T">The type of elements observed.</typeparam>
    /// <param name="onNext">The action to invoke for each element.</param>
    /// <param name="onError">The action to invoke on error.</param>
    /// <param name="onCompleted">The action to invoke on completion.</param>
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
