// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.ConvertersCustom;

/// <summary>Examples of creating and using custom binding converters.</summary>
public static class CustomConvertersExamples
{
    /// <summary>Registers a custom TodoPriority-to-color converter and demonstrates its use.</summary>
    /// <param name="viewModel">The view model containing todo items.</param>
    public static void RegisterAndUseCustomConverter(TodoListViewModel viewModel)
    {
        var converter = new TodoPriorityToColorConverter();
        BindingConverters.Current.TypedConverters.Register(converter);

        var firstItem = viewModel.Items[0];

        var originalPriority = firstItem.Priority;

        firstItem.Priority = TodoPriority.High;
        var success = converter.TryConvert(firstItem.Priority, null, out var highColor);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal("Red", highColor);

        firstItem.Priority = TodoPriority.Low;
        success = converter.TryConvert(firstItem.Priority, null, out var lowColor);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal("Gray", lowColor);

        firstItem.Priority = originalPriority;
    }

    /// <summary>Demonstrates looking up color names for all priority levels.</summary>
    public static void DisplayPriorityColors()
    {
        var converter = new TodoPriorityToColorConverter();

        foreach (var priority in Enum.GetValues<TodoPriority>())
        {
            var success = converter.TryConvert(priority, null, out var colorName);
            SampleCheck.Equal(true, success);
            SampleCheck.Equal(true, !string.IsNullOrEmpty(colorName));
        }
    }
}
