// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
/// Tests for converting TimeSpan to strings.
/// </summary>
public class TimeSpanToStringTypeConverterTests
{
    /// <summary>
    ///     Verifies GetAffinityForObjects Returns2.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task GetAffinityForObjects_Returns2()
    {
        var converter = new TimeSpanToStringTypeConverter();
        var affinity = converter.GetAffinityForObjects();
        await Assert.That(affinity).IsEqualTo(2);
    }

    /// <summary>
    ///     Verifies TryConvert TimeSpan Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_TimeSpan_Succeeds()
    {
        var converter = new TimeSpanToStringTypeConverter();
        var value = TimeSpan.FromHours(2.5);

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString());
    }

    /// <summary>
    ///     Verifies TryConvert ZeroTimeSpan Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_ZeroTimeSpan_Succeeds()
    {
        var converter = new TimeSpanToStringTypeConverter();
        var value = TimeSpan.Zero;

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("00:00:00");
    }

    /// <summary>
    ///     Verifies TryConvert NegativeTimeSpan Succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TryConvert_NegativeTimeSpan_Succeeds()
    {
        var converter = new TimeSpanToStringTypeConverter();
        var value = TimeSpan.FromMinutes(-30);

        var result = converter.TryConvert(value, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(value.ToString());
    }
}
