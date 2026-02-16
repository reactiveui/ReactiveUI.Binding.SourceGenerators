// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenChanged;

/// <summary>
/// Edge case tests for WhenChanged covering deep chain intermediate replacement,
/// DistinctUntilChanged behavior, and multi-property with deep chains.
/// </summary>
public class WhenChangedEdgeCaseTests
{
    /// <summary>
    /// Verifies that replacing the intermediate object in a deep chain re-subscribes
    /// and emits the new object's property value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_IntermediateObjectReplacement_ReSubscribes()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        await Assert.That(values[0]).IsEqualTo("Seattle");

        // Replace the entire Address object
        var newAddress = new Address { City = "Portland" };
        vm.Address = newAddress;

        await Assert.That(values).Contains("Portland");

        // Change the new address's City
        newAddress.City = "Eugene";

        await Assert.That(values).Contains("Eugene");
    }

    /// <summary>
    /// Verifies that after replacing the intermediate object, changes to the old
    /// object's property are no longer observed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_IntermediateObjectReplacement_UnsubscribesOld()
    {
        var vm = new BigViewModel();
        vm.Address.Street = "First Ave";
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_AddressStreet(vm)
            .Subscribe(values.Add);

        var oldAddress = vm.Address;

        // Replace the Address object
        vm.Address = new Address { Street = "Second Ave" };

        var countAfterReplacement = values.Count;

        // Change the old address — should NOT emit
        oldAddress.Street = "Third Ave";

        await Assert.That(values.Count).IsEqualTo(countAfterReplacement);
        await Assert.That(values[^1]).IsEqualTo("Second Ave");
    }

    /// <summary>
    /// Verifies that DistinctUntilChanged filters out duplicate consecutive values
    /// for single-property observation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_DistinctUntilChanged_FiltersDuplicates()
    {
        var vm = new TestViewModel { Name = "A" };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.SingleProperty_Name(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("A");

        // Setting to the same value — TestViewModel's setter checks inequality,
        // so PropertyChanged won't fire. This verifies no extra emission.
        vm.Name = "A";

        await Assert.That(values.Count).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that DistinctUntilChanged filters out duplicate consecutive values
    /// in deep chain observation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_DistinctUntilChanged_FiltersDuplicates()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Seattle");

        // Replace Address with one that has the same City — DistinctUntilChanged should filter
        vm.Address = new Address { City = "Seattle" };

        await Assert.That(values.Count).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that multi-property observation with a deep chain emits when
    /// the nested property changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_WithDeepChain_EmitsOnNestedChange()
    {
        var vm = new BigViewModel { Prop1 = "Hello" };
        vm.Address.City = "Seattle";
        var values = new List<(string city, string prop1)>();

        using var sub = WhenChangedScenarios.MultiProperty_WithDeepChain(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].city).IsEqualTo("Seattle");
        await Assert.That(values[0].prop1).IsEqualTo("Hello");

        // Change the nested property
        vm.Address.City = "Portland";

        await Assert.That(values[^1].city).IsEqualTo("Portland");
        await Assert.That(values[^1].prop1).IsEqualTo("Hello");
    }

    /// <summary>
    /// Verifies that multi-property observation with a deep chain emits when
    /// the simple property changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_WithDeepChain_EmitsOnSimpleChange()
    {
        var vm = new BigViewModel { Prop1 = "Hello" };
        vm.Address.City = "Seattle";
        var values = new List<(string city, string prop1)>();

        using var sub = WhenChangedScenarios.MultiProperty_WithDeepChain(vm)
            .Subscribe(values.Add);

        // Change the simple property
        vm.Prop1 = "World";

        await Assert.That(values[^1].city).IsEqualTo("Seattle");
        await Assert.That(values[^1].prop1).IsEqualTo("World");
    }

    /// <summary>
    /// Verifies that multi-property observation with a deep chain re-subscribes
    /// when the intermediate object is replaced.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultiProperty_WithDeepChain_IntermediateReplacement()
    {
        var vm = new BigViewModel { Prop1 = "Hello" };
        vm.Address.City = "Seattle";
        var values = new List<(string city, string prop1)>();

        using var sub = WhenChangedScenarios.MultiProperty_WithDeepChain(vm)
            .Subscribe(values.Add);

        // Replace the intermediate object
        vm.Address = new Address { City = "Portland" };

        await Assert.That(values[^1].city).IsEqualTo("Portland");

        // Change the new object's property
        vm.Address.City = "Eugene";

        await Assert.That(values[^1].city).IsEqualTo("Eugene");
    }

    /// <summary>
    /// Verifies that multiple subscriptions to the same WhenChanged observable
    /// each receive independent emissions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleSubscriptions_ReceiveIndependentEmissions()
    {
        var vm = new TestViewModel { Name = "Initial" };
        var values1 = new List<string>();
        var values2 = new List<string>();

        var obs = WhenChangedScenarios.SingleProperty_Name(vm);

        using var sub1 = obs.Subscribe(values1.Add);
        using var sub2 = obs.Subscribe(values2.Add);

        vm.Name = "Changed";

        await Assert.That(values1.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values2.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values1).Contains("Changed");
        await Assert.That(values2).Contains("Changed");
    }

    /// <summary>
    /// Verifies that disposing one subscription does not affect another.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleSubscriptions_DisposingOneDoesNotAffectOther()
    {
        var vm = new TestViewModel { Name = "Initial" };
        var values1 = new List<string>();
        var values2 = new List<string>();

        var obs = WhenChangedScenarios.SingleProperty_Name(vm);

        var sub1 = obs.Subscribe(values1.Add);
        using var sub2 = obs.Subscribe(values2.Add);

        sub1.Dispose();

        vm.Name = "AfterDisposal";

        await Assert.That(values1).DoesNotContain("AfterDisposal");
        await Assert.That(values2).Contains("AfterDisposal");
    }

    /// <summary>
    /// Verifies that the deep chain disposal stops listening for nested property changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_Disposal_StopsListening()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var values = new List<string>();

        var sub = WhenChangedScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        sub.Dispose();

        vm.Address.City = "Portland";

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Seattle");
    }

    /// <summary>
    /// Verifies that the deep chain disposal also stops listening after intermediate replacement.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_Disposal_AfterIntermediateReplacement()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var values = new List<string>();

        var sub = WhenChangedScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        vm.Address = new Address { City = "Portland" };
        sub.Dispose();

        vm.Address.City = "Eugene";

        await Assert.That(values).DoesNotContain("Eugene");
    }

    /// <summary>
    /// Verifies that the int property WhenChanged emits the initial value and changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleProperty_IntType_EmitsChanges()
    {
        var vm = new TestViewModel { Age = 25 };
        var values = new List<int>();

        using var sub = WhenChangedScenarios.SingleProperty_Age(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(25);

        vm.Age = 30;

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values).Contains(30);
    }

    /// <summary>
    /// Verifies that deep chain with null-forgiving operator (x => x.Child!.Name)
    /// correctly observes the nested property when the intermediate is non-null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_NullForgiving_EmitsWhenChildSet()
    {
        var host = new HostTestFixture
        {
            Child = new TestViewModel { Name = "Alice" },
        };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_ChildName(host)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Alice");

        host.Child!.Name = "Bob";

        await Assert.That(values).Contains("Bob");
    }

    /// <summary>
    /// Verifies that deep chain with null-forgiving operator emits default
    /// when the intermediate object is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_NullForgiving_EmitsDefaultWhenChildNull()
    {
        var host = new HostTestFixture { Child = null };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_ChildName(host)
            .Subscribe(values.Add);

        // When Child is null, should emit default (null for string)
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Verifies that deep chain with null-forgiving operator re-subscribes
    /// when the intermediate object is replaced.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_NullForgiving_ReSubscribesOnReplacement()
    {
        var host = new HostTestFixture
        {
            Child = new TestViewModel { Name = "Alice" },
        };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_ChildName(host)
            .Subscribe(values.Add);

        // Replace the child entirely
        host.Child = new TestViewModel { Name = "Charlie" };

        await Assert.That(values).Contains("Charlie");

        // Modify the new child
        host.Child.Name = "Dave";

        await Assert.That(values).Contains("Dave");
    }

    /// <summary>
    /// Verifies that deep chain with null-forgiving operator transitions
    /// from null to non-null and starts observing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_NullForgiving_NullToNonNull()
    {
        var host = new HostTestFixture { Child = null };
        var values = new List<string>();

        using var sub = WhenChangedScenarios.DeepChain_ChildName(host)
            .Subscribe(values.Add);

        // Set child from null to a value
        host.Child = new TestViewModel { Name = "Eve" };

        await Assert.That(values).Contains("Eve");

        // Verify further changes are tracked
        host.Child.Name = "Frank";

        await Assert.That(values).Contains("Frank");
    }
}
