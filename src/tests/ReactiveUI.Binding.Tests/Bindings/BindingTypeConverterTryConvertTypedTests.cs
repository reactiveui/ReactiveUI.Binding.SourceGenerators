// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>
/// Unit tests for <see cref="BindingTypeConverter{TFrom, TTo}.TryConvertTyped(object?, object?, out object?)"/>
/// to ensure coverage of the base class dispatch path.
/// </summary>
public class BindingTypeConverterTryConvertTypedTests
{
    /// <summary>
    /// Literal used as a deliberately wrong-typed conversion input.
    /// </summary>
    private const string WrongTypeInput = "wrong";

    /// <summary>
    /// Sample byte value used for byte conversion tests.
    /// </summary>
    private const byte ByteValue = 42;

    /// <summary>
    /// Smaller byte value used for byte conversion tests.
    /// </summary>
    private const byte SmallByteValue = 7;

    /// <summary>
    /// Sample integer value used for integer conversion tests.
    /// </summary>
    private const int IntValue = 123;

    /// <summary>
    /// Smaller integer value used for integer conversion tests.
    /// </summary>
    private const int SmallIntValue = 42;

    /// <summary>
    /// Integer value parsed from a string in conversion tests.
    /// </summary>
    private const int ParsedIntValue = 456;

    /// <summary>
    /// Sample double value used for double conversion tests.
    /// </summary>
    private const double DoubleValue = 3.14;

    /// <summary>
    /// Alternate double value used for double conversion tests.
    /// </summary>
    private const double EulerDoubleValue = 2.718;

    /// <summary>
    /// Sample long value used for long conversion tests.
    /// </summary>
    private const long LongValue = 9_876_543_210L;

    /// <summary>
    /// Sample short value used for short conversion tests.
    /// </summary>
    private const short ShortValue = 321;

    /// <summary>
    /// Sample single-precision value used for float conversion tests.
    /// </summary>
    private const float SingleValue = 1.5f;

    /// <summary>
    /// Sample decimal value used for decimal conversion tests.
    /// </summary>
    private const decimal DecimalValue = 99.99m;

    // ===================================================================
    // Non-nullable to String converters
    // ===================================================================

    /// <summary>
    /// Verifies ByteToStringTypeConverter converts a byte value to its string representation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ByteToString_Success() =>
        await AssertConverterSuccess(new ByteToStringTypeConverter(), (byte)ByteValue, "42");

    /// <summary>
    /// Verifies ByteToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ByteToString_Failure_WrongType() =>
        await AssertConverterFailure(new ByteToStringTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies ByteToStringTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ByteToString_Failure_Null() =>
        await AssertConverterFailure(new ByteToStringTypeConverter(), null);

    /// <summary>
    /// Verifies IntegerToStringTypeConverter converts an int value to its string representation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IntegerToString_Success() =>
        await AssertConverterSuccess(new IntegerToStringTypeConverter(), IntValue, "123");

    /// <summary>
    /// Verifies IntegerToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IntegerToString_Failure_WrongType() =>
        await AssertConverterFailure(new IntegerToStringTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies IntegerToStringTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IntegerToString_Failure_Null() =>
        await AssertConverterFailure(new IntegerToStringTypeConverter(), null);

    /// <summary>
    /// Verifies StringToIntegerTypeConverter converts a string to an int.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task StringToInteger_Success() =>
        await AssertConverterSuccess(new StringToIntegerTypeConverter(), "456", ParsedIntValue);

    /// <summary>
    /// Verifies BooleanToStringTypeConverter converts true to "True".
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BooleanToString_Success() =>
        await AssertConverterSuccess(new BooleanToStringTypeConverter(), true, "True");

    /// <summary>
    /// Verifies DoubleToStringTypeConverter converts a double to its string representation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DoubleToString_Success() =>
        await AssertConverterSuccess(new DoubleToStringTypeConverter(), DoubleValue, DoubleValue.ToString(System.Globalization.CultureInfo.CurrentCulture));

    /// <summary>
    /// Verifies DoubleToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DoubleToString_Failure_WrongType() =>
        await AssertConverterFailure(new DoubleToStringTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies DoubleToStringTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DoubleToString_Failure_Null() =>
        await AssertConverterFailure(new DoubleToStringTypeConverter(), null);

    /// <summary>
    /// Verifies LongToStringTypeConverter converts a long to its string representation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LongToString_Success() =>
        await AssertConverterSuccess(new LongToStringTypeConverter(), LongValue, "9876543210");

    /// <summary>
    /// Verifies LongToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LongToString_Failure_WrongType() =>
        await AssertConverterFailure(new LongToStringTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies LongToStringTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LongToString_Failure_Null() =>
        await AssertConverterFailure(new LongToStringTypeConverter(), null);

    /// <summary>
    /// Verifies ShortToStringTypeConverter converts a short to its string representation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShortToString_Success() =>
        await AssertConverterSuccess(new ShortToStringTypeConverter(), (short)ShortValue, "321");

    /// <summary>
    /// Verifies ShortToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShortToString_Failure_WrongType() =>
        await AssertConverterFailure(new ShortToStringTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies ShortToStringTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShortToString_Failure_Null() =>
        await AssertConverterFailure(new ShortToStringTypeConverter(), null);

    /// <summary>
    /// Verifies SingleToStringTypeConverter converts a float to its string representation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleToString_Success() =>
        await AssertConverterSuccess(new SingleToStringTypeConverter(), SingleValue, SingleValue.ToString(System.Globalization.CultureInfo.CurrentCulture));

    /// <summary>
    /// Verifies SingleToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleToString_Failure_WrongType() =>
        await AssertConverterFailure(new SingleToStringTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies SingleToStringTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleToString_Failure_Null() =>
        await AssertConverterFailure(new SingleToStringTypeConverter(), null);

    // ===================================================================
    // Nullable to String converters
    // ===================================================================

    /// <summary>
    /// Verifies NullableByteToStringTypeConverter converts a byte to its string representation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableByteToString_Success() =>
        await AssertConverterSuccess(new NullableByteToStringTypeConverter(), (byte)ByteValue, "42");

    /// <summary>
    /// Verifies NullableByteToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableByteToString_Failure_WrongType() =>
        await AssertConverterFailure(new NullableByteToStringTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies NullableByteToStringTypeConverter succeeds when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableByteToString_Success_Null() =>
        await AssertConverterSuccess(new NullableByteToStringTypeConverter(), null, null);

    /// <summary>
    /// Verifies NullableDecimalToStringTypeConverter converts a decimal to its string representation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDecimalToString_Success() =>
        await AssertConverterSuccess(new NullableDecimalToStringTypeConverter(), DecimalValue, DecimalValue.ToString(System.Globalization.CultureInfo.CurrentCulture));

    /// <summary>
    /// Verifies NullableDecimalToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDecimalToString_Failure_WrongType() =>
        await AssertConverterFailure(new NullableDecimalToStringTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies NullableDecimalToStringTypeConverter succeeds when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDecimalToString_Success_Null() =>
        await AssertConverterSuccess(new NullableDecimalToStringTypeConverter(), null, null);

    /// <summary>
    /// Verifies NullableDoubleToStringTypeConverter converts a double to its string representation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDoubleToString_Success() =>
        await AssertConverterSuccess(new NullableDoubleToStringTypeConverter(), EulerDoubleValue, EulerDoubleValue.ToString(System.Globalization.CultureInfo.CurrentCulture));

    /// <summary>
    /// Verifies NullableDoubleToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDoubleToString_Failure_WrongType() =>
        await AssertConverterFailure(new NullableDoubleToStringTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies NullableDoubleToStringTypeConverter succeeds when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDoubleToString_Success_Null() =>
        await AssertConverterSuccess(new NullableDoubleToStringTypeConverter(), null, null);

    /// <summary>
    /// Verifies NullableIntegerToStringTypeConverter converts an int to its string representation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableIntegerToString_Success() =>
        await AssertConverterSuccess(new NullableIntegerToStringTypeConverter(), IntValue, "123");

    /// <summary>
    /// Verifies NullableIntegerToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableIntegerToString_Failure_WrongType() =>
        await AssertConverterFailure(new NullableIntegerToStringTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies NullableIntegerToStringTypeConverter succeeds when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableIntegerToString_Success_Null() =>
        await AssertConverterSuccess(new NullableIntegerToStringTypeConverter(), null, null);

    /// <summary>
    /// Verifies NullableLongToStringTypeConverter converts a long to its string representation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableLongToString_Success() =>
        await AssertConverterSuccess(new NullableLongToStringTypeConverter(), LongValue, "9876543210");

    /// <summary>
    /// Verifies NullableLongToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableLongToString_Failure_WrongType() =>
        await AssertConverterFailure(new NullableLongToStringTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies NullableLongToStringTypeConverter succeeds when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableLongToString_Success_Null() =>
        await AssertConverterSuccess(new NullableLongToStringTypeConverter(), null, null);

    /// <summary>
    /// Verifies NullableShortToStringTypeConverter converts a short to its string representation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableShortToString_Success() =>
        await AssertConverterSuccess(new NullableShortToStringTypeConverter(), (short)ShortValue, "321");

    /// <summary>
    /// Verifies NullableShortToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableShortToString_Failure_WrongType() =>
        await AssertConverterFailure(new NullableShortToStringTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies NullableShortToStringTypeConverter succeeds when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableShortToString_Success_Null() =>
        await AssertConverterSuccess(new NullableShortToStringTypeConverter(), null, null);

    /// <summary>
    /// Verifies NullableSingleToStringTypeConverter converts a float to its string representation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableSingleToString_Success() =>
        await AssertConverterSuccess(new NullableSingleToStringTypeConverter(), SingleValue, SingleValue.ToString(System.Globalization.CultureInfo.CurrentCulture));

    /// <summary>
    /// Verifies NullableSingleToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableSingleToString_Failure_WrongType() =>
        await AssertConverterFailure(new NullableSingleToStringTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies NullableSingleToStringTypeConverter succeeds when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableSingleToString_Success_Null() =>
        await AssertConverterSuccess(new NullableSingleToStringTypeConverter(), null, null);

    // ===================================================================
    // Value-to-Nullable converters
    // ===================================================================

    /// <summary>
    /// Verifies DoubleToNullableDoubleTypeConverter converts a double to nullable double.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DoubleToNullableDouble_Success() =>
        await AssertConverterSuccess(new DoubleToNullableDoubleTypeConverter(), DoubleValue, (double?)DoubleValue);

    /// <summary>
    /// Verifies DoubleToNullableDoubleTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DoubleToNullableDouble_Failure_WrongType() =>
        await AssertConverterFailure(new DoubleToNullableDoubleTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies DoubleToNullableDoubleTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DoubleToNullableDouble_Failure_Null() =>
        await AssertConverterFailure(new DoubleToNullableDoubleTypeConverter(), null);

    /// <summary>
    /// Verifies IntegerToNullableIntegerTypeConverter converts an int to nullable int.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IntegerToNullableInteger_Success() =>
        await AssertConverterSuccess(new IntegerToNullableIntegerTypeConverter(), SmallIntValue, (int?)SmallIntValue);

    /// <summary>
    /// Verifies IntegerToNullableIntegerTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IntegerToNullableInteger_Failure_WrongType() =>
        await AssertConverterFailure(new IntegerToNullableIntegerTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies IntegerToNullableIntegerTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IntegerToNullableInteger_Failure_Null() =>
        await AssertConverterFailure(new IntegerToNullableIntegerTypeConverter(), null);

    /// <summary>
    /// Verifies ByteToNullableByteTypeConverter converts a byte to nullable byte.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ByteToNullableByte_Success() =>
        await AssertConverterSuccess(new ByteToNullableByteTypeConverter(), (byte)SmallByteValue, (byte?)SmallByteValue);

    /// <summary>
    /// Verifies ByteToNullableByteTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ByteToNullableByte_Failure_WrongType() =>
        await AssertConverterFailure(new ByteToNullableByteTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies ByteToNullableByteTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ByteToNullableByte_Failure_Null() =>
        await AssertConverterFailure(new ByteToNullableByteTypeConverter(), null);

    /// <summary>
    /// Verifies LongToNullableLongTypeConverter converts a long to nullable long.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LongToNullableLong_Success() =>
        await AssertConverterSuccess(new LongToNullableLongTypeConverter(), LongValue, (long?)LongValue);

    /// <summary>
    /// Verifies LongToNullableLongTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LongToNullableLong_Failure_WrongType() =>
        await AssertConverterFailure(new LongToNullableLongTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies LongToNullableLongTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LongToNullableLong_Failure_Null() =>
        await AssertConverterFailure(new LongToNullableLongTypeConverter(), null);

    /// <summary>
    /// Verifies ShortToNullableShortTypeConverter converts a short to nullable short.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShortToNullableShort_Success() =>
        await AssertConverterSuccess(new ShortToNullableShortTypeConverter(), (short)ShortValue, (short?)ShortValue);

    /// <summary>
    /// Verifies ShortToNullableShortTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShortToNullableShort_Failure_WrongType() =>
        await AssertConverterFailure(new ShortToNullableShortTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies ShortToNullableShortTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShortToNullableShort_Failure_Null() =>
        await AssertConverterFailure(new ShortToNullableShortTypeConverter(), null);

    /// <summary>
    /// Verifies SingleToNullableSingleTypeConverter converts a float to nullable float.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleToNullableSingle_Success() =>
        await AssertConverterSuccess(new SingleToNullableSingleTypeConverter(), SingleValue, (float?)SingleValue);

    /// <summary>
    /// Verifies SingleToNullableSingleTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleToNullableSingle_Failure_WrongType() =>
        await AssertConverterFailure(new SingleToNullableSingleTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies SingleToNullableSingleTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleToNullableSingle_Failure_Null() =>
        await AssertConverterFailure(new SingleToNullableSingleTypeConverter(), null);

    /// <summary>
    /// Verifies DecimalToNullableDecimalTypeConverter converts a decimal to nullable decimal.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DecimalToNullableDecimal_Success() =>
        await AssertConverterSuccess(new DecimalToNullableDecimalTypeConverter(), DecimalValue, (decimal?)DecimalValue);

    /// <summary>
    /// Verifies DecimalToNullableDecimalTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DecimalToNullableDecimal_Failure_WrongType() =>
        await AssertConverterFailure(new DecimalToNullableDecimalTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies DecimalToNullableDecimalTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DecimalToNullableDecimal_Failure_Null() =>
        await AssertConverterFailure(new DecimalToNullableDecimalTypeConverter(), null);

    // ===================================================================
    // Nullable-to-Value converters
    // ===================================================================

    /// <summary>
    /// Verifies NullableDoubleToDoubleTypeConverter converts a double value successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDoubleToDouble_Success() =>
        await AssertConverterSuccess(new NullableDoubleToDoubleTypeConverter(), DoubleValue, DoubleValue);

    /// <summary>
    /// Verifies NullableDoubleToDoubleTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDoubleToDouble_Failure_WrongType() =>
        await AssertConverterFailure(new NullableDoubleToDoubleTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies NullableDoubleToDoubleTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDoubleToDouble_Failure_Null() =>
        await AssertConverterFailure(new NullableDoubleToDoubleTypeConverter(), null);

    /// <summary>
    /// Verifies NullableIntegerToIntegerTypeConverter converts an int value successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableIntegerToInteger_Success() =>
        await AssertConverterSuccess(new NullableIntegerToIntegerTypeConverter(), SmallIntValue, SmallIntValue);

    /// <summary>
    /// Verifies NullableIntegerToIntegerTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableIntegerToInteger_Failure_WrongType() =>
        await AssertConverterFailure(new NullableIntegerToIntegerTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies NullableIntegerToIntegerTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableIntegerToInteger_Failure_Null() =>
        await AssertConverterFailure(new NullableIntegerToIntegerTypeConverter(), null);

    /// <summary>
    /// Verifies NullableByteToByteTypeConverter converts a byte value successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableByteToByte_Success() =>
        await AssertConverterSuccess(new NullableByteToByteTypeConverter(), (byte)SmallByteValue, (byte)SmallByteValue);

    /// <summary>
    /// Verifies NullableByteToByteTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableByteToByte_Failure_WrongType() =>
        await AssertConverterFailure(new NullableByteToByteTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies NullableByteToByteTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableByteToByte_Failure_Null() =>
        await AssertConverterFailure(new NullableByteToByteTypeConverter(), null);

    /// <summary>
    /// Verifies NullableLongToLongTypeConverter converts a long value successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableLongToLong_Success() =>
        await AssertConverterSuccess(new NullableLongToLongTypeConverter(), LongValue, LongValue);

    /// <summary>
    /// Verifies NullableLongToLongTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableLongToLong_Failure_WrongType() =>
        await AssertConverterFailure(new NullableLongToLongTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies NullableLongToLongTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableLongToLong_Failure_Null() =>
        await AssertConverterFailure(new NullableLongToLongTypeConverter(), null);

    /// <summary>
    /// Verifies NullableShortToShortTypeConverter converts a short value successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableShortToShort_Success() =>
        await AssertConverterSuccess(new NullableShortToShortTypeConverter(), (short)ShortValue, (short)ShortValue);

    /// <summary>
    /// Verifies NullableShortToShortTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableShortToShort_Failure_WrongType() =>
        await AssertConverterFailure(new NullableShortToShortTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies NullableShortToShortTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableShortToShort_Failure_Null() =>
        await AssertConverterFailure(new NullableShortToShortTypeConverter(), null);

    /// <summary>
    /// Verifies NullableSingleToSingleTypeConverter converts a float value successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableSingleToSingle_Success() =>
        await AssertConverterSuccess(new NullableSingleToSingleTypeConverter(), SingleValue, SingleValue);

    /// <summary>
    /// Verifies NullableSingleToSingleTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableSingleToSingle_Failure_WrongType() =>
        await AssertConverterFailure(new NullableSingleToSingleTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies NullableSingleToSingleTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableSingleToSingle_Failure_Null() =>
        await AssertConverterFailure(new NullableSingleToSingleTypeConverter(), null);

    /// <summary>
    /// Verifies NullableDecimalToDecimalTypeConverter converts a decimal value successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDecimalToDecimal_Success() =>
        await AssertConverterSuccess(new NullableDecimalToDecimalTypeConverter(), DecimalValue, DecimalValue);

    /// <summary>
    /// Verifies NullableDecimalToDecimalTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDecimalToDecimal_Failure_WrongType() =>
        await AssertConverterFailure(new NullableDecimalToDecimalTypeConverter(), WrongTypeInput);

    /// <summary>
    /// Verifies NullableDecimalToDecimalTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDecimalToDecimal_Failure_Null() =>
        await AssertConverterFailure(new NullableDecimalToDecimalTypeConverter(), null);

    /// <summary>
    /// Asserts that the converter successfully converts the input to the expected output.
    /// </summary>
    /// <param name="converter">The converter to test.</param>
    /// <param name="input">The input value to convert.</param>
    /// <param name="expectedOutput">The expected conversion result.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    private static async Task AssertConverterSuccess(
        IBindingTypeConverter converter,
        object? input,
        object? expectedOutput)
    {
        var success = converter.TryConvertTyped(input, null, out var result);
        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(expectedOutput);
    }

    /// <summary>
    /// Asserts that the converter fails to convert the given input.
    /// </summary>
    /// <param name="converter">The converter to test.</param>
    /// <param name="input">The input value that should fail conversion.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    private static async Task AssertConverterFailure(IBindingTypeConverter converter, object? input)
    {
        var success = converter.TryConvertTyped(input, null, out _);
        await Assert.That(success).IsFalse();
    }
}
