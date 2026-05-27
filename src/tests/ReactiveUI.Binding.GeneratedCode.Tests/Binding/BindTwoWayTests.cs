// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>
/// Tests that the source-generator-generated BindTwoWay code works correctly at runtime.
/// </summary>
public class BindTwoWayTests
{
    /// <summary>
    /// The initial string property value used across the binding tests.
    /// </summary>
    private const string HelloValue = "Hello";

    /// <summary>
    /// The initial integer property test value.
    /// </summary>
    private const int IntValue = 42;

    /// <summary>
    /// The updated integer property test value.
    /// </summary>
    private const int UpdatedIntValue = 100;

    /// <summary>
    /// The second updated integer property test value.
    /// </summary>
    private const int SecondIntValue = 200;

    /// <summary>
    /// Verifies that BindTwoWay syncs the initial value from source to target.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringProperty_SyncsSourceToTarget()
    {
        var source = new BigViewModel { Prop1 = HelloValue };
        var target = new BigView();

        using var binding = BindTwoWayScenarios.StringProperty(source, target);

        await Assert.That(target.ViewProp1).IsEqualTo(HelloValue);

        source.Prop1 = "World";

        await Assert.That(target.ViewProp1).IsEqualTo("World");
    }

    /// <summary>
    /// Verifies that BindTwoWay syncs changes from target back to source.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringProperty_SyncsTargetToSource()
    {
        var source = new BigViewModel { Prop1 = HelloValue };
        var target = new BigView();

        using var binding = BindTwoWayScenarios.StringProperty(source, target);

        target.ViewProp1 = "FromTarget";

        await Assert.That(source.Prop1).IsEqualTo("FromTarget");
    }

    /// <summary>
    /// Verifies that BindTwoWay syncs int property in both directions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IntProperty_SyncsBothDirections()
    {
        var source = new BigViewModel { Prop2 = IntValue };
        var target = new BigView();

        using var binding = BindTwoWayScenarios.IntProperty(source, target);

        await Assert.That(target.ViewProp2).IsEqualTo(IntValue);

        source.Prop2 = UpdatedIntValue;
        await Assert.That(target.ViewProp2).IsEqualTo(UpdatedIntValue);

        target.ViewProp2 = SecondIntValue;
        await Assert.That(source.Prop2).IsEqualTo(SecondIntValue);
    }

    /// <summary>
    /// Verifies that disposing the two-way binding stops syncing in both directions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Disposal_StopsSyncing()
    {
        var source = new BigViewModel { Prop1 = HelloValue };
        var target = new BigView();

        var binding = BindTwoWayScenarios.StringProperty(source, target);
        await Assert.That(target.ViewProp1).IsEqualTo(HelloValue);

        binding.Dispose();

        source.Prop1 = "AfterDisposal";
        await Assert.That(target.ViewProp1).IsEqualTo(HelloValue);

        target.ViewProp1 = "FromTarget";
        await Assert.That(source.Prop1).IsEqualTo("AfterDisposal");
    }
}
