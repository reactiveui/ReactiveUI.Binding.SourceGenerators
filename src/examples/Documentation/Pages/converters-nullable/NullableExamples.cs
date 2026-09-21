// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.ConvertersNullable;

/// <summary>Demonstrates nullable type converters.</summary>
public static class NullableExamples
{
    /// <summary>Invalid number text for testing parse failures.</summary>
    private const string InvalidNumberText = "not-a-number";

    /// <summary>A GitHub issue number for tracking and reference.</summary>
    private const int IssueNumber = 42;

    /// <summary>A file size in bytes (512 KB for cloud storage).</summary>
    private const long FileSize = 524_288L;

    /// <summary>A TCP port number for network configuration.</summary>
    private const short PortNumber = 8080;

    /// <summary>A scientific constant (pi) value.</summary>
    private const float Pi = 3.14F;

    /// <summary>A mathematical constant (Euler's number).</summary>
    private const double EulersNumber = 2.71828D;

    /// <summary>A customer's transfer amount in banking applications.</summary>
    private const decimal TransferAmount = 250.50M;

    /// <summary>An RGB color channel value (0-255).</summary>
    private const byte ColorChannelValue = 255;

    /// <summary>A customer's overdraft limit for banking.</summary>
    private const decimal OverdraftLimit = 1500.00M;

    /// <summary>A student's score in an educational system.</summary>
    private const decimal StudentScore = 72.5M;

    /// <summary>A todo item's due date.</summary>
    private static readonly DateOnly TodoDueDate = new(2025, 12, 25);

    /// <summary>A cloud storage file's last modified time.</summary>
    private static readonly DateTimeOffset FileLastModifiedTime = new(2025, 9, 21, 10, 30, 0, TimeSpan.Zero);

    /// <summary>A unique session correlation ID.</summary>
    private static readonly Guid SessionCorrelationId = new("550e8400-e29b-41d4-a716-446655440000");

    /// <summary>An appointment start time.</summary>
    private static readonly TimeOnly AppointmentStartTime = new(14, 30, 0);

    /// <summary>A project's estimated duration.</summary>
    private static readonly TimeSpan ProjectDuration = new(1, 30, 0);

    /// <summary>Wraps an issue number in a nullable integer.</summary>
    public static void WrapGitHubIssueNumber()
    {
        var converter = new IntegerToNullableIntegerTypeConverter();
        var success = converter.TryConvert(IssueNumber, conversionHint: null, out var result);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((int?)IssueNumber, result);
    }

    /// <summary>Unwraps an optional issue number to a required integer.</summary>
    public static void UnwrapGitHubIssueNumber()
    {
        var converter = new NullableIntegerToIntegerTypeConverter();
        var success = converter.TryConvert((int?)IssueNumber, conversionHint: null, out var result);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal(IssueNumber, result);
    }

    /// <summary>Wraps a file size in a nullable long.</summary>
    public static void WrapCloudStorageFileSize()
    {
        var converter = new LongToNullableLongTypeConverter();
        var success = converter.TryConvert(FileSize, conversionHint: null, out var result);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((long?)FileSize, result);
    }

    /// <summary>Unwraps an optional file size to a required long.</summary>
    public static void UnwrapCloudStorageFileSize()
    {
        var converter = new NullableLongToLongTypeConverter();
        var success = converter.TryConvert((long?)FileSize, conversionHint: null, out var result);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal(FileSize, result);
    }

    /// <summary>Wraps a port number in a nullable short.</summary>
    public static void WrapNetworkPortNumber()
    {
        var converter = new ShortToNullableShortTypeConverter();
        var success = converter.TryConvert(PortNumber, conversionHint: null, out var result);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((short?)PortNumber, result);
    }

    /// <summary>Unwraps an optional port number to a required short.</summary>
    public static void UnwrapNetworkPortNumber()
    {
        var converter = new NullableShortToShortTypeConverter();
        var success = converter.TryConvert((short?)PortNumber, conversionHint: null, out var result);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal(PortNumber, result);
    }

    /// <summary>Wraps a scientific constant in a nullable single.</summary>
    public static void WrapScientificConstant()
    {
        var converter = new SingleToNullableSingleTypeConverter();
        var success = converter.TryConvert(Pi, conversionHint: null, out var result);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((float?)Pi, result);
    }

    /// <summary>Unwraps an optional scientific value to a required single.</summary>
    public static void UnwrapScientificConstant()
    {
        var converter = new NullableSingleToSingleTypeConverter();
        var success = converter.TryConvert((float?)Pi, conversionHint: null, out var result);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal(Pi, result);
    }

    /// <summary>Wraps Euler's number in a nullable double.</summary>
    public static void WrapEulersNumber()
    {
        var converter = new DoubleToNullableDoubleTypeConverter();
        var success = converter.TryConvert(EulersNumber, conversionHint: null, out var result);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((double?)EulersNumber, result);
    }

    /// <summary>Unwraps an optional mathematical constant to a required double.</summary>
    public static void UnwrapEulersNumber()
    {
        var converter = new NullableDoubleToDoubleTypeConverter();
        var success = converter.TryConvert((double?)EulersNumber, conversionHint: null, out var result);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal(EulersNumber, result);
    }

    /// <summary>Wraps a transfer amount in a nullable decimal.</summary>
    public static void WrapBankTransferAmount()
    {
        var converter = new DecimalToNullableDecimalTypeConverter();
        var success = converter.TryConvert(TransferAmount, conversionHint: null, out var result);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((decimal?)TransferAmount, result);
    }

    /// <summary>Unwraps an optional transfer amount to a required decimal.</summary>
    public static void UnwrapBankTransferAmount()
    {
        var converter = new NullableDecimalToDecimalTypeConverter();
        var success = converter.TryConvert((decimal?)TransferAmount, conversionHint: null, out var result);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal(TransferAmount, result);
    }

    /// <summary>Wraps an RGB color channel value in a nullable byte.</summary>
    public static void WrapRGBColorChannel()
    {
        var converter = new ByteToNullableByteTypeConverter();
        var success = converter.TryConvert(ColorChannelValue, conversionHint: null, out var result);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((byte?)ColorChannelValue, result);
    }

    /// <summary>Unwraps an optional color channel value to a required byte.</summary>
    public static void UnwrapRGBColorChannel()
    {
        var converter = new NullableByteToByteTypeConverter();
        var success = converter.TryConvert((byte?)ColorChannelValue, conversionHint: null, out var result);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal(ColorChannelValue, result);
    }

    /// <summary>Parses an optional GitHub issue number from text input.</summary>
    public static void ParseOptionalGitHubIssueNumber()
    {
        var converter = new StringToNullableIntegerTypeConverter();

        // Success case: valid issue number
        var success = converter.TryConvert("42", conversionHint: null, out var issueNumber);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((int?)IssueNumber, issueNumber);

        // Success case: empty string returns null
        var empty = converter.TryConvert(string.Empty, conversionHint: null, out var noIssue);
        SampleCheck.Equal(true, empty);
        SampleCheck.Equal((int?)null, noIssue);

        // Failure case: invalid text
        var failure = converter.TryConvert(InvalidNumberText, conversionHint: null, out _);
        SampleCheck.Equal(false, failure);
    }

    /// <summary>Parses an optional transfer amount from a user input field.</summary>
    public static void ParseOptionalBankTransferAmount()
    {
        var converter = new StringToNullableDecimalTypeConverter();

        // Success case: valid amount
        var success = converter.TryConvert("250.50", conversionHint: null, out var amount);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((decimal?)TransferAmount, amount);

        // Success case: empty returns null
        var empty = converter.TryConvert(string.Empty, conversionHint: null, out var noAmount);
        SampleCheck.Equal(true, empty);
        SampleCheck.Equal((decimal?)null, noAmount);

        // Failure case: invalid currency
        var failure = converter.TryConvert("not-money", conversionHint: null, out _);
        SampleCheck.Equal(false, failure);
    }

    /// <summary>Formats an optional GitHub issue number for display.</summary>
    public static void FormatOptionalGitHubIssueNumber()
    {
        var converter = new NullableIntegerToStringTypeConverter();
        var success = converter.TryConvert((int?)IssueNumber, conversionHint: null, out var issueText);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal("42", issueText);
    }

    /// <summary>Formats an optional transfer amount for display in a banking receipt.</summary>
    public static void FormatOptionalBankTransferAmount()
    {
        var converter = new NullableDecimalToStringTypeConverter();
        var success = converter.TryConvert((decimal?)TransferAmount, conversionHint: null, out var amountText);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal("250.50", amountText);
    }

    /// <summary>Parses an optional customer's bank balance from user input.</summary>
    public static void ParseOptionalBankBalance()
    {
        var converter = new StringToNullableDecimalTypeConverter();

        // Success case: valid balance amount
        var success = converter.TryConvert("1500.00", conversionHint: null, out var balance);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((decimal?)OverdraftLimit, balance);

        // Success case: empty field means unknown balance
        var empty = converter.TryConvert(string.Empty, conversionHint: null, out var unknownBalance);
        SampleCheck.Equal(true, empty);
        SampleCheck.Equal((decimal?)null, unknownBalance);
    }

    /// <summary>Wraps a student's score in a nullable decimal.</summary>
    public static void WrapStudentScore()
    {
        var converter = new DecimalToNullableDecimalTypeConverter();
        var success = converter.TryConvert(StudentScore, conversionHint: null, out var score);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((decimal?)StudentScore, score);
    }

    /// <summary>Parses an optional student score from a grading system text field.</summary>
    public static void ParseOptionalStudentScore()
    {
        var converter = new StringToNullableDecimalTypeConverter();

        // Success case: valid score
        var success = converter.TryConvert("72.5", conversionHint: null, out var score);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((decimal?)StudentScore, score);

        // Success case: no score recorded
        var empty = converter.TryConvert(string.Empty, conversionHint: null, out var noScore);
        SampleCheck.Equal(true, empty);
        SampleCheck.Equal((decimal?)null, noScore);
    }

    /// <summary>Parses an optional byte value (color channel) from text input.</summary>
    public static void ParseOptionalRGBColorChannel()
    {
        var converter = new StringToNullableByteTypeConverter();

        // Success case: valid color intensity
        var success = converter.TryConvert("255", conversionHint: null, out var channelValue);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((byte?)ColorChannelValue, channelValue);

        // Success case: not set
        var notSet = converter.TryConvert(string.Empty, conversionHint: null, out var noChannel);
        SampleCheck.Equal(true, notSet);
        SampleCheck.Equal((byte?)null, noChannel);

        // Failure case: out of range
        var outOfRange = converter.TryConvert("256", conversionHint: null, out _);
        SampleCheck.Equal(false, outOfRange);
    }

    /// <summary>Formats an optional byte value for display in a color picker.</summary>
    public static void FormatOptionalRGBColorChannel()
    {
        var converter = new NullableByteToStringTypeConverter();

        var success = converter.TryConvert((byte?)ColorChannelValue, conversionHint: null, out var channelText);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal("255", channelText);
    }

    /// <summary>Parses an optional short value (port) from text input.</summary>
    public static void ParseOptionalNetworkPortNumber()
    {
        var converter = new StringToNullableShortTypeConverter();

        // Success case: valid port
        var success = converter.TryConvert("8080", conversionHint: null, out var portValue);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((short?)PortNumber, portValue);

        // Success case: empty means default
        var empty = converter.TryConvert(string.Empty, conversionHint: null, out var defaultPort);
        SampleCheck.Equal(true, empty);
        SampleCheck.Equal((short?)null, defaultPort);

        // Failure case: out of range
        var outOfRange = converter.TryConvert("99999", conversionHint: null, out _);
        SampleCheck.Equal(false, outOfRange);
    }

    /// <summary>Formats an optional short value for display in network settings.</summary>
    public static void FormatOptionalNetworkPortNumber()
    {
        var converter = new NullableShortToStringTypeConverter();

        var success = converter.TryConvert((short?)PortNumber, conversionHint: null, out var portText);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal("8080", portText);
    }

    /// <summary>Parses an optional long value (file size) from text input.</summary>
    public static void ParseOptionalCloudStorageFileSize()
    {
        var converter = new StringToNullableLongTypeConverter();

        // Success case: valid file size
        var success = converter.TryConvert("524288", conversionHint: null, out var fileSize);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((long?)FileSize, fileSize);

        // Success case: empty means unknown size
        var empty = converter.TryConvert(string.Empty, conversionHint: null, out var unknownSize);
        SampleCheck.Equal(true, empty);
        SampleCheck.Equal((long?)null, unknownSize);

        // Failure case: overflow
        var overflow = converter.TryConvert("999999999999999999999999", conversionHint: null, out _);
        SampleCheck.Equal(false, overflow);
    }

    /// <summary>Formats an optional long value for display in storage usage views.</summary>
    public static void FormatOptionalCloudStorageFileSize()
    {
        var converter = new NullableLongToStringTypeConverter();

        var success = converter.TryConvert((long?)FileSize, conversionHint: null, out var sizeText);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal("524288", sizeText);
    }

    /// <summary>Parses an optional float value (pi) from text input.</summary>
    public static void ParseOptionalScientificConstant()
    {
        var converter = new StringToNullableSingleTypeConverter();

        // Success case: valid constant
        var success = converter.TryConvert("3.14", conversionHint: null, out var piValue);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((float?)Pi, piValue);

        // Success case: not specified
        var notSpecified = converter.TryConvert(string.Empty, conversionHint: null, out var noPi);
        SampleCheck.Equal(true, notSpecified);
        SampleCheck.Equal((float?)null, noPi);

        // Failure case: invalid input
        var failure = converter.TryConvert(InvalidNumberText, conversionHint: null, out _);
        SampleCheck.Equal(false, failure);
    }

    /// <summary>Formats an optional float value for display in calculator results.</summary>
    public static void FormatOptionalScientificConstant()
    {
        var converter = new NullableSingleToStringTypeConverter();

        var success = converter.TryConvert((float?)Pi, conversionHint: null, out var piText);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal("3.14", piText);
    }

    /// <summary>Parses an optional double value (Euler's number) from text input.</summary>
    public static void ParseOptionalEulersNumber()
    {
        var converter = new StringToNullableDoubleTypeConverter();

        // Success case: valid mathematical constant
        var success = converter.TryConvert("2.71828", conversionHint: null, out var eValue);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((double?)EulersNumber, eValue);

        // Success case: empty means unknown
        var empty = converter.TryConvert(string.Empty, conversionHint: null, out var noE);
        SampleCheck.Equal(true, empty);
        SampleCheck.Equal((double?)null, noE);

        // Failure case: invalid text
        var failure = converter.TryConvert(InvalidNumberText, conversionHint: null, out _);
        SampleCheck.Equal(false, failure);
    }

    /// <summary>Formats an optional double value for display in scientific calculations.</summary>
    public static void FormatOptionalEulersNumber()
    {
        var converter = new NullableDoubleToStringTypeConverter();

        var success = converter.TryConvert((double?)EulersNumber, conversionHint: null, out var eText);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal("2.71828", eText);
    }

    /// <summary>Parses an optional boolean from text input for a feature toggle.</summary>
    public static void ParseOptionalFeatureToggleBoolean()
    {
        var converter = new StringToNullableBooleanTypeConverter();

        // Success case: feature enabled
        var success = converter.TryConvert("True", conversionHint: null, out var isEnabled);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((bool?)true, isEnabled);

        // Success case: undecided/not set
        var notSet = converter.TryConvert(string.Empty, conversionHint: null, out var undecided);
        SampleCheck.Equal(true, notSet);
        SampleCheck.Equal((bool?)null, undecided);

        // Failure case: invalid toggle
        var failure = converter.TryConvert("maybe", conversionHint: null, out _);
        SampleCheck.Equal(false, failure);
    }

    /// <summary>Formats an optional boolean for display in settings UI.</summary>
    public static void FormatOptionalFeatureToggleBoolean()
    {
        var converter = new NullableBooleanToStringTypeConverter();

        var success = converter.TryConvert((bool?)true, conversionHint: null, out var toggleText);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal("True", toggleText);
    }

    /// <summary>Parses an optional DateOnly (todo due date) from text input.</summary>
    public static void ParseOptionalTodoDueDate()
    {
        var converter = new StringToNullableDateOnlyTypeConverter();

        // Success case: valid due date
        var success = converter.TryConvert("2025-12-25", conversionHint: null, out var dueDate);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((DateOnly?)TodoDueDate, dueDate);

        // Success case: not set yet
        var notSet = converter.TryConvert(string.Empty, conversionHint: null, out var noDueDate);
        SampleCheck.Equal(true, notSet);
        SampleCheck.Equal((DateOnly?)null, noDueDate);

        // Failure case: invalid date
        var failure = converter.TryConvert("32/13/2025", conversionHint: null, out _);
        SampleCheck.Equal(false, failure);
    }

    /// <summary>Formats an optional DateOnly for display in task management views.</summary>
    public static void FormatOptionalTodoDueDate()
    {
        var converter = new NullableDateOnlyToStringTypeConverter();

        var success = converter.TryConvert((DateOnly?)TodoDueDate, conversionHint: null, out _);
        SampleCheck.Equal(true, success);
    }

    /// <summary>Parses an optional DateTimeOffset (file modification time) from text input.</summary>
    public static void ParseOptionalCloudStorageModificationTime()
    {
        var converter = new StringToNullableDateTimeOffsetTypeConverter();

        // Success case: valid timestamp with timezone
        var success = converter.TryConvert("2025-09-21T10:30:00+00:00", conversionHint: null, out var modifiedTime);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((DateTimeOffset?)FileLastModifiedTime, modifiedTime);

        // Success case: not available
        var notAvailable = converter.TryConvert(string.Empty, conversionHint: null, out var noModifiedTime);
        SampleCheck.Equal(true, notAvailable);
        SampleCheck.Equal((DateTimeOffset?)null, noModifiedTime);

        // Failure case: invalid format
        var failure = converter.TryConvert("invalid-date", conversionHint: null, out _);
        SampleCheck.Equal(false, failure);
    }

    /// <summary>Formats an optional DateTimeOffset for display in file properties.</summary>
    public static void FormatOptionalCloudStorageModificationTime()
    {
        var converter = new NullableDateTimeOffsetToStringTypeConverter();

        var success = converter.TryConvert((DateTimeOffset?)FileLastModifiedTime, conversionHint: null, out _);
        SampleCheck.Equal(true, success);
    }

    /// <summary>Parses an optional DateTime from text input.</summary>
    public static void ParseOptionalDateTime()
    {
        var converter = new StringToNullableDateTimeTypeConverter();

        // Success case: valid due date-time
        var success = converter.TryConvert("2025-12-25T10:30:00", conversionHint: null, out var dueDateTime);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((DateTime?)new DateTime(2025, 12, 25, 10, 30, 0, DateTimeKind.Unspecified), dueDateTime);

        // Failure case: invalid format
        var failure = converter.TryConvert("not-a-date", conversionHint: null, out _);
        SampleCheck.Equal(false, failure);
    }

    /// <summary>Formats an optional DateTime for display.</summary>
    public static void FormatOptionalDateTime()
    {
        var testDateTime = new DateTime(2025, 12, 25, 10, 30, 0, DateTimeKind.Unspecified);
        var converter = new NullableDateTimeToStringTypeConverter();

        var success = converter.TryConvert((DateTime?)testDateTime, conversionHint: null, out _);
        SampleCheck.Equal(true, success);
    }

    /// <summary>Parses an optional TimeOnly from text input.</summary>
    public static void ParseOptionalAppointmentTime()
    {
        var converter = new StringToNullableTimeOnlyTypeConverter();

        // Success case: valid time
        var success = converter.TryConvert("14:30:00", conversionHint: null, out var appointmentTime);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((TimeOnly?)AppointmentStartTime, appointmentTime);

        // Success case: not scheduled yet
        var notScheduled = converter.TryConvert(string.Empty, conversionHint: null, out var noAppointment);
        SampleCheck.Equal(true, notScheduled);
        SampleCheck.Equal((TimeOnly?)null, noAppointment);

        // Failure case: invalid time
        var failure = converter.TryConvert("25:00:00", conversionHint: null, out _);
        SampleCheck.Equal(false, failure);
    }

    /// <summary>Formats an optional TimeOnly for display in calendar events.</summary>
    public static void FormatOptionalAppointmentTime()
    {
        var converter = new NullableTimeOnlyToStringTypeConverter();

        var success = converter.TryConvert((TimeOnly?)AppointmentStartTime, conversionHint: null, out _);
        SampleCheck.Equal(true, success);
    }

    /// <summary>Parses an optional TimeSpan (project duration) from text input.</summary>
    public static void ParseOptionalProjectDuration()
    {
        var converter = new StringToNullableTimeSpanTypeConverter();

        // Success case: valid duration
        var success = converter.TryConvert("01:30:00", conversionHint: null, out var duration);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((TimeSpan?)ProjectDuration, duration);

        // Success case: not estimated yet
        var notEstimated = converter.TryConvert(string.Empty, conversionHint: null, out var noDuration);
        SampleCheck.Equal(true, notEstimated);
        SampleCheck.Equal((TimeSpan?)null, noDuration);

        // Failure case: invalid format
        var failure = converter.TryConvert("invalid-duration", conversionHint: null, out _);
        SampleCheck.Equal(false, failure);
    }

    /// <summary>Formats an optional TimeSpan for display in project management views.</summary>
    public static void FormatOptionalProjectDuration()
    {
        var converter = new NullableTimeSpanToStringTypeConverter();

        var success = converter.TryConvert((TimeSpan?)ProjectDuration, conversionHint: null, out _);
        SampleCheck.Equal(true, success);
    }

    /// <summary>Parses an optional Guid (session ID) from text input.</summary>
    public static void ParseOptionalSessionCorrelationId()
    {
        var converter = new StringToNullableGuidTypeConverter();

        // Success case: valid GUID
        var success = converter.TryConvert("550e8400-e29b-41d4-a716-446655440000", conversionHint: null, out var correlationId);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal((Guid?)SessionCorrelationId, correlationId);

        // Success case: not set
        var notSet = converter.TryConvert(string.Empty, conversionHint: null, out var noCorrelationId);
        SampleCheck.Equal(true, notSet);
        SampleCheck.Equal((Guid?)null, noCorrelationId);

        // Failure case: invalid GUID format
        var failure = converter.TryConvert("not-a-guid", conversionHint: null, out _);
        SampleCheck.Equal(false, failure);
    }

    /// <summary>Formats an optional Guid for display in logging and trace views.</summary>
    public static void FormatOptionalSessionCorrelationId()
    {
        var converter = new NullableGuidToStringTypeConverter();

        var success = converter.TryConvert((Guid?)SessionCorrelationId, conversionHint: null, out var correlationIdText);
        SampleCheck.Equal(true, success);
        SampleCheck.Equal("550e8400-e29b-41d4-a716-446655440000", correlationIdText);
    }
}
