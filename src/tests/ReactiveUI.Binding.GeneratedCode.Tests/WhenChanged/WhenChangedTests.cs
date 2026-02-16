// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenChanged;

/// <summary>
/// Tests that the source-generator-generated WhenChanged code works correctly at runtime.
/// </summary>
public class WhenChangedTests
{
    /// <summary>
    /// Verifies that a single-property WhenChanged emits the initial value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsInitialValue()
    {
        var vm = new TestViewModel { Name = "Initial" };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Initial");
    }

    /// <summary>
    /// Verifies that a single-property WhenChanged emits when the property changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsOnChange()
    {
        var vm = new TestViewModel { Name = "Initial" };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        vm.Name = "Changed";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values).Contains("Changed");
    }

    /// <summary>
    /// Verifies that a single-property WhenChanged emits all sequential changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_EmitsSequentialChanges()
    {
        var vm = new TestViewModel { Name = "A" };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        vm.Name = "B";
        vm.Name = "C";
        vm.Name = "D";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(4);
        await Assert.That(values[0]).IsEqualTo("A");
        await Assert.That(values[1]).IsEqualTo("B");
        await Assert.That(values[2]).IsEqualTo("C");
        await Assert.That(values[3]).IsEqualTo("D");
    }

    /// <summary>
    /// Verifies that a two-property WhenChanged emits the initial tuple.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_EmitsInitialTuple()
    {
        var vm = new BigViewModel { Prop1 = "Hello", Prop2 = 42 };
        var values = new List<(string property1, int property2)>();

        using var sub = WhenChangedScenarios.TwoProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].property1).IsEqualTo("Hello");
        await Assert.That(values[0].property2).IsEqualTo(42);
    }

    /// <summary>
    /// Verifies that a two-property WhenChanged emits when either property changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_EmitsOnEitherChange()
    {
        var vm = new BigViewModel { Prop1 = "A", Prop2 = 1 };
        var values = new List<(string property1, int property2)>();

        using var sub = WhenChangedScenarios.TwoProperties(vm)
            .Subscribe(values.Add);

        vm.Prop1 = "B";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values[^1].property1).IsEqualTo("B");
        await Assert.That(values[^1].property2).IsEqualTo(1);

        vm.Prop2 = 2;

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(3);
        await Assert.That(values[^1].property1).IsEqualTo("B");
        await Assert.That(values[^1].property2).IsEqualTo(2);
    }

    /// <summary>
    /// Verifies that three-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThreeProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "X", Prop2 = 10, Prop3 = 3.14 };
        var values = new List<(string property1, int property2, double property3)>();

        using var sub = WhenChangedScenarios.ThreeProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].property1).IsEqualTo("X");
        await Assert.That(values[0].property2).IsEqualTo(10);
        await Assert.That(values[0].property3).IsEqualTo(3.14);
    }

    /// <summary>
    /// Verifies that four-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "Y", Prop2 = 20, Prop3 = 2.71, Prop4 = true };
        var values = new List<(string property1, int property2, double property3, bool property4)>();

        using var sub = WhenChangedScenarios.FourProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].property1).IsEqualTo("Y");
        await Assert.That(values[0].property2).IsEqualTo(20);
        await Assert.That(values[0].property3).IsEqualTo(2.71);
        await Assert.That(values[0].property4).IsEqualTo(true);
    }

    /// <summary>
    /// Verifies that WhenChanged with a selector combines property values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSelector_CombinesValues()
    {
        var vm = new BigViewModel { Prop1 = "Hello", Prop2 = 42 };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.WithSelector_TwoProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Hello_42");
    }

    /// <summary>
    /// Verifies that deep chain WhenChanged emits the nested property value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_EmitsNestedPropertyValue()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Seattle");
    }

    /// <summary>
    /// Verifies that deep chain WhenChanged emits when the nested property changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_EmitsOnNestedPropertyChange()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        vm.Address.City = "Portland";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values).Contains("Portland");
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

        var sub = WhenChangedScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        sub.Dispose();

        vm.Name = "AfterDisposal";

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Initial");
    }
}
