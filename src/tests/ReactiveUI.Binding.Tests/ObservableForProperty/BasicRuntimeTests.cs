// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding;

namespace ReactiveUI.Binding.Tests.ObservableForProperty;

/// <summary>
/// Basic tests for the runtime library types.
/// </summary>
public class BasicRuntimeTests
{
    /// <summary>
    /// Verifies that ObservedChange can be constructed and its properties accessed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservedChange_Properties()
    {
        var change = new ObservedChange<string, int>("sender", null, 42);

        await Assert.That(change.Sender).IsEqualTo("sender");
        await Assert.That(change.Value).IsEqualTo(42);
        await Assert.That(change.Expression).IsNull();
    }

    /// <summary>
    /// Verifies that BindingDirection enum values exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingDirection_Values()
    {
        var oneWay = BindingDirection.OneWay;
        var twoWay = BindingDirection.TwoWay;
        await Assert.That(oneWay).IsEqualTo(BindingDirection.OneWay);
        await Assert.That(twoWay).IsEqualTo(BindingDirection.TwoWay);
    }
}
