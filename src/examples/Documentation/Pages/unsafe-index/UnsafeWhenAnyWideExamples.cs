// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.UnsafeIndex;

/// <summary>Demonstrates <c>WhenAnyUnsafe</c> over three to twelve properties whose paths are built while the app runs.</summary>
public static class UnsafeWhenAnyWideExamples
{
    /// <summary>The title of the item an example observes.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives the item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The notes of the item an example observes.</summary>
    private const string SeededNotes = "Bring the insurance certificate.";

    /// <summary>Observes three properties chosen by name; the selector receives each change, not only its value.</summary>
    public static void ObserveThreeColumnsWithChanges()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenAnyUnsafe(title, notes, notes, static (c1, c2, c3) => ColumnText.Join(c1.Value, c2.Value, c3.Value));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(ColumnText.Join(RenamedTitle, SeededNotes, SeededNotes), recording.Latest);
    }

    /// <summary>Observes four properties chosen by name; the selector receives each change, not only its value.</summary>
    public static void ObserveFourColumnsWithChanges()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenAnyUnsafe(title, notes, notes, notes, static (c1, c2, c3, c4) => ColumnText.Join(c1.Value, c2.Value, c3.Value, c4.Value));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(ColumnText.Join(RenamedTitle, SeededNotes, SeededNotes, SeededNotes), recording.Latest);
    }

    /// <summary>Observes five properties chosen by name; the selector receives each change, not only its value.</summary>
    public static void ObserveFiveColumnsWithChanges()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenAnyUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5) =>
            ColumnText.Join(c1.Value, c2.Value, c3.Value, c4.Value, c5.Value));

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

    /// <summary>Observes six properties chosen by name; the selector receives each change, not only its value.</summary>
    public static void ObserveSixColumnsWithChanges()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenAnyUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5, c6) =>
            ColumnText.Join(c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value));

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

    /// <summary>Observes seven properties chosen by name; the selector receives each change, not only its value.</summary>
    public static void ObserveSevenColumnsWithChanges()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenAnyUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5, c6, c7) =>
            ColumnText.Join(c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value, c7.Value));

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

    /// <summary>Observes eight properties chosen by name; the selector receives each change, not only its value.</summary>
    public static void ObserveEightColumnsWithChanges()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenAnyUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            static (c1, c2, c3, c4, c5, c6, c7, c8) =>
            ColumnText.Join(c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value, c7.Value, c8.Value));

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

    /// <summary>Observes nine properties chosen by name; the selector receives each change, not only its value.</summary>
    public static void ObserveNineColumnsWithChanges()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenAnyUnsafe(
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
            ColumnText.Join(c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value, c7.Value, c8.Value, c9.Value));

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

    /// <summary>Observes ten properties chosen by name; the selector receives each change, not only its value.</summary>
    public static void ObserveTenColumnsWithChanges()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenAnyUnsafe(
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
            ColumnText.Join(c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value, c7.Value, c8.Value, c9.Value, c10.Value));

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

    /// <summary>Observes eleven properties chosen by name; the selector receives each change, not only its value.</summary>
    public static void ObserveElevenColumnsWithChanges()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenAnyUnsafe(
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
            ColumnText.Join(c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value, c7.Value, c8.Value, c9.Value, c10.Value, c11.Value));

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

    /// <summary>Observes twelve properties chosen by name; the selector receives each change, not only its value.</summary>
    public static void ObserveTwelveColumnsWithChanges()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenAnyUnsafe(
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
            ColumnText.Join(c1.Value, c2.Value, c3.Value, c4.Value, c5.Value, c6.Value, c7.Value, c8.Value, c9.Value, c10.Value, c11.Value, c12.Value));

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
