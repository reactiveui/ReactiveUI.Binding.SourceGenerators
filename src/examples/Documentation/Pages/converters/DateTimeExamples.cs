// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Demonstrates date and time type converters.</summary>
public static class DateTimeExamples
{
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
}
