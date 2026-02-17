// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
/// Tests for converting nullable double to double.
/// </summary>
public class NullableDoubleToDoubleTypeConverterTests
{
    /// <summary>
    ///     Verifies GetAffinityForObjects Returns2.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableDoubleToDoubleTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(2);
    }

    /// <summary>
    ///     Verifies TryConvert WithValue Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithValue_Succeeds()
    {
        var converter = new NullableDoubleToDoubleTypeConverter();
        double? value = 123.456789;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(123.456789);
    }

    /// <summary>
    ///     Verifies TryConvert Null ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Null_ReturnsFalse()
    {
        var converter = new NullableDoubleToDoubleTypeConverter();
        double? value = null;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    ///     Verifies FromType ReturnsDoubleNullable.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsDoubleNullable()
    {
        var converter = new NullableDoubleToDoubleTypeConverter();
        await Assert.That(converter.FromType).IsEqualTo(typeof(double?));
    }

    /// <summary>
    ///     Verifies ToType ReturnsDouble.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsDouble()
    {
        var converter = new NullableDoubleToDoubleTypeConverter();
        await Assert.That(converter.ToType).IsEqualTo(typeof(double));
    }
}
