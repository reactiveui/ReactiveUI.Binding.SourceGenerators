// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>
/// Tests for the <see cref="BindingTypeConverter{TFrom, TTo}"/> base class,
/// exercising the TryConvertTyped method's null handling and type dispatch paths
/// through both production converters and purpose-built test stubs.
/// </summary>
public class BindingTypeConverterBaseTests
{
    /// <summary>
    /// Verifies TryConvertTyped returns false when input is null and TFrom is a non-nullable value type (int).
    /// Exercises lines 61-64 in BindingTypeConverter.cs via a production converter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvertTyped_NullInput_NonNullableValueTypeFrom_ReturnsFalse()
    {
        var converter = new IntegerToStringTypeConverter();

        var result = converter.TryConvertTyped(null, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>
    /// Verifies TryConvertTyped returns false when the input type does not match TFrom.
    /// Exercises lines 83-86 in BindingTypeConverter.cs via a production converter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvertTyped_WrongInputType_ReturnsFalse()
    {
        var converter = new StringToIntegerTypeConverter();

        var result = converter.TryConvertTyped(42, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>
    /// Verifies TryConvertTyped returns false when TryConvert fails for a non-null input.
    /// Exercises lines 89-92 in BindingTypeConverter.cs via a production converter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvertTyped_ConversionFails_ReturnsFalse()
    {
        var converter = new StringToIntegerTypeConverter();

        var result = converter.TryConvertTyped("not-a-number", null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>
    /// Verifies TryConvertTyped succeeds for a valid non-null input through a production converter.
    /// Exercises lines 89, 101-102 in BindingTypeConverter.cs.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvertTyped_ValidInput_ReturnsTrue()
    {
        var converter = new StringToIntegerTypeConverter();

        var result = converter.TryConvertTyped("42", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(42);
    }

    /// <summary>
    /// Verifies TryConvertTyped succeeds when converting from a value type to a reference type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvertTyped_ValueTypeToReferenceType_Succeeds()
    {
        var converter = new IntegerToStringTypeConverter();

        var result = converter.TryConvertTyped(42, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("42");
    }

    /// <summary>
    /// Verifies TryConvertTyped returns false when null input is passed to a converter
    /// where TFrom is a nullable value type but TryConvert returns false for null.
    /// Exercises lines 67-70 in BindingTypeConverter.cs.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvertTyped_NullInput_NullableSourceTryConvertFails_ReturnsFalse()
    {
        var converter = new NullRejectingNullableConverter();

        var result = converter.TryConvertTyped(null, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>
    /// Verifies TryConvertTyped succeeds when null input is converted to a nullable result
    /// through a converter whose TFrom is a nullable value type.
    /// Exercises lines 67, 79-80 in BindingTypeConverter.cs.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvertTyped_NullInput_NullableSourceConvertsToNullableResult_Succeeds()
    {
        var converter = new NullableBooleanToStringTypeConverter();

        var result = converter.TryConvertTyped(null, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsNull();
    }

    /// <summary>
    /// Verifies TryConvertTyped succeeds when null input is converted to a non-null result
    /// through a converter whose TFrom is a reference type.
    /// Exercises lines 59, 67, 79-80 in BindingTypeConverter.cs.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvertTyped_NullInput_ReferenceTypeSourceConvertsToNonNullResult_Succeeds()
    {
        var converter = new NullAcceptingStringToStringConverter();

        var result = converter.TryConvertTyped(null, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("(null)");
    }

    /// <summary>
    /// Verifies TryConvertTyped succeeds when null input is converted to null for a nullable target type.
    /// Exercises lines 59, 67, 73 (condition false for reference type TTo), 79-80.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvertTyped_NullInput_NullResultWithNullableTarget_Succeeds()
    {
        var converter = new NullToNullStringConverter();

        var result = converter.TryConvertTyped(null, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsNull();
    }

    /// <summary>
    /// Verifies TryConvertTyped returns false when null input is passed to a converter
    /// where TFrom is a reference type but TryConvert returns false for null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvertTyped_NullInput_ReferenceTypeSourceTryConvertFails_ReturnsFalse()
    {
        var converter = new StringToIntegerTypeConverter();

        var result = converter.TryConvertTyped(null, null, out var output);

        await Assert.That(result).IsFalse();
        await Assert.That(output).IsNull();
    }

    /// <summary>
    /// Verifies TryConvertTyped passes conversionHint through to TryConvert.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvertTyped_WithConversionHint_PassesThroughToTryConvert()
    {
        var converter = new IntegerToStringTypeConverter();

        var result = converter.TryConvertTyped(5, 3, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo("005");
    }

    /// <summary>
    /// Verifies TryConvertTyped correctly boxes value type results.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvertTyped_ValueTypeResult_IsBoxedCorrectly()
    {
        var converter = new StringToIntegerTypeConverter();

        var result = converter.TryConvertTyped("100", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output is int).IsTrue();
        await Assert.That(output).IsEqualTo(100);
    }

    /// <summary>
    /// Verifies that FromType returns the correct type for a generic converter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FromType_ReturnsCorrectGenericType()
    {
        var converter = new NullAcceptingStringToStringConverter();

        await Assert.That(converter.FromType).IsEqualTo(typeof(string));
    }

    /// <summary>
    /// Verifies that ToType returns the correct type for a generic converter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ToType_ReturnsCorrectGenericType()
    {
        var converter = new NullAcceptingStringToStringConverter();

        await Assert.That(converter.ToType).IsEqualTo(typeof(string));
    }

    /// <summary>
    /// Verifies TryConvertTyped succeeds with null result when TTo is a nullable value type and input is null.
    /// Exercises the null-input path through a production StringToNullableIntegerTypeConverter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvertTyped_NullableValueTypeTarget_NullInput_ReturnsNullResult()
    {
        var converter = new StringToNullableIntegerTypeConverter();

        var result = converter.TryConvertTyped(null, null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsNull();
    }

    /// <summary>
    /// Verifies TryConvertTyped succeeds when TTo is a nullable value type and a valid non-null input is provided.
    /// Exercises the non-null path through a production StringToNullableIntegerTypeConverter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TryConvertTyped_NullableValueTypeTarget_ValidInput_ConvertsSuccessfully()
    {
        var converter = new StringToNullableIntegerTypeConverter();

        var result = converter.TryConvertTyped("42", null, out var output);

        await Assert.That(result).IsTrue();
        await Assert.That(output).IsEqualTo(42);
    }

    /// <summary>
    /// A converter from int? to string that rejects null input.
    /// Used to exercise the TryConvert failure path for nullable TFrom with null input.
    /// </summary>
    private sealed class NullRejectingNullableConverter : BindingTypeConverter<int?, string>
    {
        /// <inheritdoc/>
        public override int GetAffinityForObjects() => 1;

        /// <inheritdoc/>
        public override bool TryConvert(int? from, object? conversionHint, [MaybeNullWhen(true)] out string? result)
        {
            if (from is null)
            {
                result = null;
                return false;
            }

            result = from.Value.ToString();
            return true;
        }
    }

    /// <summary>
    /// A converter from string to string that accepts null input and converts it to "(null)".
    /// </summary>
    private sealed class NullAcceptingStringToStringConverter : BindingTypeConverter<string, string>
    {
        /// <inheritdoc/>
        public override int GetAffinityForObjects() => 1;

        /// <inheritdoc/>
        public override bool TryConvert(string? from, object? conversionHint, [MaybeNullWhen(true)] out string? result)
        {
            result = from ?? "(null)";
            return true;
        }
    }

    /// <summary>
    /// A converter from string to string that returns null for null input (for nullable TTo).
    /// </summary>
    private sealed class NullToNullStringConverter : BindingTypeConverter<string, string>
    {
        /// <inheritdoc/>
        public override int GetAffinityForObjects() => 1;

        /// <inheritdoc/>
        public override bool TryConvert(string? from, object? conversionHint, [MaybeNullWhen(true)] out string? result)
        {
            result = from;
            return true;
        }
    }
}
