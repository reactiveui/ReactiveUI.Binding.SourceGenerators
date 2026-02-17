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
    // ===================================================================
    // Non-nullable to String converters
    // ===================================================================

    /// <summary>
    /// Verifies ByteToStringTypeConverter converts a byte value to its string representation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ByteToString_Success() =>
        await AssertConverterSuccess(new ByteToStringTypeConverter(), (byte)42, "42");

    /// <summary>
    /// Verifies ByteToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ByteToString_Failure_WrongType() =>
        await AssertConverterFailure(new ByteToStringTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new IntegerToStringTypeConverter(), 123, "123");

    /// <summary>
    /// Verifies IntegerToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IntegerToString_Failure_WrongType() =>
        await AssertConverterFailure(new IntegerToStringTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new StringToIntegerTypeConverter(), "456", 456);

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
        await AssertConverterSuccess(new DoubleToStringTypeConverter(), 3.14, 3.14.ToString());

    /// <summary>
    /// Verifies DoubleToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DoubleToString_Failure_WrongType() =>
        await AssertConverterFailure(new DoubleToStringTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new LongToStringTypeConverter(), 9876543210L, "9876543210");

    /// <summary>
    /// Verifies LongToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LongToString_Failure_WrongType() =>
        await AssertConverterFailure(new LongToStringTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new ShortToStringTypeConverter(), (short)321, "321");

    /// <summary>
    /// Verifies ShortToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShortToString_Failure_WrongType() =>
        await AssertConverterFailure(new ShortToStringTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new SingleToStringTypeConverter(), 1.5f, 1.5f.ToString());

    /// <summary>
    /// Verifies SingleToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleToString_Failure_WrongType() =>
        await AssertConverterFailure(new SingleToStringTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new NullableByteToStringTypeConverter(), (byte)42, "42");

    /// <summary>
    /// Verifies NullableByteToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableByteToString_Failure_WrongType() =>
        await AssertConverterFailure(new NullableByteToStringTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new NullableDecimalToStringTypeConverter(), 99.99m, 99.99m.ToString());

    /// <summary>
    /// Verifies NullableDecimalToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDecimalToString_Failure_WrongType() =>
        await AssertConverterFailure(new NullableDecimalToStringTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new NullableDoubleToStringTypeConverter(), 2.718, 2.718.ToString());

    /// <summary>
    /// Verifies NullableDoubleToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDoubleToString_Failure_WrongType() =>
        await AssertConverterFailure(new NullableDoubleToStringTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new NullableIntegerToStringTypeConverter(), 123, "123");

    /// <summary>
    /// Verifies NullableIntegerToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableIntegerToString_Failure_WrongType() =>
        await AssertConverterFailure(new NullableIntegerToStringTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new NullableLongToStringTypeConverter(), 9876543210L, "9876543210");

    /// <summary>
    /// Verifies NullableLongToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableLongToString_Failure_WrongType() =>
        await AssertConverterFailure(new NullableLongToStringTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new NullableShortToStringTypeConverter(), (short)321, "321");

    /// <summary>
    /// Verifies NullableShortToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableShortToString_Failure_WrongType() =>
        await AssertConverterFailure(new NullableShortToStringTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new NullableSingleToStringTypeConverter(), 1.5f, 1.5f.ToString());

    /// <summary>
    /// Verifies NullableSingleToStringTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableSingleToString_Failure_WrongType() =>
        await AssertConverterFailure(new NullableSingleToStringTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new DoubleToNullableDoubleTypeConverter(), 3.14, (double?)3.14);

    /// <summary>
    /// Verifies DoubleToNullableDoubleTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DoubleToNullableDouble_Failure_WrongType() =>
        await AssertConverterFailure(new DoubleToNullableDoubleTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new IntegerToNullableIntegerTypeConverter(), 42, (int?)42);

    /// <summary>
    /// Verifies IntegerToNullableIntegerTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task IntegerToNullableInteger_Failure_WrongType() =>
        await AssertConverterFailure(new IntegerToNullableIntegerTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new ByteToNullableByteTypeConverter(), (byte)7, (byte?)7);

    /// <summary>
    /// Verifies ByteToNullableByteTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ByteToNullableByte_Failure_WrongType() =>
        await AssertConverterFailure(new ByteToNullableByteTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new LongToNullableLongTypeConverter(), 9876543210L, (long?)9876543210L);

    /// <summary>
    /// Verifies LongToNullableLongTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task LongToNullableLong_Failure_WrongType() =>
        await AssertConverterFailure(new LongToNullableLongTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new ShortToNullableShortTypeConverter(), (short)321, (short?)321);

    /// <summary>
    /// Verifies ShortToNullableShortTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ShortToNullableShort_Failure_WrongType() =>
        await AssertConverterFailure(new ShortToNullableShortTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new SingleToNullableSingleTypeConverter(), 1.5f, (float?)1.5f);

    /// <summary>
    /// Verifies SingleToNullableSingleTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SingleToNullableSingle_Failure_WrongType() =>
        await AssertConverterFailure(new SingleToNullableSingleTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new DecimalToNullableDecimalTypeConverter(), 99.99m, (decimal?)99.99m);

    /// <summary>
    /// Verifies DecimalToNullableDecimalTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DecimalToNullableDecimal_Failure_WrongType() =>
        await AssertConverterFailure(new DecimalToNullableDecimalTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new NullableDoubleToDoubleTypeConverter(), 3.14, 3.14);

    /// <summary>
    /// Verifies NullableDoubleToDoubleTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDoubleToDouble_Failure_WrongType() =>
        await AssertConverterFailure(new NullableDoubleToDoubleTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new NullableIntegerToIntegerTypeConverter(), 42, 42);

    /// <summary>
    /// Verifies NullableIntegerToIntegerTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableIntegerToInteger_Failure_WrongType() =>
        await AssertConverterFailure(new NullableIntegerToIntegerTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new NullableByteToByteTypeConverter(), (byte)7, (byte)7);

    /// <summary>
    /// Verifies NullableByteToByteTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableByteToByte_Failure_WrongType() =>
        await AssertConverterFailure(new NullableByteToByteTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new NullableLongToLongTypeConverter(), 9876543210L, 9876543210L);

    /// <summary>
    /// Verifies NullableLongToLongTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableLongToLong_Failure_WrongType() =>
        await AssertConverterFailure(new NullableLongToLongTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new NullableShortToShortTypeConverter(), (short)321, (short)321);

    /// <summary>
    /// Verifies NullableShortToShortTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableShortToShort_Failure_WrongType() =>
        await AssertConverterFailure(new NullableShortToShortTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new NullableSingleToSingleTypeConverter(), 1.5f, 1.5f);

    /// <summary>
    /// Verifies NullableSingleToSingleTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableSingleToSingle_Failure_WrongType() =>
        await AssertConverterFailure(new NullableSingleToSingleTypeConverter(), "wrong");

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
        await AssertConverterSuccess(new NullableDecimalToDecimalTypeConverter(), 99.99m, 99.99m);

    /// <summary>
    /// Verifies NullableDecimalToDecimalTypeConverter fails when given a wrong type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDecimalToDecimal_Failure_WrongType() =>
        await AssertConverterFailure(new NullableDecimalToDecimalTypeConverter(), "wrong");

    /// <summary>
    /// Verifies NullableDecimalToDecimalTypeConverter fails when given null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NullableDecimalToDecimal_Failure_Null() =>
        await AssertConverterFailure(new NullableDecimalToDecimalTypeConverter(), null);

    private static async Task AssertConverterSuccess(IBindingTypeConverter converter, object? input, object? expectedOutput)
    {
        var success = converter.TryConvertTyped(input, null, out var result);
        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(expectedOutput);
    }

    private static async Task AssertConverterFailure(IBindingTypeConverter converter, object? input)
    {
        var success = converter.TryConvertTyped(input, null, out _);
        await Assert.That(success).IsFalse();
    }
}
