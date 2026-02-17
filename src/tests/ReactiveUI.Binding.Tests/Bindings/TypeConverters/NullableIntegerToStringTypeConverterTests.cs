// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
///     Tests for the <see cref="NullableIntegerToStringTypeConverter"/> type converter.
/// </summary>
public class NullableIntegerToStringTypeConverterTests
{
    /// <summary>
    ///     Verifies GetAffinityForObjects Returns2.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new NullableIntegerToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(2);
    }

    /// <summary>
    ///     Verifies TryConvert IntNullableToString Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_IntNullableToString_Succeeds()
    {
        var converter = new NullableIntegerToStringTypeConverter();
        int? value = 123456;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("123456");
    }

    /// <summary>
    ///     Verifies TryConvert MaxValue Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MaxValue_Succeeds()
    {
        var converter = new NullableIntegerToStringTypeConverter();
        int? value = int.MaxValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(int.MaxValue.ToString());
    }

    /// <summary>
    ///     Verifies TryConvert MinValue Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_MinValue_Succeeds()
    {
        var converter = new NullableIntegerToStringTypeConverter();
        int? value = int.MinValue;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(int.MinValue.ToString());
    }

    /// <summary>
    ///     Verifies TryConvert NullValue ReturnsTrue.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NullValue_ReturnsTrue()
    {
        var converter = new NullableIntegerToStringTypeConverter();

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
        var converter = new NullableIntegerToStringTypeConverter();
        int? value = 42;

        var result = converter.TryConvert(value, 8, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("00000042");
    }
}
