// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.ObservableForProperty;

/// <summary>
/// Basic tests for the runtime library types.
/// </summary>
public class BasicRuntimeTests
{
    /// <summary>
    /// The expected value used when constructing and asserting an observed change.
    /// </summary>
    private const int ExpectedValue = 42;

    /// <summary>
    /// Verifies that ObservedChange can be constructed and its properties accessed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ObservedChange_Properties()
    {
        var change = new ObservedChange<string, int>("sender", null, ExpectedValue);

        await Assert.That(change.Sender).IsEqualTo("sender");
        await Assert.That(change.Value).IsEqualTo(ExpectedValue);
        await Assert.That(change.Expression).IsNull();
    }

    /// <summary>
    /// Verifies that the BindingDirection enum defines the expected members at runtime.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindingDirection_Values()
    {
        await Assert.That(Enum.IsDefined(BindingDirection.OneWay)).IsTrue();
        await Assert.That(Enum.IsDefined(BindingDirection.TwoWay)).IsTrue();
    }
}
