// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Demonstrates date and time type converters.</summary>
public static class DateTimeExamples
{
    /// <summary>The text of a todo due date and time.</summary>
    private const string DueDateTimeText = "2025-12-25T10:30:00";

    /// <summary>The text of a file modification time with its timezone offset.</summary>
    private const string ModifiedTimeText = "2025-09-21T10:30:00+00:00";

    /// <summary>The text of a todo due date.</summary>
    private const string DueDateText = "2025-12-25";

    /// <summary>The text of an appointment start time.</summary>
    private const string StartTimeText = "14:30:00";

    /// <summary>The text of a project duration.</summary>
    private const string DurationText = "01:30:00";

    /// <summary>A todo item's due date and time for task scheduling.</summary>
    private static readonly DateTime TodoDueDateTime = new(2025, 12, 25, 10, 30, 0, DateTimeKind.Unspecified);

    /// <summary>A cloud storage file's last modification time with timezone information.</summary>
    private static readonly DateTimeOffset FileLastModifiedTime = new(2025, 9, 21, 10, 30, 0, TimeSpan.Zero);

    /// <summary>A todo item's due date in a task management app.</summary>
    private static readonly DateOnly TodoDueDate = new(2025, 12, 25);

    /// <summary>An appointment's start time in a calendar application.</summary>
    private static readonly TimeOnly AppointmentStartTime = new(14, 30, 0);

    /// <summary>A project's estimated duration for completion.</summary>
    private static readonly TimeSpan ProjectDuration = new(1, 30, 0);

    /// <summary>Parses a todo item's due date and time that a user typed into the task app.</summary>
    public static void ParseTodoDueDateTime()
    {
        var converter = new StringToDateTimeTypeConverter();

        // Success case: valid date-time
        var success = converter.TryConvert("2025-12-25T10:30:00", conversionHint: null, out var dueDateTime);
        Console.WriteLine(success);
        Console.WriteLine(dueDateTime);

        // Failure case: invalid date format
        var failure = converter.TryConvert("not-a-date", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 12/25/2025 10:30:00
        // False
    }

    /// <summary>Formats a todo item's due date-time for display in a task list.</summary>
    public static void FormatTodoDueDateTime()
    {
        var converter = new DateTimeToStringTypeConverter();

        var success = converter.TryConvert(TodoDueDateTime, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 12/25/2025 10:30:00
    }

    /// <summary>Parses a cloud storage file's last modified time that includes timezone information.</summary>
    public static void ParseCloudStorageModificationTime()
    {
        var converter = new StringToDateTimeOffsetTypeConverter();

        // Success case: valid date-time with timezone
        var success = converter.TryConvert("2025-09-21T10:30:00+00:00", conversionHint: null, out var modifiedTime);
        Console.WriteLine(success);
        Console.WriteLine(modifiedTime);

        // Failure case: invalid timezone format
        var failure = converter.TryConvert("invalid-date-time", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 09/21/2025 10:30:00 +00:00
        // False
    }

    /// <summary>Formats a cloud storage file's modification time for display.</summary>
    public static void FormatCloudStorageModificationTime()
    {
        var converter = new DateTimeOffsetToStringTypeConverter();

        var success = converter.TryConvert(FileLastModifiedTime, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 09/21/2025 10:30:00 +00:00
    }

    /// <summary>Parses a todo item's due date that a user typed in the date picker.</summary>
    public static void ParseTodoDueDate()
    {
        var converter = new StringToDateOnlyTypeConverter();

        // Success case: valid date
        var success = converter.TryConvert("2025-12-25", conversionHint: null, out var dueDate);
        Console.WriteLine(success);
        Console.WriteLine(dueDate);

        // Failure case: invalid date
        var failure = converter.TryConvert("32/13/2025", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 12/25/2025
        // False
    }

    /// <summary>Formats a todo item's due date for display in the task list.</summary>
    public static void FormatTodoDueDate()
    {
        var converter = new DateOnlyToStringTypeConverter();

        var success = converter.TryConvert(TodoDueDate, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 12/25/2025
    }

    /// <summary>Parses an appointment start time from a calendar text field.</summary>
    public static void ParseAppointmentStartTime()
    {
        var converter = new StringToTimeOnlyTypeConverter();

        // Success case: valid time
        var success = converter.TryConvert("14:30:00", conversionHint: null, out var startTime);
        Console.WriteLine(success);
        Console.WriteLine(startTime);

        // Failure case: invalid time format
        var failure = converter.TryConvert("25:00:00", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 14:30
        // False
    }

    /// <summary>Formats an appointment start time for display in a calendar event.</summary>
    public static void FormatAppointmentStartTime()
    {
        var converter = new TimeOnlyToStringTypeConverter();

        var success = converter.TryConvert(AppointmentStartTime, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 14:30
    }

    /// <summary>Parses a project duration that a user typed into a task estimate field.</summary>
    public static void ParseProjectDuration()
    {
        var converter = new StringToTimeSpanTypeConverter();

        // Success case: valid duration
        var success = converter.TryConvert("01:30:00", conversionHint: null, out var duration);
        Console.WriteLine(success);
        Console.WriteLine(duration);

        // Failure case: invalid duration format
        var failure = converter.TryConvert("not-a-duration", conversionHint: null, out _);
        Console.WriteLine(failure);

        // Output:
        // True
        // 01:30:00
        // False
    }

    /// <summary>Formats a project duration for display in task management views.</summary>
    public static void FormatProjectDuration()
    {
        var converter = new TimeSpanToStringTypeConverter();

        var success = converter.TryConvert(ProjectDuration, conversionHint: null, out var formatted);
        Console.WriteLine(success);
        Console.WriteLine(formatted);

        // Output:
        // True
        // 01:30:00
    }

    /// <summary>Converts a todo due date and time to and from text with all four DateTime converters and shows their affinity.</summary>
    public static void ConvertTodoDueDateTimeBothWays()
    {
        var toDateTime = new StringToDateTimeTypeConverter();
        _ = toDateTime.TryConvert(DueDateTimeText, conversionHint: null, out var dueDateTime);
        Console.WriteLine($"{dueDateTime} affinity {toDateTime.GetAffinityForObjects()}");

        var fromDateTime = new DateTimeToStringTypeConverter();
        _ = fromDateTime.TryConvert(TodoDueDateTime, conversionHint: null, out var dueDateTimeText);
        Console.WriteLine($"{dueDateTimeText} affinity {fromDateTime.GetAffinityForObjects()}");

        var toOptionalDateTime = new StringToNullableDateTimeTypeConverter();
        _ = toOptionalDateTime.TryConvert(DueDateTimeText, conversionHint: null, out var optionalDueDateTime);
        Console.WriteLine($"{optionalDueDateTime} affinity {toOptionalDateTime.GetAffinityForObjects()}");

        var fromOptionalDateTime = new NullableDateTimeToStringTypeConverter();
        _ = fromOptionalDateTime.TryConvert((DateTime?)TodoDueDateTime, conversionHint: null, out var optionalDueDateTimeText);
        Console.WriteLine($"{optionalDueDateTimeText} affinity {fromOptionalDateTime.GetAffinityForObjects()}");

        // Output:
        // 12/25/2025 10:30:00 affinity 2
        // 12/25/2025 10:30:00 affinity 2
        // 12/25/2025 10:30:00 affinity 2
        // 12/25/2025 10:30:00 affinity 2
    }

    /// <summary>Converts a file modification time to and from text with all four DateTimeOffset converters and shows their affinity.</summary>
    public static void ConvertCloudStorageModificationTimeBothWays()
    {
        var toOffset = new StringToDateTimeOffsetTypeConverter();
        _ = toOffset.TryConvert(ModifiedTimeText, conversionHint: null, out var modifiedTime);
        Console.WriteLine($"{modifiedTime} affinity {toOffset.GetAffinityForObjects()}");

        var fromOffset = new DateTimeOffsetToStringTypeConverter();
        _ = fromOffset.TryConvert(FileLastModifiedTime, conversionHint: null, out var modifiedTimeText);
        Console.WriteLine($"{modifiedTimeText} affinity {fromOffset.GetAffinityForObjects()}");

        var toOptionalOffset = new StringToNullableDateTimeOffsetTypeConverter();
        _ = toOptionalOffset.TryConvert(ModifiedTimeText, conversionHint: null, out var optionalModifiedTime);
        Console.WriteLine($"{optionalModifiedTime} affinity {toOptionalOffset.GetAffinityForObjects()}");

        var fromOptionalOffset = new NullableDateTimeOffsetToStringTypeConverter();
        _ = fromOptionalOffset.TryConvert((DateTimeOffset?)FileLastModifiedTime, conversionHint: null, out var optionalModifiedTimeText);
        Console.WriteLine($"{optionalModifiedTimeText} affinity {fromOptionalOffset.GetAffinityForObjects()}");

        // Output:
        // 09/21/2025 10:30:00 +00:00 affinity 2
        // 09/21/2025 10:30:00 +00:00 affinity 2
        // 09/21/2025 10:30:00 +00:00 affinity 2
        // 09/21/2025 10:30:00 +00:00 affinity 2
    }

    /// <summary>Converts a todo due date to and from text with all four DateOnly converters and shows their affinity.</summary>
    public static void ConvertTodoDueDateBothWays()
    {
        var toDate = new StringToDateOnlyTypeConverter();
        _ = toDate.TryConvert(DueDateText, conversionHint: null, out var dueDate);
        Console.WriteLine($"{dueDate} affinity {toDate.GetAffinityForObjects()}");

        var fromDate = new DateOnlyToStringTypeConverter();
        _ = fromDate.TryConvert(TodoDueDate, conversionHint: null, out var dueDateText);
        Console.WriteLine($"{dueDateText} affinity {fromDate.GetAffinityForObjects()}");

        var toOptionalDate = new StringToNullableDateOnlyTypeConverter();
        _ = toOptionalDate.TryConvert(DueDateText, conversionHint: null, out var optionalDueDate);
        Console.WriteLine($"{optionalDueDate} affinity {toOptionalDate.GetAffinityForObjects()}");

        var fromOptionalDate = new NullableDateOnlyToStringTypeConverter();
        _ = fromOptionalDate.TryConvert((DateOnly?)TodoDueDate, conversionHint: null, out var optionalDueDateText);
        Console.WriteLine($"{optionalDueDateText} affinity {fromOptionalDate.GetAffinityForObjects()}");

        // Output:
        // 12/25/2025 affinity 2
        // 12/25/2025 affinity 2
        // 12/25/2025 affinity 2
        // 12/25/2025 affinity 2
    }

    /// <summary>Converts an appointment start time to and from text with all four TimeOnly converters and shows their affinity.</summary>
    public static void ConvertAppointmentStartTimeBothWays()
    {
        var toTime = new StringToTimeOnlyTypeConverter();
        _ = toTime.TryConvert(StartTimeText, conversionHint: null, out var startTime);
        Console.WriteLine($"{startTime} affinity {toTime.GetAffinityForObjects()}");

        var fromTime = new TimeOnlyToStringTypeConverter();
        _ = fromTime.TryConvert(AppointmentStartTime, conversionHint: null, out var startTimeText);
        Console.WriteLine($"{startTimeText} affinity {fromTime.GetAffinityForObjects()}");

        var toOptionalTime = new StringToNullableTimeOnlyTypeConverter();
        _ = toOptionalTime.TryConvert(StartTimeText, conversionHint: null, out var optionalStartTime);
        Console.WriteLine($"{optionalStartTime} affinity {toOptionalTime.GetAffinityForObjects()}");

        var fromOptionalTime = new NullableTimeOnlyToStringTypeConverter();
        _ = fromOptionalTime.TryConvert((TimeOnly?)AppointmentStartTime, conversionHint: null, out var optionalStartTimeText);
        Console.WriteLine($"{optionalStartTimeText} affinity {fromOptionalTime.GetAffinityForObjects()}");

        // Output:
        // 14:30 affinity 2
        // 14:30 affinity 2
        // 14:30 affinity 2
        // 14:30 affinity 2
    }

    /// <summary>Converts a project duration to and from text with all four TimeSpan converters and shows their affinity.</summary>
    public static void ConvertProjectDurationBothWays()
    {
        var toSpan = new StringToTimeSpanTypeConverter();
        _ = toSpan.TryConvert(DurationText, conversionHint: null, out var duration);
        Console.WriteLine($"{duration} affinity {toSpan.GetAffinityForObjects()}");

        var fromSpan = new TimeSpanToStringTypeConverter();
        _ = fromSpan.TryConvert(ProjectDuration, conversionHint: null, out var durationText);
        Console.WriteLine($"{durationText} affinity {fromSpan.GetAffinityForObjects()}");

        var toOptionalSpan = new StringToNullableTimeSpanTypeConverter();
        _ = toOptionalSpan.TryConvert(DurationText, conversionHint: null, out var optionalDuration);
        Console.WriteLine($"{optionalDuration} affinity {toOptionalSpan.GetAffinityForObjects()}");

        var fromOptionalSpan = new NullableTimeSpanToStringTypeConverter();
        _ = fromOptionalSpan.TryConvert((TimeSpan?)ProjectDuration, conversionHint: null, out var optionalDurationText);
        Console.WriteLine($"{optionalDurationText} affinity {fromOptionalSpan.GetAffinityForObjects()}");

        // Output:
        // 01:30:00 affinity 2
        // 01:30:00 affinity 2
        // 01:30:00 affinity 2
        // 01:30:00 affinity 2
    }
}
