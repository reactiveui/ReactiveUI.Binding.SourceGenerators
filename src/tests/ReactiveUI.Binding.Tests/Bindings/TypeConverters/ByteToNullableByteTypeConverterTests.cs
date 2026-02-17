// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
/// Tests for converting byte to nullable byte.
/// </summary>
public class ByteToNullableByteTypeConverterTests
{
    /// <summary>
    ///     Verifies GetAffinityForObjects Returns2.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new ByteToNullableByteTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(2);
    }

    /// <summary>
    ///     Verifies TryConvert AlwaysSucceeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_AlwaysSucceeds()
    {
        var converter = new ByteToNullableByteTypeConverter();
        byte value = 42;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo((byte?)42);
    }

    /// <summary>
    ///     Verifies FromType ReturnsByte.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task FromType_ReturnsByte()
    {
        var converter = new ByteToNullableByteTypeConverter();
        await Assert.That(converter.FromType).IsEqualTo(typeof(byte));
    }

    /// <summary>
    ///     Verifies ToType ReturnsByteNullable.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task ToType_ReturnsByteNullable()
    {
        var converter = new ByteToNullableByteTypeConverter();
        await Assert.That(converter.ToType).IsEqualTo(typeof(byte?));
    }

    /// <summary>
    ///     Verifies TryConvertTyped WithValidValue ReturnsTrueAndOutput.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithValidValue_ReturnsTrueAndOutput()
    {
        var converter = new ByteToNullableByteTypeConverter();
        byte value = 42;

        var success = converter.TryConvertTyped(value, null, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo((byte?)42);
    }

    /// <summary>
    ///     Verifies TryConvertTyped WithNullValue ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithNullValue_ReturnsFalse()
    {
        var converter = new ByteToNullableByteTypeConverter();

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
        var converter = new ByteToNullableByteTypeConverter();
        string value = "invalid";

        var success = converter.TryConvertTyped(value, null, out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsNull();
    }
}
