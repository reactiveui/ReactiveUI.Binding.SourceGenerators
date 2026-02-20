// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>
///     Unit tests for standard binding type converters verifying basic conversion correctness.
/// </summary>
public class BindingTypeConvertersUnitTests
{
    /// <summary>
    ///     Verifies that ByteToStringTypeConverter converts correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ByteToStringTypeConverter_Converts_Correctly()
    {
        var converter = new ByteToStringTypeConverter();
        byte val = 123;

        // Byte to String
        var result = converter.TryConvert(val, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("123");
    }

    /// <summary>
    ///     Verifies that DecimalToStringTypeConverter converts correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task DecimalToStringTypeConverter_Converts_Correctly()
    {
        var converter = new DecimalToStringTypeConverter();
        var val = 123.456m;

        var result = converter.TryConvert(val, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(val.ToString());
    }

    /// <summary>
    ///     Verifies that DoubleToStringTypeConverter converts correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task DoubleToStringTypeConverter_Converts_Correctly()
    {
        var converter = new DoubleToStringTypeConverter();
        var val = 123.456789;

        var result = converter.TryConvert(val, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(val.ToString());
    }

    /// <summary>
    ///     Verifies that IntegerToStringTypeConverter converts correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task IntegerToStringTypeConverter_Converts_Correctly()
    {
        var converter = new IntegerToStringTypeConverter();
        var val = 123456789;

        var result = converter.TryConvert(val, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("123456789");
    }

    /// <summary>
    ///     Verifies that LongToStringTypeConverter converts correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task LongToStringTypeConverter_Converts_Correctly()
    {
        var converter = new LongToStringTypeConverter();
        var val = 1234567890123456789;

        var result = converter.TryConvert(val, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("1234567890123456789");
    }

    /// <summary>
    ///     Verifies that NullableByteToStringTypeConverter converts correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NullableByteToStringTypeConverter_Converts_Correctly()
    {
        var converter = new NullableByteToStringTypeConverter();
        byte? val = 123;

        // Byte? to String
        var result = converter.TryConvert(val, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("123");
    }

    /// <summary>
    ///     Verifies that NullableDecimalToStringTypeConverter converts correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NullableDecimalToStringTypeConverter_Converts_Correctly()
    {
        var converter = new NullableDecimalToStringTypeConverter();
        decimal? val = 123.456m;

        var result = converter.TryConvert(val, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(val.ToString());
    }

    /// <summary>
    ///     Verifies that NullableDoubleToStringTypeConverter converts correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NullableDoubleToStringTypeConverter_Converts_Correctly()
    {
        var converter = new NullableDoubleToStringTypeConverter();
        double? val = 123.456789;

        var result = converter.TryConvert(val, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(val.ToString());
    }

    /// <summary>
    ///     Verifies that NullableIntegerToStringTypeConverter converts correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NullableIntegerToStringTypeConverter_Converts_Correctly()
    {
        var converter = new NullableIntegerToStringTypeConverter();
        int? val = 123456789;

        var result = converter.TryConvert(val, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("123456789");
    }

    /// <summary>
    ///     Verifies that NullableLongToStringTypeConverter converts correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NullableLongToStringTypeConverter_Converts_Correctly()
    {
        var converter = new NullableLongToStringTypeConverter();
        long? val = 1234567890123456789;

        var result = converter.TryConvert(val, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("1234567890123456789");
    }

    /// <summary>
    ///     Verifies that NullableShortToStringTypeConverter converts correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NullableShortToStringTypeConverter_Converts_Correctly()
    {
        var converter = new NullableShortToStringTypeConverter();
        short? val = 12345;

        var result = converter.TryConvert(val, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("12345");
    }

    /// <summary>
    ///     Verifies that NullableSingleToStringTypeConverter converts correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task NullableSingleToStringTypeConverter_Converts_Correctly()
    {
        var converter = new NullableSingleToStringTypeConverter();
        float? val = 123.45f;

        var result = converter.TryConvert(val, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(val.ToString());
    }

    /// <summary>
    ///     Verifies that ShortToStringTypeConverter converts correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ShortToStringTypeConverter_Converts_Correctly()
    {
        var converter = new ShortToStringTypeConverter();
        short val = 12345;

        var result = converter.TryConvert(val, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("12345");
    }

    /// <summary>
    ///     Verifies that SingleToStringTypeConverter converts correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task SingleToStringTypeConverter_Converts_Correctly()
    {
        var converter = new SingleToStringTypeConverter();
        var val = 123.45f;

        var result = converter.TryConvert(val, null, out var output);
        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(val.ToString());
    }
}
