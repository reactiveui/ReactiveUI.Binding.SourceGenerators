// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Reactive.Linq;

using ReactiveUI.Binding.Expressions;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.ObservableForProperty;

/// <summary>
/// Tests for the <see cref="INPCObservableForProperty"/> class.
/// </summary>
public class INPCObservableForPropertyTests
{
    /// <summary>
    /// Verifies affinity is 5 for types implementing INotifyPropertyChanged.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_INPCType_Returns5()
    {
        var sut = new INPCObservableForProperty();

        var affinity = sut.GetAffinityForObject(typeof(TestViewModel), "Name", beforeChanged: false);

        await Assert.That(affinity).IsEqualTo(5);
    }

    /// <summary>
    /// Verifies affinity is 5 for INotifyPropertyChanging when beforeChanged is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_INPCChangingType_Returns5ForBeforeChanged()
    {
        var sut = new INPCObservableForProperty();

        var affinity = sut.GetAffinityForObject(typeof(TestViewModel), "Name", beforeChanged: true);

        await Assert.That(affinity).IsEqualTo(5);
    }

    /// <summary>
    /// Verifies affinity is 0 for POCO types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_PocoType_Returns0()
    {
        var sut = new INPCObservableForProperty();

        var affinity = sut.GetAffinityForObject(typeof(PocoModel), "Value", beforeChanged: false);

        await Assert.That(affinity).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that GetNotificationForProperty emits when a property changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_PropertyChanged_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new TestViewModel();

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, body, "Name")
            .Subscribe(_ => emitted = true);

        vm.Name = "Alice";

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>
    /// Verifies that GetNotificationForProperty emits before-change notifications.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_BeforeChanged_EmitsNotification()
    {
        var sut = new INPCObservableForProperty();
        var vm = new TestViewModel();

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, body, "Name", beforeChanged: true)
            .Subscribe(_ => emitted = true);

        vm.Name = "Alice";

        await Assert.That(emitted).IsTrue();
    }

    /// <summary>
    /// Verifies that GetNotificationForProperty filters by property name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotificationForProperty_DifferentProperty_DoesNotEmit()
    {
        var sut = new INPCObservableForProperty();
        var vm = new TestViewModel();

        Expression<Func<TestViewModel, string>> expr = x => x.Name;
        var body = Reflection.Rewrite(expr.Body);

        var emitted = false;
        using var sub = sut.GetNotificationForProperty(vm, body, "Name")
            .Subscribe(_ => emitted = true);

        // Change Age, not Name
        vm.Age = 30;

        await Assert.That(emitted).IsFalse();
    }
}
