// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
/// Tests for converting strings to nullable Guid.
/// </summary>
public class StringToNullableGuidTypeConverterTests
{
    /// <summary>
    ///     Verifies GetAffinityForObjects Returns2.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new StringToNullableGuidTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(2);
    }

    /// <summary>
    ///     Verifies TryConvert ValidString Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_ValidString_Succeeds()
    {
        var converter = new StringToNullableGuidTypeConverter();
        var expected = Guid.Parse("12345678-1234-1234-1234-123456789abc");

        var result = converter.TryConvert("12345678-1234-1234-1234-123456789abc", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(expected);
    }

    /// <summary>
    ///     Verifies TryConvert Null ReturnsNull.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Null_ReturnsNull()
    {
        var converter = new StringToNullableGuidTypeConverter();

        var result = converter.TryConvert(null, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsNull();
    }

    /// <summary>
    ///     Verifies TryConvert EmptyString ReturnsNull.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_EmptyString_ReturnsNull()
    {
        var converter = new StringToNullableGuidTypeConverter();

        var result = converter.TryConvert(string.Empty, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsNull();
    }

    /// <summary>
    ///     Verifies TryConvert InvalidString ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_InvalidString_ReturnsFalse()
    {
        var converter = new StringToNullableGuidTypeConverter();

        var result = converter.TryConvert("invalid", null, out var output);

        await Assert.That(result).IsFalse();
    }
}
