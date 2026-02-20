// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
///     Tests for the <see cref="NullableShortToStringTypeConverter"/> type converter.
/// </summary>
public class NullableShortToStringTypeConverterTests
{
    /// <summary>
    ///     Verifies GetAffinityForObjects Returns2.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableShortToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(2);
    }

    /// <summary>
    ///     Verifies TryConvert MaxValue Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MaxValue_Succeeds()
    {
        var converter = new NullableShortToStringTypeConverter();
        short? value = short.MaxValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(short.MaxValue.ToString());
    }

    /// <summary>
    ///     Verifies TryConvert MinValue Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MinValue_Succeeds()
    {
        var converter = new NullableShortToStringTypeConverter();
        short? value = short.MinValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(short.MinValue.ToString());
    }

    /// <summary>
    ///     Verifies TryConvert NullValue ReturnsTrue.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NullValue_ReturnsTrue()
    {
        var converter = new NullableShortToStringTypeConverter();

        var result = converter.TryConvert(null, null, out var output);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    ///     Verifies TryConvert ShortNullableToString Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_ShortNullableToString_Succeeds()
    {
        var converter = new NullableShortToStringTypeConverter();
        short? value = 12345;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("12345");
    }

    /// <summary>
    ///     Verifies TryConvert WithConversionHint FormatsCorrectly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithConversionHint_FormatsCorrectly()
    {
        var converter = new NullableShortToStringTypeConverter();
        short? value = 42;

        var result = converter.TryConvert(value, 5, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("00042");
    }

    /// <summary>
    ///     Verifies TryConvert with a string format hint formats correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_WithStringFormatHint_FormatsCorrectly()
    {
        var converter = new NullableShortToStringTypeConverter();
        short? value = 255;

        var result = converter.TryConvert(value, "X", out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("FF");
    }
}
