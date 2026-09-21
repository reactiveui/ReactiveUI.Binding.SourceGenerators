// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Controls;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.ConvertersMigration;

/// <summary>Moves the converters an older app kept in a Splat resolver into the binding converter service.</summary>
public static class MigrationExamples
{
    /// <summary>Reads the converters out of the older app's Splat resolver.</summary>
    public static void ExtractConvertersFromLegacyResolver()
    {
        using var legacyResolver = LegacyAppDependencyResolver.Create();

        var extracted = ConverterMigrationHelper.ExtractConverters(legacyResolver);

        SampleCheck.Equal(1, extracted.TypedConverters.Count);
        SampleCheck.Equal(1, extracted.FallbackConverters.Count);
        SampleCheck.Equal(1, extracted.SetMethodConverters.Count);
        SampleCheck.Equal(typeof(LegacyPriorityToColorConverter), extracted.TypedConverters[0].GetType());
        SampleCheck.Equal(typeof(LegacyObjectToStringFallbackConverter), extracted.FallbackConverters[0].GetType());
        SampleCheck.Equal(typeof(LegacyTagListSetMethodConverter), extracted.SetMethodConverters[0].GetType());
    }

    /// <summary>Copies the older app's converters into a converter service and uses each one.</summary>
    public static void ImportLegacyConvertersIntoService()
    {
        using var legacyResolver = LegacyAppDependencyResolver.Create();
        ConverterService service = new();

        service.ImportFrom(legacyResolver);

        var typed = service.TypedConverters.TryGetConverter(typeof(TodoPriority), typeof(string));
        SampleCheck.Equal(true, typed is not null);
        SampleCheck.Equal(true, typed!.TryConvertTyped(TodoPriority.High, null, out var highColour));
        SampleCheck.Equal(LegacyPriorityToColorConverter.HighColour, highColour);

        var fallback = service.FallbackConverters.TryGetConverter(typeof(IssueState), typeof(string));
        SampleCheck.Equal(true, fallback is not null);
        SampleCheck.Equal(true, fallback!.TryConvert(typeof(IssueState), IssueState.Closed, typeof(string), null, out var stateText));
        SampleCheck.Equal(nameof(IssueState.Closed), stateText);

        var setMethod = service.SetMethodConverters.TryGetConverter(typeof(IReadOnlyList<string>), typeof(List<string>));
        SampleCheck.Equal(true, setMethod is not null);

        List<string> tagList = ["draft"];
        List<string> newTags = ["travel", "admin"];

        _ = setMethod!.PerformSet(tagList, newTags, null);

        SampleCheck.SequenceEqual(newTags, tagList);
    }

    /// <summary>Binds a priority badge with the converter the older app registered, passed to the binding as the converter override.</summary>
    public static void BindToWithMigratedConverterOverride()
    {
        using var legacyResolver = LegacyAppDependencyResolver.Create();

        BindingConverters.Current.ImportFrom(legacyResolver);

        var migrated = BindingConverters.Current.TypedConverters.TryGetConverter(typeof(TodoPriority), typeof(string));
        SampleCheck.Equal(true, migrated is not null);

        TodoItem item = new() { Title = "Renew passport", Priority = TodoPriority.High };
        LabelControl priorityBadge = new();

        using (item.WhenChanged(x => x.Priority).BindTo(priorityBadge, x => x.Text, migrated!))
        {
            SampleCheck.Equal(LegacyPriorityToColorConverter.HighColour, priorityBadge.Text);

            item.Priority = TodoPriority.Low;

            SampleCheck.Equal(LegacyPriorityToColorConverter.LowColour, priorityBadge.Text);
        }
    }

    /// <summary>Binds a priority badge and lets the binding find the migrated converter in the service.</summary>
    public static void BindToResolvesMigratedConverter()
    {
        using var legacyResolver = LegacyAppDependencyResolver.Create();

        BindingConverters.Current.ImportFrom(legacyResolver);

        TodoItem item = new() { Title = "Book dentist", Priority = TodoPriority.Normal };
        LabelControl priorityBadge = new();

        using (item.WhenChanged(x => x.Priority).BindTo(priorityBadge, x => x.Text))
        {
            SampleCheck.Equal(LegacyPriorityToColorConverter.NormalColour, priorityBadge.Text);

            item.Priority = TodoPriority.High;

            SampleCheck.Equal(LegacyPriorityToColorConverter.HighColour, priorityBadge.Text);
        }
    }
}
