// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
/// Tests for converting nullable booleans to strings.
/// </summary>
public class NullableBooleanToStringTypeConverterTests
{
    /// <summary>
    ///     Verifies GetAffinityForObjects Returns2.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableBooleanToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(2);
    }

    /// <summary>
    ///     Verifies TryConvert True ReturnsTrue.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_True_ReturnsTrue()
    {
        var converter = new NullableBooleanToStringTypeConverter();
        bool? value = true;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("True");
    }

    /// <summary>
    ///     Verifies TryConvert False ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_False_ReturnsFalse()
    {
        var converter = new NullableBooleanToStringTypeConverter();
        bool? value = false;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("False");
    }

    /// <summary>
    ///     Verifies TryConvert Null ReturnsNullString.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Null_ReturnsNullString()
    {
        var converter = new NullableBooleanToStringTypeConverter();
        bool? value = null;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsNull();
    }
}
