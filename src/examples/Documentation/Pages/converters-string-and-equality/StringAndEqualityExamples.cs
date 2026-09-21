// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.StringAndEquality;

/// <summary>Examples of using built-in converters for strings, booleans, and equality.</summary>
public static class StringAndEqualityExamples
{
    /// <summary>Demonstrates the StringConverter for identity string-to-string binding.</summary>
    public static void DemonstrateStringConverter()
    {
        var converter = new StringConverter();
        var success = converter.TryConvertTyped("Hello", null, out var result);

        SampleCheck.Equal(true, success);
        SampleCheck.Equal("Hello", result);

        var affinity = converter.GetAffinityForObjects();

        SampleCheck.Equal(true, affinity > 0);
        SampleCheck.Equal(typeof(string), converter.FromType);
        SampleCheck.Equal(typeof(string), converter.ToType);
    }

    /// <summary>Demonstrates the EqualityTypeConverter for comparing values to a hint.</summary>
    /// <param name="viewModel">The view model containing todo items.</param>
    public static void DemonstrateEqualityConverter(TodoListViewModel viewModel)
    {
        var converter = new EqualityTypeConverter();
        var firstItem = viewModel.Items[0];
        var originalPriority = firstItem.Priority;

        firstItem.Priority = TodoPriority.High;

        var success = converter.TryConvertTyped(firstItem.Priority, TodoPriority.High, out var result);

        SampleCheck.Equal(true, success);
        SampleCheck.Equal(true, (bool)result!);

        success = converter.TryConvertTyped(firstItem.Priority, TodoPriority.Low, out result);

        SampleCheck.Equal(true, success);
        SampleCheck.Equal(false, (bool)result!);

        firstItem.Priority = originalPriority;
    }

    /// <summary>Demonstrates boolean-to-string and string-to-boolean converters.</summary>
    public static void DemonstrateBooleanConverters()
    {
        var boolToStringConverter = new BooleanToStringTypeConverter();
        var trueSuccess = boolToStringConverter.TryConvertTyped(true, null, out var trueResult);

        SampleCheck.Equal(true, trueSuccess);
        SampleCheck.Equal("True", trueResult);

        var falseSuccess = boolToStringConverter.TryConvertTyped(false, null, out var falseResult);

        SampleCheck.Equal(true, falseSuccess);
        SampleCheck.Equal("False", falseResult);

        var stringToBoolConverter = new StringToBooleanTypeConverter();
        var stringTrueSuccess = stringToBoolConverter.TryConvertTyped("True", null, out var parsedTrue);

        SampleCheck.Equal(true, stringTrueSuccess);
        SampleCheck.Equal(true, (bool)parsedTrue!);

        var stringFalseSuccess = stringToBoolConverter.TryConvertTyped("False", null, out var parsedFalse);

        SampleCheck.Equal(true, stringFalseSuccess);
        SampleCheck.Equal(false, (bool)parsedFalse!);
    }
}
