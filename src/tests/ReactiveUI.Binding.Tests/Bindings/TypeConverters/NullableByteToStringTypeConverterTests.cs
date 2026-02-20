// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
///     Tests for the <see cref="NullableByteToStringTypeConverter"/> type converter.
/// </summary>
public class NullableByteToStringTypeConverterTests
{
    /// <summary>
    ///     Verifies GetAffinityForObjects Returns2.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableByteToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(2);
    }

    /// <summary>
    ///     Verifies TryConvert ByteNullableToString Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_ByteNullableToString_Succeeds()
    {
        var converter = new NullableByteToStringTypeConverter();
        byte? value = 123;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("123");
    }

    /// <summary>
    ///     Verifies TryConvert MaxValue Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MaxValue_Succeeds()
    {
        var converter = new NullableByteToStringTypeConverter();
        byte? value = byte.MaxValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("255");
    }

    /// <summary>
    ///     Verifies TryConvert MinValue Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MinValue_Succeeds()
    {
        var converter = new NullableByteToStringTypeConverter();
        byte? value = byte.MinValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("0");
    }

    /// <summary>
    ///     Verifies TryConvert NullValue ReturnsTrue.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NullValue_ReturnsTrue()
    {
        var converter = new NullableByteToStringTypeConverter();

        var result = converter.TryConvert(null, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsNull();
    }

    /// <summary>
    ///     Verifies TryConvert WithConversionHint FormatsCorrectly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithConversionHint_FormatsCorrectly()
    {
        var converter = new NullableByteToStringTypeConverter();
        byte? value = 5;

        var result = converter.TryConvert(value, 3, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("005");
    }

    /// <summary>
    ///     Verifies TryConvert with a string format hint formats correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_FormatsCorrectly()
    {
        var converter = new NullableByteToStringTypeConverter();
        byte? value = 255;

        var result = converter.TryConvert(value, "X", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("FF");
    }
}
