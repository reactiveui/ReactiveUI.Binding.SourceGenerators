// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
/// Tests for converting strings to nullable booleans.
/// </summary>
public class StringToNullableBooleanTypeConverterTests
{
    /// <summary>
    ///     Verifies GetAffinityForObjects Returns2.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new StringToNullableBooleanTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(2);
    }

    /// <summary>
    ///     Verifies TryConvert TrueString Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_TrueString_Succeeds()
    {
        var converter = new StringToNullableBooleanTypeConverter();

        var result = converter.TryConvert("True", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(true);
    }

    /// <summary>
    ///     Verifies TryConvert FalseString Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_FalseString_Succeeds()
    {
        var converter = new StringToNullableBooleanTypeConverter();

        var result = converter.TryConvert("False", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(false);
    }

    /// <summary>
    ///     Verifies TryConvert TrueLowercase Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_TrueLowercase_Succeeds()
    {
        var converter = new StringToNullableBooleanTypeConverter();

        var result = converter.TryConvert("true", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(true);
    }

    /// <summary>
    ///     Verifies TryConvert FalseLowercase Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_FalseLowercase_Succeeds()
    {
        var converter = new StringToNullableBooleanTypeConverter();

        var result = converter.TryConvert("false", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(false);
    }

    /// <summary>
    ///     Verifies TryConvert Null ReturnsNull.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Null_ReturnsNull()
    {
        var converter = new StringToNullableBooleanTypeConverter();

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
        var converter = new StringToNullableBooleanTypeConverter();

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
        var converter = new StringToNullableBooleanTypeConverter();

        var result = converter.TryConvert("invalid", null, out var output);

        await Assert.That(result).IsFalse();
    }
}
