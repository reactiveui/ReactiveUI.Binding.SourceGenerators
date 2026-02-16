// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>
/// Edge case tests for BindOneWay and BindTwoWay covering deep chain source properties,
/// multiple bindings to the same property, and rapid bidirectional changes.
/// </summary>
public class BindingEdgeCaseTests
{
    /// <summary>
    /// Verifies that BindOneWay with a deep chain source property syncs the initial value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_DeepChainSource_SyncsInitialValue()
    {
        var source = new BigViewModel();
        source.Address.City = "Seattle";
        var target = new BigView();

        using var binding = BindOneWayScenarios.DeepChainProperty(source, target);

        await Assert.That(target.ViewProp1).IsEqualTo("Seattle");
    }

    /// <summary>
    /// Verifies that BindOneWay with a deep chain source property syncs nested changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_DeepChainSource_SyncsNestedChanges()
    {
        var source = new BigViewModel();
        source.Address.City = "Seattle";
        var target = new BigView();

        using var binding = BindOneWayScenarios.DeepChainProperty(source, target);

        source.Address.City = "Portland";

        await Assert.That(target.ViewProp1).IsEqualTo("Portland");
    }

    /// <summary>
    /// Verifies that BindOneWay with a deep chain re-subscribes on intermediate replacement.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_DeepChainSource_IntermediateReplacement()
    {
        var source = new BigViewModel();
        source.Address.City = "Seattle";
        var target = new BigView();

        using var binding = BindOneWayScenarios.DeepChainProperty(source, target);

        source.Address = new Address { City = "Portland" };

        await Assert.That(target.ViewProp1).IsEqualTo("Portland");

        // Further changes on the new address
        source.Address.City = "Eugene";

        await Assert.That(target.ViewProp1).IsEqualTo("Eugene");
    }

    /// <summary>
    /// Verifies that multiple BindOneWay bindings to the same target property
    /// both function correctly (last write wins).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_MultipleBindingsToSameTarget()
    {
        var source = new BigViewModel { Prop1 = "From1", Prop5 = "From5" };
        var target = new BigView();

        // Both bind to the same target: ViewProp1 and ViewProp5
        using var binding1 = BindOneWayScenarios.StringProperty(source, target);
        using var binding2 = BindOneWayScenarios.StringProperty5(source, target);

        // Each binding independently syncs its own target
        await Assert.That(target.ViewProp1).IsEqualTo("From1");
        await Assert.That(target.ViewProp5).IsEqualTo("From5");

        source.Prop1 = "Updated1";
        await Assert.That(target.ViewProp1).IsEqualTo("Updated1");

        source.Prop5 = "Updated5";
        await Assert.That(target.ViewProp5).IsEqualTo("Updated5");
    }

    /// <summary>
    /// Verifies that BindTwoWay handles rapid back-and-forth changes without infinite loop.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_RapidBackAndForth()
    {
        var source = new BigViewModel { Prop1 = "Initial" };
        var target = new BigView();

        using var binding = BindTwoWayScenarios.StringProperty(source, target);

        // Rapid alternating changes
        source.Prop1 = "A";
        await Assert.That(target.ViewProp1).IsEqualTo("A");

        target.ViewProp1 = "B";
        await Assert.That(source.Prop1).IsEqualTo("B");

        source.Prop1 = "C";
        await Assert.That(target.ViewProp1).IsEqualTo("C");

        target.ViewProp1 = "D";
        await Assert.That(source.Prop1).IsEqualTo("D");
    }

    /// <summary>
    /// Verifies that BindTwoWay syncs double property in both directions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_DoubleProperty_SyncsBothDirections()
    {
        var source = new BigViewModel { Prop3 = 3.14 };
        var target = new BigView();

        using var binding = BindTwoWayScenarios.DoubleProperty(source, target);

        await Assert.That(target.ViewProp3).IsEqualTo(3.14);

        source.Prop3 = 2.71;
        await Assert.That(target.ViewProp3).IsEqualTo(2.71);

        target.ViewProp3 = 1.41;
        await Assert.That(source.Prop3).IsEqualTo(1.41);
    }

    /// <summary>
    /// Verifies that BindTwoWay syncs bool property in both directions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_BoolProperty_SyncsBothDirections()
    {
        var source = new BigViewModel { Prop4 = true };
        var target = new BigView();

        using var binding = BindTwoWayScenarios.BoolProperty(source, target);

        await Assert.That(target.ViewProp4).IsEqualTo(true);

        source.Prop4 = false;
        await Assert.That(target.ViewProp4).IsEqualTo(false);

        target.ViewProp4 = true;
        await Assert.That(source.Prop4).IsEqualTo(true);
    }

    /// <summary>
    /// Verifies that disposing a BindOneWay with deep chain source stops syncing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_DeepChainSource_Disposal()
    {
        var source = new BigViewModel();
        source.Address.City = "Seattle";
        var target = new BigView();

        var binding = BindOneWayScenarios.DeepChainProperty(source, target);
        await Assert.That(target.ViewProp1).IsEqualTo("Seattle");

        binding.Dispose();

        source.Address.City = "Portland";

        await Assert.That(target.ViewProp1).IsEqualTo("Seattle");
    }

    /// <summary>
    /// Verifies that setting a source property before establishing the binding
    /// correctly syncs the pre-set value to the target on subscription.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_SourcePropertySetBeforeBinding_SyncsOnSubscription()
    {
        var source = new BigViewModel { Prop1 = "PreSet" };
        var target = new BigView();

        // Property is already set before binding is created
        using var binding = BindOneWayScenarios.StringProperty(source, target);

        await Assert.That(target.ViewProp1).IsEqualTo("PreSet");
    }

    /// <summary>
    /// Verifies that BindTwoWay correctly syncs a pre-set source value and then
    /// supports bidirectional changes after binding.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindTwoWay_SourcePropertySetBeforeBinding_SyncsAndBiDirectional()
    {
        var source = new BigViewModel { Prop1 = "PreSet" };
        var target = new BigView();

        using var binding = BindTwoWayScenarios.StringProperty(source, target);

        // Pre-set value should sync
        await Assert.That(target.ViewProp1).IsEqualTo("PreSet");

        // Bidirectional should still work
        target.ViewProp1 = "FromView";
        await Assert.That(source.Prop1).IsEqualTo("FromView");

        source.Prop1 = "FromSource";
        await Assert.That(target.ViewProp1).IsEqualTo("FromSource");
    }

    /// <summary>
    /// Verifies that BindOneWay handles rapid sequential changes without missing any.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindOneWay_RapidChanges_AllSynced()
    {
        var source = new BigViewModel { Prop1 = "Start" };
        var target = new BigView();

        using var binding = BindOneWayScenarios.StringProperty(source, target);

        for (var i = 0; i < 100; i++)
        {
            source.Prop1 = $"Value_{i}";
        }

        await Assert.That(target.ViewProp1).IsEqualTo("Value_99");
    }
}
