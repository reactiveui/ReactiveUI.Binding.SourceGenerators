// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.UnsafeIndex;

/// <summary>Demonstrates <c>WhenChangedUnsafe</c> over four to sixteen properties as a group of values; the paths are built while the app runs.</summary>
public static class UnsafeWhenChangedWideExamples
{
    /// <summary>The title of the item an example observes.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives the item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The notes of the item an example observes.</summary>
    private const string SeededNotes = "Bring the insurance certificate.";

    /// <summary>Observes four properties chosen by name; each change delivers all four values.</summary>
    public static void ObserveFourColumns()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangedUnsafe(title, notes, notes, notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(new(RenamedTitle, SeededNotes, SeededNotes, SeededNotes), recording.Latest);
    }

    /// <summary>Observes five properties chosen by name; each change delivers all five values.</summary>
    public static void ObserveFiveColumns()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            new(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes six properties chosen by name; each change delivers all six values.</summary>
    public static void ObserveSixColumns()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            new(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes seven properties chosen by name; each change delivers all seven values.</summary>
    public static void ObserveSevenColumns()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            new(
                RenamedTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes eight properties chosen by name; each change delivers all eight values.</summary>
    public static void ObserveEightColumns()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            new(
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

    /// <summary>Observes nine properties chosen by name; each change delivers all nine values.</summary>
    public static void ObserveNineColumns()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            new(
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

    /// <summary>Observes ten properties chosen by name; each change delivers all ten values.</summary>
    public static void ObserveTenColumns()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangedUnsafe(
            title,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes,
            notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            new(
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

    /// <summary>Observes eleven properties chosen by name; each change delivers all eleven values.</summary>
    public static void ObserveElevenColumns()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangedUnsafe(
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
            notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            new(
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

    /// <summary>Observes twelve properties chosen by name; each change delivers all twelve values.</summary>
    public static void ObserveTwelveColumns()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangedUnsafe(
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
            notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            new(
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

    /// <summary>Observes thirteen properties chosen by name; each change delivers all thirteen values.</summary>
    public static void ObserveThirteenColumns()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangedUnsafe(
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
            notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            new(
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

    /// <summary>Observes fourteen properties chosen by name; each change delivers all fourteen values.</summary>
    public static void ObserveFourteenColumns()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangedUnsafe(
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
            notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            new(
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

    /// <summary>Observes fifteen properties chosen by name; each change delivers all fifteen values.</summary>
    public static void ObserveFifteenColumns()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangedUnsafe(
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
            notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            new(
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

    /// <summary>Observes sixteen properties chosen by name; each change delivers all sixteen values.</summary>
    public static void ObserveSixteenColumns()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangedUnsafe(
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
            notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            new(
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
