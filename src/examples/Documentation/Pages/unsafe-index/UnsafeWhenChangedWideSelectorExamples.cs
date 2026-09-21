// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.UnsafeIndex;

/// <summary>Demonstrates <c>WhenChangedUnsafe</c> over four to sixteen properties with a selector; the paths are built while the app runs.</summary>
public static class UnsafeWhenChangedWideSelectorExamples
{
    /// <summary>The title of the item an example observes.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives the item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The notes of the item an example observes.</summary>
    private const string SeededNotes = "Bring the insurance certificate.";

    /// <summary>Observes four properties chosen by name and combines them with a selector.</summary>
    public static void ObserveFourColumnsWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangedUnsafe(title, notes, notes, notes, static (c1, c2, c3, c4) => ColumnText.Join(c1, c2, c3, c4));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(ColumnText.Join(RenamedTitle, SeededNotes, SeededNotes, SeededNotes), recording.Latest);
    }

    /// <summary>Observes five properties chosen by name and combines them with a selector.</summary>
    public static void ObserveFiveColumnsWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5) =>
            ColumnText.Join(c1, c2, c3, c4, c5));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            ColumnText.Join(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes six properties chosen by name and combines them with a selector.</summary>
    public static void ObserveSixColumnsWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5, c6) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            ColumnText.Join(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes seven properties chosen by name and combines them with a selector.</summary>
    public static void ObserveSevenColumnsWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5, c6, c7) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            ColumnText.Join(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes eight properties chosen by name and combines them with a selector.</summary>
    public static void ObserveEightColumnsWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5, c6, c7, c8) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7, c8));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            ColumnText.Join(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes nine properties chosen by name and combines them with a selector.</summary>
    public static void ObserveNineColumnsWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7, c8, c9));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            ColumnText.Join(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes ten properties chosen by name and combines them with a selector.</summary>
    public static void ObserveTenColumnsWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7, c8, c9, c10));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            ColumnText.Join(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes eleven properties chosen by name and combines them with a selector.</summary>
    public static void ObserveElevenColumnsWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            ColumnText.Join(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes twelve properties chosen by name and combines them with a selector.</summary>
    public static void ObserveTwelveColumnsWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            ColumnText.Join(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes thirteen properties chosen by name and combines them with a selector.</summary>
    public static void ObserveThirteenColumnsWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            ColumnText.Join(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes fourteen properties chosen by name and combines them with a selector.</summary>
    public static void ObserveFourteenColumnsWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            ColumnText.Join(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes fifteen properties chosen by name and combines them with a selector.</summary>
    public static void ObserveFifteenColumnsWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            ColumnText.Join(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes sixteen properties chosen by name and combines them with a selector.</summary>
    public static void ObserveSixteenColumnsWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15, c16) =>
            ColumnText.Join(c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15, c16));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            ColumnText.Join(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Creates the item an example observes.</summary>
    /// <returns>The item.</returns>
    private static TodoItem CreateItem() => new() { Title = OriginalTitle, Notes = SeededNotes };

    /// <summary>Builds the path of the title, as if a user had picked that column.</summary>
    /// <returns>The path.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Expression<Func<TodoItem, string>> TitleColumn() => RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Title));

    /// <summary>Builds the path of the notes, as if a user had picked that column.</summary>
    /// <returns>The path.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Expression<Func<TodoItem, string>> NotesColumn() => RuntimePath<TodoItem, string>.Of(nameof(TodoItem.Notes));
}
