// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenAnyValue;

/// <summary>
/// Edge case tests for WhenAnyValue covering disposal, deep chains, and multi-property
/// change emission scenarios.
/// </summary>
public class WhenAnyValueEdgeCaseTests
{
    /// <summary>
    /// Verifies that disposing the WhenAnyValue subscription stops listening for changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Disposal_StopsListening()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "Initial" };
        var values = new List<string>();

        var sub = WhenAnyValueScenarios.SingleProperty(fixture)
            .Subscribe(values.Add);

        sub.Dispose();

        fixture.Value1 = "AfterDisposal";

        await Assert.That(values.Count).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Initial");
    }

    /// <summary>
    /// Verifies that multi-property WhenAnyValue emits when either property changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwoProperties_EmitsOnEitherChange()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "A", Value2 = "B" };
        var values = new List<(string property1, string property2)>();

        using var sub = WhenAnyValueScenarios.TwoProperties(fixture)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);

        fixture.Value1 = "C";

        await Assert.That(values[^1].property1).IsEqualTo("C");
        await Assert.That(values[^1].property2).IsEqualTo("B");

        fixture.Value2 = "D";

        await Assert.That(values[^1].property1).IsEqualTo("C");
        await Assert.That(values[^1].property2).IsEqualTo("D");
    }

    /// <summary>
    /// Verifies that deep chain WhenAnyValue emits the nested property value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_EmitsNestedPropertyValue()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsEqualTo("Seattle");
    }

    /// <summary>
    /// Verifies that deep chain WhenAnyValue emits on nested property change.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_EmitsOnNestedPropertyChange()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        vm.Address.City = "Portland";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values).Contains("Portland");
    }

    /// <summary>
    /// Verifies that deep chain WhenAnyValue re-subscribes on intermediate object replacement.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DeepChain_IntermediateObjectReplacement()
    {
        var vm = new BigViewModel();
        vm.Address.City = "Seattle";
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.DeepChain_AddressCity(vm)
            .Subscribe(values.Add);

        var newAddress = new Address { City = "Portland" };
        vm.Address = newAddress;

        await Assert.That(values).Contains("Portland");

        // Verify resubscription
        newAddress.City = "Eugene";

        await Assert.That(values).Contains("Eugene");
    }

    /// <summary>
    /// Verifies that WhenAnyValue with selector re-emits when properties change.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSelector_EmitsOnChange()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "Hello", Value2 = "World" };
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.WithSelector_TwoProperties(fixture)
            .Subscribe(values.Add);

        await Assert.That(values[0]).IsEqualTo("Hello_World");

        fixture.Value1 = "Goodbye";

        await Assert.That(values[^1]).IsEqualTo("Goodbye_World");
    }

    /// <summary>
    /// Verifies that rapid sequential property changes are all captured by WhenAnyValue.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RapidSequentialChanges_AllCaptured()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "A" };
        var values = new List<string>();

        using var sub = WhenAnyValueScenarios.SingleProperty(fixture)
            .Subscribe(values.Add);

        for (var i = 0; i < 100; i++)
        {
            fixture.Value1 = $"Value_{i}";
        }

        await Assert.That(values.Count).IsEqualTo(101); // initial + 100 changes
        await Assert.That(values[0]).IsEqualTo("A");
        await Assert.That(values[^1]).IsEqualTo("Value_99");
    }

    /// <summary>
    /// Verifies that multiple subscriptions to the same WhenAnyValue observable
    /// each receive independent emissions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MultipleSubscriptions_ReceiveIndependentEmissions()
    {
        var fixture = new WhenAnyTestFixture { Value1 = "Initial" };
        var values1 = new List<string>();
        var values2 = new List<string>();

        var obs = WhenAnyValueScenarios.SingleProperty(fixture);

        using var sub1 = obs.Subscribe(values1.Add);
        using var sub2 = obs.Subscribe(values2.Add);

        fixture.Value1 = "Changed";

        await Assert.That(values1.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values2.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values1).Contains("Changed");
        await Assert.That(values2).Contains("Changed");
    }

    /// <summary>
    /// Verifies that twelve-property WhenAnyValue fires when any one of the 12 properties changes.
    /// This is a parity test matching ReactiveUI's exhaustive WhenAnyValue coverage.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwelveProperties_EachPropertyFiresIndependently()
    {
        var fixture = new WhenAnyTestFixture();
        var count = 0;

        using var sub = WhenAnyValueExtendedScenarios.TwelveProperties_AllStrings(fixture)
            .Subscribe(_ => count++);

        // Initial emission
        await Assert.That(count).IsEqualTo(1);

        // Change each of the 12 properties — each should fire
        fixture.Value1 = "a";
        fixture.Value2 = "b";
        fixture.Value3 = "c";
        fixture.Value4 = "d";
        fixture.Value5 = "e";
        fixture.Value6 = "f";
        fixture.Value7 = "g";
        fixture.Value8 = "h";
        fixture.Value9 = "i";
        fixture.Value10 = "j";
        fixture.Value11 = "k";
        fixture.Value12 = "l";

        await Assert.That(count).IsEqualTo(13); // initial + 12 changes
    }
}
