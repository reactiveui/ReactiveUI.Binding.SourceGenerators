// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
///     Tests for converting strings to floats (single-precision).
/// </summary>
public class StringToSingleTypeConverterTests
{
    /// <summary>
    ///     Verifies GetAffinityForObjects Returns2.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new StringToSingleTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(2);
    }

    /// <summary>
    ///     Verifies TryConvert EmptyString ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_EmptyString_ReturnsFalse()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvert(string.Empty, null, out var output);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    ///     Verifies TryConvert InvalidString ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_InvalidString_ReturnsFalse()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvert("invalid", null, out var output);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    ///     Verifies TryConvert StringToSingle Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_StringToSingle_Succeeds()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvert("123.456", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(123.456f).Within(0.001f);
    }

    /// <summary>
    ///     Verifies TryConvert NullString ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NullString_ReturnsFalse()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvert(null, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsEqualTo(0.0f);
    }

    /// <summary>
    ///     Verifies TryConvert ZeroValue Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_ZeroValue_Succeeds()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvert("0", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(0.0f);
    }

    /// <summary>
    ///     Verifies TryConvert NegativeValue Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NegativeValue_Succeeds()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvert("-123.456", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(-123.456f).Within(0.001f);
    }

    /// <summary>
    ///     Verifies TryConvert ScientificNotation Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_ScientificNotation_Succeeds()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvert("1.23E+5", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(1.23E+5f).Within(0.1f);
    }

    /// <summary>
    ///     Verifies TryConvertTyped ValidString Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_ValidString_Succeeds()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvertTyped("456.789", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsTypeOf<float>();
        await Assert.That((float)output!).IsEqualTo(456.789f).Within(0.001f);
    }

    /// <summary>
    ///     Verifies TryConvertTyped InvalidType ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_InvalidType_ReturnsFalse()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvertTyped(123.456, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>
    ///     Verifies TryConvertTyped NullInput ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_NullInput_ReturnsFalse()
    {
        var converter = new StringToSingleTypeConverter();

        var result = converter.TryConvertTyped(null, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>
    ///     Verifies FromType ReturnsStringType.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsStringType()
    {
        var converter = new StringToSingleTypeConverter();

        await Assert.That(converter.FromType).IsEqualTo(typeof(string));
    }

    /// <summary>
    ///     Verifies ToType ReturnsFloatType.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsFloatType()
    {
        var converter = new StringToSingleTypeConverter();

        await Assert.That(converter.ToType).IsEqualTo(typeof(float));
    }
}
