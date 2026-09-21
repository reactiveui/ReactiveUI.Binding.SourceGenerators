// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Demonstrates the converters that wrap a number in its nullable type and unwrap it again.</summary>
public static class NullableWrapperExamples
{
    /// <summary>An RGB color channel value (0-255).</summary>
    private const byte ColorChannelValue = 255;

    /// <summary>A TCP port number for network configuration.</summary>
    private const short PortNumber = 8080;

    /// <summary>A GitHub issue number.</summary>
    private const int IssueNumber = 42;

    /// <summary>A file size in bytes (512 KB for cloud storage).</summary>
    private const long FileSize = 524_288L;

    /// <summary>A scientific constant (pi).</summary>
    private const float Pi = 3.14F;

    /// <summary>A mathematical constant (Euler's number).</summary>
    private const double EulersNumber = 2.71828D;

    /// <summary>A customer's transfer amount in a banking app.</summary>
    private const decimal TransferAmount = 250.50M;

    /// <summary>Wraps whole numbers in their nullable types and shows each converter's type pair and affinity.</summary>
    public static void WrapWholeNumbers()
    {
        var wrapByte = new ByteToNullableByteTypeConverter();
        _ = wrapByte.TryConvertTyped(ColorChannelValue, conversionHint: null, out var colorChannel);
        Console.WriteLine($"{wrapByte.FromType} -> {wrapByte.ToType}: {colorChannel} (affinity {wrapByte.GetAffinityForObjects()})");

        var wrapShort = new ShortToNullableShortTypeConverter();
        _ = wrapShort.TryConvertTyped(PortNumber, conversionHint: null, out var portNumber);
        Console.WriteLine($"{wrapShort.FromType} -> {wrapShort.ToType}: {portNumber} (affinity {wrapShort.GetAffinityForObjects()})");

        var wrapInteger = new IntegerToNullableIntegerTypeConverter();
        _ = wrapInteger.TryConvertTyped(IssueNumber, conversionHint: null, out var issueNumber);
        Console.WriteLine($"{wrapInteger.FromType} -> {wrapInteger.ToType}: {issueNumber} (affinity {wrapInteger.GetAffinityForObjects()})");

        var wrapLong = new LongToNullableLongTypeConverter();
        _ = wrapLong.TryConvertTyped(FileSize, conversionHint: null, out var fileSize);
        Console.WriteLine($"{wrapLong.FromType} -> {wrapLong.ToType}: {fileSize} (affinity {wrapLong.GetAffinityForObjects()})");

        // Output:
        // System.Byte -> System.Nullable`1[System.Byte]: 255 (affinity 2)
        // System.Int16 -> System.Nullable`1[System.Int16]: 8080 (affinity 2)
        // System.Int32 -> System.Nullable`1[System.Int32]: 42 (affinity 2)
        // System.Int64 -> System.Nullable`1[System.Int64]: 524288 (affinity 2)
    }

    /// <summary>Wraps fractional numbers in their nullable types and shows each converter's type pair and affinity.</summary>
    public static void WrapFractionalNumbers()
    {
        var wrapSingle = new SingleToNullableSingleTypeConverter();
        _ = wrapSingle.TryConvertTyped(Pi, conversionHint: null, out var pi);
        Console.WriteLine($"{wrapSingle.FromType} -> {wrapSingle.ToType}: {pi} (affinity {wrapSingle.GetAffinityForObjects()})");

        var wrapDouble = new DoubleToNullableDoubleTypeConverter();
        _ = wrapDouble.TryConvertTyped(EulersNumber, conversionHint: null, out var eulersNumber);
        Console.WriteLine($"{wrapDouble.FromType} -> {wrapDouble.ToType}: {eulersNumber} (affinity {wrapDouble.GetAffinityForObjects()})");

        var wrapDecimal = new DecimalToNullableDecimalTypeConverter();
        _ = wrapDecimal.TryConvertTyped(TransferAmount, conversionHint: null, out var transferAmount);
        Console.WriteLine($"{wrapDecimal.FromType} -> {wrapDecimal.ToType}: {transferAmount} (affinity {wrapDecimal.GetAffinityForObjects()})");

        // Output:
        // System.Single -> System.Nullable`1[System.Single]: 3.14 (affinity 2)
        // System.Double -> System.Nullable`1[System.Double]: 2.71828 (affinity 2)
        // System.Decimal -> System.Nullable`1[System.Decimal]: 250.50 (affinity 2)
    }

    /// <summary>Unwraps nullable whole numbers and shows each converter's type pair and affinity.</summary>
    public static void UnwrapWholeNumbers()
    {
        var unwrapByte = new NullableByteToByteTypeConverter();
        _ = unwrapByte.TryConvertTyped((byte?)ColorChannelValue, conversionHint: null, out var colorChannel);
        Console.WriteLine($"{unwrapByte.FromType} -> {unwrapByte.ToType}: {colorChannel} (affinity {unwrapByte.GetAffinityForObjects()})");

        var unwrapShort = new NullableShortToShortTypeConverter();
        _ = unwrapShort.TryConvertTyped((short?)PortNumber, conversionHint: null, out var portNumber);
        Console.WriteLine($"{unwrapShort.FromType} -> {unwrapShort.ToType}: {portNumber} (affinity {unwrapShort.GetAffinityForObjects()})");

        var unwrapInteger = new NullableIntegerToIntegerTypeConverter();
        _ = unwrapInteger.TryConvertTyped((int?)IssueNumber, conversionHint: null, out var issueNumber);
        Console.WriteLine($"{unwrapInteger.FromType} -> {unwrapInteger.ToType}: {issueNumber} (affinity {unwrapInteger.GetAffinityForObjects()})");

        var unwrapLong = new NullableLongToLongTypeConverter();
        _ = unwrapLong.TryConvertTyped((long?)FileSize, conversionHint: null, out var fileSize);
        Console.WriteLine($"{unwrapLong.FromType} -> {unwrapLong.ToType}: {fileSize} (affinity {unwrapLong.GetAffinityForObjects()})");

        // Output:
        // System.Nullable`1[System.Byte] -> System.Byte: 255 (affinity 2)
        // System.Nullable`1[System.Int16] -> System.Int16: 8080 (affinity 2)
        // System.Nullable`1[System.Int32] -> System.Int32: 42 (affinity 2)
        // System.Nullable`1[System.Int64] -> System.Int64: 524288 (affinity 2)
    }

    /// <summary>Unwraps nullable fractional numbers and shows each converter's type pair and affinity.</summary>
    public static void UnwrapFractionalNumbers()
    {
        var unwrapSingle = new NullableSingleToSingleTypeConverter();
        _ = unwrapSingle.TryConvertTyped((float?)Pi, conversionHint: null, out var pi);
        Console.WriteLine($"{unwrapSingle.FromType} -> {unwrapSingle.ToType}: {pi} (affinity {unwrapSingle.GetAffinityForObjects()})");

        var unwrapDouble = new NullableDoubleToDoubleTypeConverter();
        _ = unwrapDouble.TryConvertTyped((double?)EulersNumber, conversionHint: null, out var eulersNumber);
        Console.WriteLine($"{unwrapDouble.FromType} -> {unwrapDouble.ToType}: {eulersNumber} (affinity {unwrapDouble.GetAffinityForObjects()})");

        var unwrapDecimal = new NullableDecimalToDecimalTypeConverter();
        _ = unwrapDecimal.TryConvertTyped((decimal?)TransferAmount, conversionHint: null, out var transferAmount);
        Console.WriteLine($"{unwrapDecimal.FromType} -> {unwrapDecimal.ToType}: {transferAmount} (affinity {unwrapDecimal.GetAffinityForObjects()})");

        // Output:
        // System.Nullable`1[System.Single] -> System.Single: 3.14 (affinity 2)
        // System.Nullable`1[System.Double] -> System.Double: 2.71828 (affinity 2)
        // System.Nullable`1[System.Decimal] -> System.Decimal: 250.50 (affinity 2)
    }
}
