// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>
/// Unit tests for <see cref="BindingTypeConverter{TFrom, TTo}.TryConvertTyped(object?, object?, out object?)"/>
/// to ensure coverage of the base class dispatch path.
/// </summary>
public class BindingTypeConverterTryConvertTypedTests
{
    /// <summary>
    /// Data source for converter tests.
    /// </summary>
    /// <returns>A collection of test cases.</returns>
    public static IEnumerable<ConverterTestData> TestCases()
    {
        // ===================================================================
        // Non-nullable to String converters (extend BindingTypeConverter<T, string>)
        // ===================================================================

        // ByteToString
        yield return new ConverterTestData(new ByteToStringTypeConverter(), (byte)42, "42", "ByteToString (Success)");
        yield return new ConverterTestData(new ByteToStringTypeConverter(), "wrong", null, "ByteToString (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new ByteToStringTypeConverter(), null, null, "ByteToString (Failure - Null)", false);

        // IntegerToString
        yield return new ConverterTestData(new IntegerToStringTypeConverter(), 123, "123", "IntegerToString (Success)");
        yield return new ConverterTestData(new IntegerToStringTypeConverter(), "wrong", null, "IntegerToString (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new IntegerToStringTypeConverter(), null, null, "IntegerToString (Failure - Null)", false);

        // StringToInteger
        yield return new ConverterTestData(new StringToIntegerTypeConverter(), "456", 456, "StringToInteger (Success)");

        // BooleanToString
        yield return new ConverterTestData(new BooleanToStringTypeConverter(), true, "True", "BooleanToString (Success)");

        // DoubleToString
        yield return new ConverterTestData(new DoubleToStringTypeConverter(), 3.14, 3.14.ToString(), "DoubleToString (Success)");
        yield return new ConverterTestData(new DoubleToStringTypeConverter(), "wrong", null, "DoubleToString (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new DoubleToStringTypeConverter(), null, null, "DoubleToString (Failure - Null)", false);

        // LongToString
        yield return new ConverterTestData(new LongToStringTypeConverter(), 9876543210L, "9876543210", "LongToString (Success)");
        yield return new ConverterTestData(new LongToStringTypeConverter(), "wrong", null, "LongToString (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new LongToStringTypeConverter(), null, null, "LongToString (Failure - Null)", false);

        // ShortToString
        yield return new ConverterTestData(new ShortToStringTypeConverter(), (short)321, "321", "ShortToString (Success)");
        yield return new ConverterTestData(new ShortToStringTypeConverter(), "wrong", null, "ShortToString (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new ShortToStringTypeConverter(), null, null, "ShortToString (Failure - Null)", false);

        // SingleToString
        yield return new ConverterTestData(new SingleToStringTypeConverter(), 1.5f, 1.5f.ToString(), "SingleToString (Success)");
        yield return new ConverterTestData(new SingleToStringTypeConverter(), "wrong", null, "SingleToString (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new SingleToStringTypeConverter(), null, null, "SingleToString (Failure - Null)", false);

        // ===================================================================
        // Nullable to String converters (extend BindingTypeConverter<T?, string>)
        // ===================================================================

        // NullableByteToString
        yield return new ConverterTestData(new NullableByteToStringTypeConverter(), (byte)42, "42", "NullableByteToString (Success)");
        yield return new ConverterTestData(new NullableByteToStringTypeConverter(), "wrong", null, "NullableByteToString (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new NullableByteToStringTypeConverter(), null, null, "NullableByteToString (Success - Null)");

        // NullableDecimalToString
        yield return new ConverterTestData(new NullableDecimalToStringTypeConverter(), 99.99m, 99.99m.ToString(), "NullableDecimalToString (Success)");
        yield return new ConverterTestData(new NullableDecimalToStringTypeConverter(), "wrong", null, "NullableDecimalToString (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new NullableDecimalToStringTypeConverter(), null, null, "NullableDecimalToString (Success - Null)");

        // NullableDoubleToString
        yield return new ConverterTestData(new NullableDoubleToStringTypeConverter(), 2.718, 2.718.ToString(), "NullableDoubleToString (Success)");
        yield return new ConverterTestData(new NullableDoubleToStringTypeConverter(), "wrong", null, "NullableDoubleToString (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new NullableDoubleToStringTypeConverter(), null, null, "NullableDoubleToString (Success - Null)");

        // NullableIntegerToString
        yield return new ConverterTestData(new NullableIntegerToStringTypeConverter(), 123, "123", "NullableIntegerToString (Success)");
        yield return new ConverterTestData(new NullableIntegerToStringTypeConverter(), "wrong", null, "NullableIntegerToString (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new NullableIntegerToStringTypeConverter(), null, null, "NullableIntegerToString (Success - Null)");

        // NullableLongToString
        yield return new ConverterTestData(new NullableLongToStringTypeConverter(), 9876543210L, "9876543210", "NullableLongToString (Success)");
        yield return new ConverterTestData(new NullableLongToStringTypeConverter(), "wrong", null, "NullableLongToString (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new NullableLongToStringTypeConverter(), null, null, "NullableLongToString (Success - Null)");

        // NullableShortToString
        yield return new ConverterTestData(new NullableShortToStringTypeConverter(), (short)321, "321", "NullableShortToString (Success)");
        yield return new ConverterTestData(new NullableShortToStringTypeConverter(), "wrong", null, "NullableShortToString (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new NullableShortToStringTypeConverter(), null, null, "NullableShortToString (Success - Null)");

        // NullableSingleToString
        yield return new ConverterTestData(new NullableSingleToStringTypeConverter(), 1.5f, 1.5f.ToString(), "NullableSingleToString (Success)");
        yield return new ConverterTestData(new NullableSingleToStringTypeConverter(), "wrong", null, "NullableSingleToString (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new NullableSingleToStringTypeConverter(), null, null, "NullableSingleToString (Success - Null)");

        // ===================================================================
        // Value-to-Nullable converters (implement IBindingTypeConverter directly)
        // ===================================================================

        // DoubleToNullableDouble
        yield return new ConverterTestData(new DoubleToNullableDoubleTypeConverter(), 3.14, (double?)3.14, "DoubleToNullableDouble (Success)");
        yield return new ConverterTestData(new DoubleToNullableDoubleTypeConverter(), "wrong", null, "DoubleToNullableDouble (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new DoubleToNullableDoubleTypeConverter(), null, null, "DoubleToNullableDouble (Failure - Null)", false);

        // IntegerToNullableInteger
        yield return new ConverterTestData(new IntegerToNullableIntegerTypeConverter(), 42, (int?)42, "IntegerToNullableInteger (Success)");
        yield return new ConverterTestData(new IntegerToNullableIntegerTypeConverter(), "wrong", null, "IntegerToNullableInteger (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new IntegerToNullableIntegerTypeConverter(), null, null, "IntegerToNullableInteger (Failure - Null)", false);

        // ByteToNullableByte
        yield return new ConverterTestData(new ByteToNullableByteTypeConverter(), (byte)7, (byte?)7, "ByteToNullableByte (Success)");
        yield return new ConverterTestData(new ByteToNullableByteTypeConverter(), "wrong", null, "ByteToNullableByte (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new ByteToNullableByteTypeConverter(), null, null, "ByteToNullableByte (Failure - Null)", false);

        // LongToNullableLong
        yield return new ConverterTestData(new LongToNullableLongTypeConverter(), 9876543210L, (long?)9876543210L, "LongToNullableLong (Success)");
        yield return new ConverterTestData(new LongToNullableLongTypeConverter(), "wrong", null, "LongToNullableLong (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new LongToNullableLongTypeConverter(), null, null, "LongToNullableLong (Failure - Null)", false);

        // ShortToNullableShort
        yield return new ConverterTestData(new ShortToNullableShortTypeConverter(), (short)321, (short?)321, "ShortToNullableShort (Success)");
        yield return new ConverterTestData(new ShortToNullableShortTypeConverter(), "wrong", null, "ShortToNullableShort (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new ShortToNullableShortTypeConverter(), null, null, "ShortToNullableShort (Failure - Null)", false);

        // SingleToNullableSingle
        yield return new ConverterTestData(new SingleToNullableSingleTypeConverter(), 1.5f, (float?)1.5f, "SingleToNullableSingle (Success)");
        yield return new ConverterTestData(new SingleToNullableSingleTypeConverter(), "wrong", null, "SingleToNullableSingle (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new SingleToNullableSingleTypeConverter(), null, null, "SingleToNullableSingle (Failure - Null)", false);

        // DecimalToNullableDecimal
        yield return new ConverterTestData(new DecimalToNullableDecimalTypeConverter(), 99.99m, (decimal?)99.99m, "DecimalToNullableDecimal (Success)");
        yield return new ConverterTestData(new DecimalToNullableDecimalTypeConverter(), "wrong", null, "DecimalToNullableDecimal (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new DecimalToNullableDecimalTypeConverter(), null, null, "DecimalToNullableDecimal (Failure - Null)", false);

        // ===================================================================
        // Nullable-to-Value converters (implement IBindingTypeConverter directly)
        // ===================================================================

        // NullableDoubleToDouble
        yield return new ConverterTestData(new NullableDoubleToDoubleTypeConverter(), 3.14, 3.14, "NullableDoubleToDouble (Success)");
        yield return new ConverterTestData(new NullableDoubleToDoubleTypeConverter(), "wrong", null, "NullableDoubleToDouble (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new NullableDoubleToDoubleTypeConverter(), null, null, "NullableDoubleToDouble (Failure - Null)", false);

        // NullableIntegerToInteger
        yield return new ConverterTestData(new NullableIntegerToIntegerTypeConverter(), 42, 42, "NullableIntegerToInteger (Success)");
        yield return new ConverterTestData(new NullableIntegerToIntegerTypeConverter(), "wrong", null, "NullableIntegerToInteger (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new NullableIntegerToIntegerTypeConverter(), null, null, "NullableIntegerToInteger (Failure - Null)", false);

        // NullableByteToByte
        yield return new ConverterTestData(new NullableByteToByteTypeConverter(), (byte)7, (byte)7, "NullableByteToByte (Success)");
        yield return new ConverterTestData(new NullableByteToByteTypeConverter(), "wrong", null, "NullableByteToByte (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new NullableByteToByteTypeConverter(), null, null, "NullableByteToByte (Failure - Null)", false);

        // NullableLongToLong
        yield return new ConverterTestData(new NullableLongToLongTypeConverter(), 9876543210L, 9876543210L, "NullableLongToLong (Success)");
        yield return new ConverterTestData(new NullableLongToLongTypeConverter(), "wrong", null, "NullableLongToLong (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new NullableLongToLongTypeConverter(), null, null, "NullableLongToLong (Failure - Null)", false);

        // NullableShortToShort
        yield return new ConverterTestData(new NullableShortToShortTypeConverter(), (short)321, (short)321, "NullableShortToShort (Success)");
        yield return new ConverterTestData(new NullableShortToShortTypeConverter(), "wrong", null, "NullableShortToShort (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new NullableShortToShortTypeConverter(), null, null, "NullableShortToShort (Failure - Null)", false);

        // NullableSingleToSingle
        yield return new ConverterTestData(new NullableSingleToSingleTypeConverter(), 1.5f, 1.5f, "NullableSingleToSingle (Success)");
        yield return new ConverterTestData(new NullableSingleToSingleTypeConverter(), "wrong", null, "NullableSingleToSingle (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new NullableSingleToSingleTypeConverter(), null, null, "NullableSingleToSingle (Failure - Null)", false);

        // NullableDecimalToDecimal
        yield return new ConverterTestData(new NullableDecimalToDecimalTypeConverter(), 99.99m, 99.99m, "NullableDecimalToDecimal (Success)");
        yield return new ConverterTestData(new NullableDecimalToDecimalTypeConverter(), "wrong", null, "NullableDecimalToDecimal (Failure - Wrong Type)", false);
        yield return new ConverterTestData(new NullableDecimalToDecimalTypeConverter(), null, null, "NullableDecimalToDecimal (Failure - Null)", false);
    }

    /// <summary>
    /// Executes the converter test cases.
    /// </summary>
    /// <param name="data">The test data.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    [MethodDataSource(nameof(TestCases))]
    public async Task RunTest(ConverterTestData data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var success = data.Converter.TryConvertTyped(data.Input, null, out var result);

        if (data.ExpectSuccess)
        {
            await Assert.That(success).IsTrue();
            await Assert.That(result).IsEqualTo(data.ExpectedOutput);
        }
        else
        {
            await Assert.That(success).IsFalse();
        }
    }

    /// <summary>
    /// Test data for converter tests.
    /// </summary>
    /// <param name="Converter">The converter.</param>
    /// <param name="Input">The input value.</param>
    /// <param name="ExpectedOutput">The expected output value.</param>
    /// <param name="Description">The description.</param>
    /// <param name="ExpectSuccess">Whether success is expected.</param>
    public sealed record ConverterTestData(
        IBindingTypeConverter Converter,
        object? Input,
        object? ExpectedOutput,
        string Description,
        bool ExpectSuccess = true)
    {
        /// <inheritdoc/>
        public override string ToString() => Description;
    }
}
