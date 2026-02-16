// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenChanged;

/// <summary>
/// Tests that the source-generator-generated WhenChanging code works correctly at runtime.
/// WhenChanging emits the property value before the change is applied.
/// </summary>
public class WhenChangingTests
{
    /// <summary>
    /// Verifies that a single-property WhenChanging emits the initial value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsInitialValue()
    {
        var vm = new TestViewModel { Name = "Initial" };
        var values = new List<string>();

        using var sub = WhenChangingScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Initial");
    }

    /// <summary>
    /// Verifies that a single-property WhenChanging emits when the property is about to change.
    /// The emitted value is the value before the change (the old value).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsBeforeChange()
    {
        var vm = new TestViewModel { Name = "Before" };
        var values = new List<string>();

        using var sub = WhenChangingScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        vm.Name = "After";

        // WhenChanging emits the value at the time of PropertyChanging event,
        // which is the old value (before the assignment).
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values[0]).IsEqualTo("Before");
        await Assert.That(values[1]).IsEqualTo("Before");
    }

    /// <summary>
    /// Verifies that a single-property WhenChanging emits sequential before-change values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsSequentialBeforeChangeValues()
    {
        var vm = new TestViewModel { Name = "A" };
        var values = new List<string>();

        using var sub = WhenChangingScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        vm.Name = "B";
        vm.Name = "C";

        // Initial "A", then "A" before change to "B", then "B" before change to "C"
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(3);
        await Assert.That(values[0]).IsEqualTo("A");
        await Assert.That(values[1]).IsEqualTo("A");
        await Assert.That(values[2]).IsEqualTo("B");
    }

    /// <summary>
    /// Verifies that two-property WhenChanging emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_EmitsInitialTuple()
    {
        var vm = new BigViewModel { Prop1 = "Hello", Prop2 = 42 };
        var values = new List<(string property1, int property2)>();

        using var sub = WhenChangingScenarios.TwoProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].property1).IsEqualTo("Hello");
        await Assert.That(values[0].property2).IsEqualTo(42);
    }

    /// <summary>
    /// Verifies that three-property WhenChanging emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThreeProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "X", Prop2 = 10, Prop3 = 3.14 };
        var values = new List<(string property1, int property2, double property3)>();

        using var sub = WhenChangingScenarios.ThreeProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].property1).IsEqualTo("X");
        await Assert.That(values[0].property2).IsEqualTo(10);
        await Assert.That(values[0].property3).IsEqualTo(3.14);
    }

    /// <summary>
    /// Verifies that four-property WhenChanging emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "Y", Prop2 = 20, Prop3 = 2.71, Prop4 = true };
        var values = new List<(string property1, int property2, double property3, bool property4)>();

        using var sub = WhenChangingScenarios.FourProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].property1).IsEqualTo("Y");
        await Assert.That(values[0].property2).IsEqualTo(20);
        await Assert.That(values[0].property3).IsEqualTo(2.71);
        await Assert.That(values[0].property4).IsEqualTo(true);
    }

    /// <summary>
    /// Verifies that disposing the subscription stops listening for changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Disposal_StopsListening()
    {
        var vm = new TestViewModel { Name = "Initial" };
        var values = new List<string>();

        var sub = WhenChangingScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        sub.Dispose();

        vm.Name = "AfterDisposal";

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Initial");
    }
}
