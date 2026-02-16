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
    /// Verifies that BindTwoWay syncs the initial value from source to target.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringProperty_SyncsSourceToTarget()
    {
        var source = new BigViewModel { Prop1 = "Hello" };
        var target = new BigView();

        using var binding = BindTwoWayScenarios.StringProperty(source, target);

        await Assert.That(target.ViewProp1).IsEqualTo("Hello");

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
        var source = new BigViewModel { Prop1 = "Hello" };
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
        var source = new BigViewModel { Prop2 = 42 };
        var target = new BigView();

        using var binding = BindTwoWayScenarios.IntProperty(source, target);

        await Assert.That(target.ViewProp2).IsEqualTo(42);

        source.Prop2 = 100;
        await Assert.That(target.ViewProp2).IsEqualTo(100);

        target.ViewProp2 = 200;
        await Assert.That(source.Prop2).IsEqualTo(200);
    }

    /// <summary>
    /// Verifies that disposing the two-way binding stops syncing in both directions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Disposal_StopsSyncing()
    {
        var source = new BigViewModel { Prop1 = "Hello" };
        var target = new BigView();

        var binding = BindTwoWayScenarios.StringProperty(source, target);
        await Assert.That(target.ViewProp1).IsEqualTo("Hello");

        binding.Dispose();

        source.Prop1 = "AfterDisposal";
        await Assert.That(target.ViewProp1).IsEqualTo("Hello");

        target.ViewProp1 = "FromTarget";
        await Assert.That(source.Prop1).IsEqualTo("AfterDisposal");
    }
}
