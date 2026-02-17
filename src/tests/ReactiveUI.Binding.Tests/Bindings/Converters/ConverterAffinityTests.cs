// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings.Converters;

/// <summary>
/// Tests for verifying converter affinity values are correctly set.
/// </summary>
public class ConverterAffinityTests
{
    /// <summary>
    /// Verifies that the EqualityTypeConverter has affinity 1 (last resort).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EqualityConverter_ShouldHaveAffinity1() =>
        await AssertAffinity(new EqualityTypeConverter(), 1);

    // ===================================================================
    // String identity converter
    // ===================================================================

    /// <summary>
    /// Verifies that StringConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringConverter(), 2);

    // ===================================================================
    // Numeric → String converters
    // ===================================================================

    /// <summary>
    /// Verifies that ByteToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ByteToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new ByteToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that NullableByteToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableByteToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new NullableByteToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that ShortToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShortToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new ShortToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that NullableShortToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableShortToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new NullableShortToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that IntegerToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IntegerToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new IntegerToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that NullableIntegerToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableIntegerToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new NullableIntegerToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that LongToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LongToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new LongToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that NullableLongToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableLongToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new NullableLongToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that SingleToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new SingleToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that NullableSingleToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableSingleToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new NullableSingleToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that DoubleToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DoubleToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new DoubleToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that NullableDoubleToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDoubleToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new NullableDoubleToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that DecimalToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DecimalToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new DecimalToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that NullableDecimalToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDecimalToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new NullableDecimalToStringTypeConverter(), 2);

    // ===================================================================
    // String → Numeric converters
    // ===================================================================

    /// <summary>
    /// Verifies that StringToByteTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToByteTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToByteTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToNullableByteTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToNullableByteTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToNullableByteTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToShortTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToShortTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToShortTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToNullableShortTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToNullableShortTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToNullableShortTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToIntegerTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToIntegerTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToIntegerTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToNullableIntegerTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToNullableIntegerTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToNullableIntegerTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToLongTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToLongTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToLongTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToNullableLongTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToNullableLongTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToNullableLongTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToSingleTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToSingleTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToSingleTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToNullableSingleTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToNullableSingleTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToNullableSingleTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToDoubleTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToDoubleTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToDoubleTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToNullableDoubleTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToNullableDoubleTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToNullableDoubleTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToDecimalTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToDecimalTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToDecimalTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToNullableDecimalTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToNullableDecimalTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToNullableDecimalTypeConverter(), 2);

    // ===================================================================
    // Boolean ↔ String converters
    // ===================================================================

    /// <summary>
    /// Verifies that BooleanToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BooleanToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new BooleanToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that NullableBooleanToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableBooleanToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new NullableBooleanToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToBooleanTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToBooleanTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToBooleanTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToNullableBooleanTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToNullableBooleanTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToNullableBooleanTypeConverter(), 2);

    // ===================================================================
    // Guid ↔ String converters
    // ===================================================================

    /// <summary>
    /// Verifies that GuidToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GuidToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new GuidToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that NullableGuidToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableGuidToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new NullableGuidToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToGuidTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToGuidTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToGuidTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToNullableGuidTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToNullableGuidTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToNullableGuidTypeConverter(), 2);

    // ===================================================================
    // DateTime ↔ String converters
    // ===================================================================

    /// <summary>
    /// Verifies that DateTimeToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DateTimeToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new DateTimeToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that NullableDateTimeToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDateTimeToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new NullableDateTimeToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToDateTimeTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToDateTimeTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToDateTimeTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToNullableDateTimeTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToNullableDateTimeTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToNullableDateTimeTypeConverter(), 2);

    // ===================================================================
    // DateTimeOffset ↔ String converters
    // ===================================================================

    /// <summary>
    /// Verifies that DateTimeOffsetToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DateTimeOffsetToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new DateTimeOffsetToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that NullableDateTimeOffsetToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDateTimeOffsetToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new NullableDateTimeOffsetToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToDateTimeOffsetTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToDateTimeOffsetTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToDateTimeOffsetTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToNullableDateTimeOffsetTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToNullableDateTimeOffsetTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToNullableDateTimeOffsetTypeConverter(), 2);

    // ===================================================================
    // TimeSpan ↔ String converters
    // ===================================================================

    /// <summary>
    /// Verifies that TimeSpanToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TimeSpanToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new TimeSpanToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that NullableTimeSpanToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableTimeSpanToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new NullableTimeSpanToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToTimeSpanTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToTimeSpanTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToTimeSpanTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToNullableTimeSpanTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToNullableTimeSpanTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToNullableTimeSpanTypeConverter(), 2);

    // ===================================================================
    // DateOnly ↔ String converters
    // ===================================================================

    /// <summary>
    /// Verifies that DateOnlyToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DateOnlyToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new DateOnlyToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that NullableDateOnlyToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDateOnlyToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new NullableDateOnlyToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToDateOnlyTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToDateOnlyTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToDateOnlyTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToNullableDateOnlyTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToNullableDateOnlyTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToNullableDateOnlyTypeConverter(), 2);

    // ===================================================================
    // TimeOnly ↔ String converters
    // ===================================================================

    /// <summary>
    /// Verifies that TimeOnlyToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TimeOnlyToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new TimeOnlyToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that NullableTimeOnlyToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableTimeOnlyToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new NullableTimeOnlyToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToTimeOnlyTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToTimeOnlyTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToTimeOnlyTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToNullableTimeOnlyTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToNullableTimeOnlyTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToNullableTimeOnlyTypeConverter(), 2);

    // ===================================================================
    // Uri ↔ String converters
    // ===================================================================

    /// <summary>
    /// Verifies that UriToStringTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UriToStringTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new UriToStringTypeConverter(), 2);

    /// <summary>
    /// Verifies that StringToUriTypeConverter has affinity 2.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToUriTypeConverter_ShouldHaveAffinity2() =>
        await AssertAffinity(new StringToUriTypeConverter(), 2);

    private static async Task AssertAffinity(IBindingTypeConverter converter, int expectedAffinity)
    {
        var actualAffinity = converter.GetAffinityForObjects();
        await Assert.That(actualAffinity).IsEqualTo(expectedAffinity);
    }
}
