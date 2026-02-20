// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
///     Tests for converting strings to nullable short integers.
/// </summary>
public class StringToNullableShortTypeConverterTests
{
    /// <summary>
    ///     Verifies GetAffinityForObjects Returns2.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new StringToNullableShortTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(2);
    }

    /// <summary>
    ///     Verifies TryConvert EmptyString ReturnsTrue.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_EmptyString_ReturnsTrue()
    {
        var converter = new StringToNullableShortTypeConverter();

        var result = converter.TryConvert(string.Empty, null, out var output);

        await Assert.That(result).IsTrue();
    }

    /// <summary>
    ///     Verifies TryConvert InvalidString ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_InvalidString_ReturnsFalse()
    {
        var converter = new StringToNullableShortTypeConverter();

        var result = converter.TryConvert("invalid", null, out var output);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    ///     Verifies TryConvert OutOfRangeValue ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_OutOfRangeValue_ReturnsFalse()
    {
        var converter = new StringToNullableShortTypeConverter();

        var result = converter.TryConvert("99999", null, out var output);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    ///     Verifies TryConvert StringToShortNullable Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_StringToShortNullable_Succeeds()
    {
        var converter = new StringToNullableShortTypeConverter();

        var result = converter.TryConvert("12345", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo((short)12345);
    }
}
