// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
///     Tests for the <see cref="NullableDoubleToStringTypeConverter"/> type converter.
/// </summary>
public class NullableDoubleToStringTypeConverterTests
{
    /// <summary>
    ///     Verifies GetAffinityForObjects Returns2.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableDoubleToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(2);
    }

    /// <summary>
    ///     Verifies TryConvert DoubleNullableToString Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_DoubleNullableToString_Succeeds()
    {
        var converter = new NullableDoubleToStringTypeConverter();
        double? value = 123.456;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString());
    }

    /// <summary>
    ///     Verifies TryConvert MaxValue Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MaxValue_Succeeds()
    {
        var converter = new NullableDoubleToStringTypeConverter();
        double? value = double.MaxValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(double.MaxValue.ToString());
    }

    /// <summary>
    ///     Verifies TryConvert MinValue Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MinValue_Succeeds()
    {
        var converter = new NullableDoubleToStringTypeConverter();
        double? value = double.MinValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(double.MinValue.ToString());
    }

    /// <summary>
    ///     Verifies TryConvert NegativeValue Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NegativeValue_Succeeds()
    {
        var converter = new NullableDoubleToStringTypeConverter();
        double? value = -123.456;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString());
    }

    /// <summary>
    ///     Verifies TryConvert NullValue ReturnsTrue.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NullValue_ReturnsTrue()
    {
        var converter = new NullableDoubleToStringTypeConverter();

        var result = converter.TryConvert(null, null, out var output);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    ///     Verifies TryConvert WithConversionHint FormatsCorrectly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithConversionHint_FormatsCorrectly()
    {
        var converter = new NullableDoubleToStringTypeConverter();
        double? value = 42.5;

        var result = converter.TryConvert(value, 2, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("42.50");
    }
}
