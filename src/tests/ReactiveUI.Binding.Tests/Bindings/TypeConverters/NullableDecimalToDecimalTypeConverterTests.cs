// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
/// Tests for converting nullable decimal to decimal.
/// </summary>
public class NullableDecimalToDecimalTypeConverterTests
{
    /// <summary>
    ///     Verifies GetAffinityForObjects Returns2.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableDecimalToDecimalTypeConverter();
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
        var converter = new NullableDecimalToDecimalTypeConverter();
        decimal? value = 123.456789m;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(123.456789m);
    }

    /// <summary>
    ///     Verifies TryConvert Null ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Null_ReturnsFalse()
    {
        var converter = new NullableDecimalToDecimalTypeConverter();
        decimal? value = null;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    ///     Verifies FromType ReturnsDecimalNullable.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsDecimalNullable()
    {
        var converter = new NullableDecimalToDecimalTypeConverter();
        await Assert.That(converter.FromType).IsEqualTo(typeof(decimal?));
    }

    /// <summary>
    ///     Verifies ToType ReturnsDecimal.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsDecimal()
    {
        var converter = new NullableDecimalToDecimalTypeConverter();
        await Assert.That(converter.ToType).IsEqualTo(typeof(decimal));
    }

    /// <summary>
    ///     Verifies TryConvertTyped WithValidValue ReturnsTrueAndOutput.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithValidValue_ReturnsTrueAndOutput()
    {
        var converter = new NullableDecimalToDecimalTypeConverter();
        decimal? value = 42.5m;

        var success = converter.TryConvertTyped(value, null, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(42.5m);
    }

    /// <summary>
    ///     Verifies TryConvertTyped WithNullValue ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithNullValue_ReturnsFalse()
    {
        var converter = new NullableDecimalToDecimalTypeConverter();

        var success = converter.TryConvertTyped(null, null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }

    /// <summary>
    ///     Verifies TryConvertTyped WithInvalidType ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithInvalidType_ReturnsFalse()
    {
        var converter = new NullableDecimalToDecimalTypeConverter();
        string value = "invalid";

        var success = converter.TryConvertTyped(value, null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }
}
