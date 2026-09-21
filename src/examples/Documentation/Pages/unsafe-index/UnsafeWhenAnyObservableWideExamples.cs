// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.UnsafeIndex;

/// <summary>Demonstrates <c>WhenAnyObservableUnsafe</c> over three to twelve properties whose paths are built while the app runs.</summary>
public static class UnsafeWhenAnyObservableWideExamples
{
    /// <summary>The announcement an example emits.</summary>
    private const string SyncComplete = "Sync complete";

    /// <summary>The urgent announcement an example emits.</summary>
    private const string DiskFull = "Disk full";

    /// <summary>Merges the announcement streams of three properties chosen by name.</summary>
    public static void MergeThreeAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var messages = announcer.WhenAnyObservableUnsafe(latest, urgent, urgent);

        using var recording = messages.Record();

        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(SyncComplete, recording.Latest);
    }

    /// <summary>Combines the latest announcement of three properties chosen by name with a selector.</summary>
    public static void CombineThreeAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var lines = announcer.WhenAnyObservableUnsafe(latest, urgent, urgent, static (c1, c2, c3) => ColumnText.Join(c1, c2, c3));

        using var recording = lines.Record();

        announcer.Urgent = Signal.Return(DiskFull);
        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(ColumnText.Join(SyncComplete, DiskFull, DiskFull), recording.Latest);
    }

    /// <summary>Merges the announcement streams of four properties chosen by name.</summary>
    public static void MergeFourAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var messages = announcer.WhenAnyObservableUnsafe(latest, urgent, urgent, urgent);

        using var recording = messages.Record();

        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(SyncComplete, recording.Latest);
    }

    /// <summary>Combines the latest announcement of four properties chosen by name with a selector.</summary>
    public static void CombineFourAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var lines = announcer.WhenAnyObservableUnsafe(latest, urgent, urgent, urgent, static (c1, c2, c3, c4) => ColumnText.Join(c1, c2, c3, c4));

        using var recording = lines.Record();

        announcer.Urgent = Signal.Return(DiskFull);
        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(ColumnText.Join(SyncComplete, DiskFull, DiskFull, DiskFull), recording.Latest);
    }

    /// <summary>Merges the announcement streams of five properties chosen by name.</summary>
    public static void MergeFiveAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var messages = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent);

        using var recording = messages.Record();

        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(SyncComplete, recording.Latest);
    }

    /// <summary>Combines the latest announcement of five properties chosen by name with a selector.</summary>
    public static void CombineFiveAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var lines = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            static (c1, c2, c3, c4, c5) =>
            ColumnText.Join(c1, c2, c3, c4, c5));

        using var recording = lines.Record();

        announcer.Urgent = Signal.Return(DiskFull);
        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(
            ColumnText.Join(
                SyncComplete,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull),
            recording.Latest);
    }

    /// <summary>Merges the announcement streams of six properties chosen by name.</summary>
    public static void MergeSixAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var messages = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent);

        using var recording = messages.Record();

        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(SyncComplete, recording.Latest);
    }

    /// <summary>Combines the latest announcement of six properties chosen by name with a selector.</summary>
    public static void CombineSixAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var lines = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            static (c1, c2, c3, c4, c5, c6) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6));

        using var recording = lines.Record();

        announcer.Urgent = Signal.Return(DiskFull);
        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(
            ColumnText.Join(
                SyncComplete,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull),
            recording.Latest);
    }

    /// <summary>Merges the announcement streams of seven properties chosen by name.</summary>
    public static void MergeSevenAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var messages = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent);

        using var recording = messages.Record();

        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(SyncComplete, recording.Latest);
    }

    /// <summary>Combines the latest announcement of seven properties chosen by name with a selector.</summary>
    public static void CombineSevenAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var lines = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            static (c1, c2, c3, c4, c5, c6, c7) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7));

        using var recording = lines.Record();

        announcer.Urgent = Signal.Return(DiskFull);
        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(
            ColumnText.Join(
                SyncComplete,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull),
            recording.Latest);
    }

    /// <summary>Merges the announcement streams of eight properties chosen by name.</summary>
    public static void MergeEightAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var messages = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent);

        using var recording = messages.Record();

        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(SyncComplete, recording.Latest);
    }

    /// <summary>Combines the latest announcement of eight properties chosen by name with a selector.</summary>
    public static void CombineEightAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var lines = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            static (c1, c2, c3, c4, c5, c6, c7, c8) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7, c8));

        using var recording = lines.Record();

        announcer.Urgent = Signal.Return(DiskFull);
        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(
            ColumnText.Join(
                SyncComplete,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull),
            recording.Latest);
    }

    /// <summary>Merges the announcement streams of nine properties chosen by name.</summary>
    public static void MergeNineAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var messages = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent);

        using var recording = messages.Record();

        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(SyncComplete, recording.Latest);
    }

    /// <summary>Combines the latest announcement of nine properties chosen by name with a selector.</summary>
    public static void CombineNineAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var lines = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7, c8, c9));

        using var recording = lines.Record();

        announcer.Urgent = Signal.Return(DiskFull);
        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(
            ColumnText.Join(
                SyncComplete,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull),
            recording.Latest);
    }

    /// <summary>Merges the announcement streams of ten properties chosen by name.</summary>
    public static void MergeTenAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var messages = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent);

        using var recording = messages.Record();

        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(SyncComplete, recording.Latest);
    }

    /// <summary>Combines the latest announcement of ten properties chosen by name with a selector.</summary>
    public static void CombineTenAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var lines = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7, c8, c9, c10));

        using var recording = lines.Record();

        announcer.Urgent = Signal.Return(DiskFull);
        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(
            ColumnText.Join(
                SyncComplete,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull),
            recording.Latest);
    }

    /// <summary>Merges the announcement streams of eleven properties chosen by name.</summary>
    public static void MergeElevenAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var messages = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent);

        using var recording = messages.Record();

        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(SyncComplete, recording.Latest);
    }

    /// <summary>Combines the latest announcement of eleven properties chosen by name with a selector.</summary>
    public static void CombineElevenAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var lines = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11));

        using var recording = lines.Record();

        announcer.Urgent = Signal.Return(DiskFull);
        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(
            ColumnText.Join(
                SyncComplete,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull),
            recording.Latest);
    }

    /// <summary>Merges the announcement streams of twelve properties chosen by name.</summary>
    public static void MergeTwelveAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var messages = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent);

        using var recording = messages.Record();

        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(SyncComplete, recording.Latest);
    }

    /// <summary>Combines the latest announcement of twelve properties chosen by name with a selector.</summary>
    public static void CombineTwelveAnnouncementStreams()
    {
        TodoAnnouncer announcer = new();
        var latest = LatestColumn();
        var urgent = UrgentColumn();
        var lines = announcer.WhenAnyObservableUnsafe(
            latest,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            urgent,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12));

        using var recording = lines.Record();

        announcer.Urgent = Signal.Return(DiskFull);
        announcer.Latest = Signal.Return(SyncComplete);

        SampleCheck.Equal(
            ColumnText.Join(
                SyncComplete,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull,
                DiskFull),
            recording.Latest);
    }

    /// <summary>Builds the path of the ordinary announcements, as if a user had picked that stream.</summary>
    /// <returns>The path.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Expression<Func<TodoAnnouncer, IObservable<string>?>> LatestColumn() => RuntimePath<TodoAnnouncer, IObservable<string>?>.Of(nameof(TodoAnnouncer.Latest));

    /// <summary>Builds the path of the urgent announcements, as if a user had picked that stream.</summary>
    /// <returns>The path.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Expression<Func<TodoAnnouncer, IObservable<string>?>> UrgentColumn() => RuntimePath<TodoAnnouncer, IObservable<string>?>.Of(nameof(TodoAnnouncer.Urgent));
}
