// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET6_0_OR_GREATER
namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
/// Tests for converting strings to TimeOnly.
/// </summary>
public class StringToTimeOnlyTypeConverterTests
{
    /// <summary>
    ///     Verifies GetAffinityForObjects Returns2.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new StringToTimeOnlyTypeConverter();
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
        var converter = new StringToTimeOnlyTypeConverter();
        var expected = new TimeOnly(10, 30, 0);

        var result = converter.TryConvert(expected.ToString(), null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(expected);
    }

    /// <summary>
    ///     Verifies TryConvert Null ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_Null_ReturnsFalse()
    {
        var converter = new StringToTimeOnlyTypeConverter();

        var result = converter.TryConvert(null, null, out var output);

        await Assert.That(result).IsFalse();
    }

    /// <summary>
    ///     Verifies TryConvert EmptyString ReturnsFalse.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_EmptyString_ReturnsFalse()
    {
        var converter = new StringToTimeOnlyTypeConverter();

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
        var converter = new StringToTimeOnlyTypeConverter();

        var result = converter.TryConvert("invalid", null, out var output);

        await Assert.That(result).IsFalse();
    }
}
#endif
