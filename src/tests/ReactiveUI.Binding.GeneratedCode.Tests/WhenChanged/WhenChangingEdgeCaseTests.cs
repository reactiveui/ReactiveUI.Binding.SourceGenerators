// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenChanged;

/// <summary>
/// Edge case tests for WhenChanging covering deep chains, multi-property change emission,
/// and disposal scenarios.
/// </summary>
public class WhenChangingEdgeCaseTests
{
    /// <summary>
    /// Verifies that deep chain WhenChanging emits the initial nested property value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_EmitsInitialValue()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var values = new List<string>();

        using var sub = WhenChangingScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Seattle");
    }

    /// <summary>
    /// Verifies that deep chain WhenChanging emits before a nested property changes.
    /// The emitted value is the old value before the change.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_EmitsBeforeNestedChange()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var values = new List<string>();

        using var sub = WhenChangingScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        vm.Address.City = "Portland";

        // Should have emitted "Seattle" (initial) and "Seattle" again (before change to Portland)
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values[0]).IsEqualTo("Seattle");
        await Assert.That(values[1]).IsEqualTo("Seattle");
    }

    /// <summary>
    /// Verifies that WhenChanging two-property emits when either property is about to change.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_EmitsOnEitherChange()
    {
        var vm = new BigViewModel { Prop1 = "A", Prop2 = 1 };
        var values = new List<(string property1, int property2)>();

        using var sub = WhenChangingScenarios.TwoProperties(vm)
            .Subscribe(values.Add);

        // Initial emission
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].property1).IsEqualTo("A");
        await Assert.That(values[0].property2).IsEqualTo(1);

        // Change Prop1 — should emit before-change values
        vm.Prop1 = "B";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);

        // The before-change value for Prop1 should still be "A"
        await Assert.That(values[1].property1).IsEqualTo("A");
        await Assert.That(values[1].property2).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that disposing the WhenChanging deep chain subscription stops listening.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_Disposal_StopsListening()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var values = new List<string>();

        var sub = WhenChangingScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        sub.Dispose();

        vm.Address.City = "Portland";

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Seattle");
    }

    /// <summary>
    /// Verifies that WhenChanging multi-property emits sequential before-change values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_SequentialChanges()
    {
        var vm = new BigViewModel { Prop1 = "A", Prop2 = 1 };
        var values = new List<(string property1, int property2)>();

        using var sub = WhenChangingScenarios.TwoProperties(vm)
            .Subscribe(values.Add);

        vm.Prop1 = "B";
        vm.Prop2 = 2;

        // Should have at least 3 emissions: initial + 2 changes
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(3);
    }
}
