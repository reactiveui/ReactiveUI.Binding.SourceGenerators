// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>
/// Tests that the source-generator-generated BindOneWay code works correctly at runtime.
/// </summary>
public class BindOneWayTests
{
    /// <summary>
    /// Verifies that BindOneWay syncs the initial string property value.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringProperty_SyncsInitialValue()
    {
        var source = new BigViewModel { Prop1 = "Hello" };
        var target = new BigView();

        using var binding = BindOneWayScenarios.StringProperty(source, target);

        await Assert.That(target.ViewProp1).IsEqualTo("Hello");
    }

    /// <summary>
    /// Verifies that BindOneWay syncs string property changes from source to target.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringProperty_SyncsOnSourceChange()
    {
        var source = new BigViewModel { Prop1 = "Hello" };
        var target = new BigView();

        using var binding = BindOneWayScenarios.StringProperty(source, target);

        source.Prop1 = "World";

        await Assert.That(target.ViewProp1).IsEqualTo("World");
    }

    /// <summary>
    /// Verifies that BindOneWay syncs int property values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IntProperty_SyncsValues()
    {
        var source = new BigViewModel { Prop2 = 42 };
        var target = new BigView();

        using var binding = BindOneWayScenarios.IntProperty(source, target);

        await Assert.That(target.ViewProp2).IsEqualTo(42);

        source.Prop2 = 100;

        await Assert.That(target.ViewProp2).IsEqualTo(100);
    }

    /// <summary>
    /// Verifies that BindOneWay syncs double property values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DoubleProperty_SyncsValues()
    {
        var source = new BigViewModel { Prop3 = 3.14 };
        var target = new BigView();

        using var binding = BindOneWayScenarios.DoubleProperty(source, target);

        await Assert.That(target.ViewProp3).IsEqualTo(3.14);

        source.Prop3 = 2.71;

        await Assert.That(target.ViewProp3).IsEqualTo(2.71);
    }

    /// <summary>
    /// Verifies that BindOneWay syncs bool property values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BoolProperty_SyncsValues()
    {
        var source = new BigViewModel { Prop4 = true };
        var target = new BigView();

        using var binding = BindOneWayScenarios.BoolProperty(source, target);

        await Assert.That(target.ViewProp4).IsEqualTo(true);

        source.Prop4 = false;

        await Assert.That(target.ViewProp4).IsEqualTo(false);
    }

    /// <summary>
    /// Verifies that disposing the binding stops syncing values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Disposal_StopsSyncing()
    {
        var source = new BigViewModel { Prop1 = "Hello" };
        var target = new BigView();

        var binding = BindOneWayScenarios.StringProperty(source, target);
        binding.Dispose();

        source.Prop1 = "AfterDisposal";

        await Assert.That(target.ViewProp1).IsEqualTo("Hello");
    }
}
