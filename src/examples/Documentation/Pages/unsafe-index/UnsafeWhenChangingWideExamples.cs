// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Documentation.UnsafeIndex;

/// <summary>Demonstrates <c>WhenChangingUnsafe</c> over three to sixteen properties as a group of values; the paths are built while the app runs.</summary>
public static class UnsafeWhenChangingWideExamples
{
    /// <summary>The title of the item an example observes.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives the item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The notes of the item an example observes.</summary>
    private const string SeededNotes = "Bring the insurance certificate.";

    /// <summary>Observes three properties chosen by name before they change; each change delivers all three values.</summary>
    public static void ObserveThreeColumnsBeforeChange()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangingUnsafe(title, notes, notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(new(OriginalTitle, SeededNotes, SeededNotes), recording.Latest);
    }

    /// <summary>Observes four properties chosen by name before they change; each change delivers all four values.</summary>
    public static void ObserveFourColumnsBeforeChange()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangingUnsafe(title, notes, notes, notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(new(OriginalTitle, SeededNotes, SeededNotes, SeededNotes), recording.Latest);
    }

    /// <summary>Observes five properties chosen by name before they change; each change delivers all five values.</summary>
    public static void ObserveFiveColumnsBeforeChange()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangingUnsafe(
            title,
            notes,
            notes,
            notes,
            notes);

        using var recording = changes.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(
            new(
                OriginalTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes six properties chosen by name before they change; each change delivers all six values.</summary>
    public static void ObserveSixColumnsBeforeChange()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangingUnsafe(
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
                OriginalTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes seven properties chosen by name before they change; each change delivers all seven values.</summary>
    public static void ObserveSevenColumnsBeforeChange()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangingUnsafe(
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
                OriginalTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes eight properties chosen by name before they change; each change delivers all eight values.</summary>
    public static void ObserveEightColumnsBeforeChange()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangingUnsafe(
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
                OriginalTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes nine properties chosen by name before they change; each change delivers all nine values.</summary>
    public static void ObserveNineColumnsBeforeChange()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangingUnsafe(
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
                OriginalTitle,
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

    /// <summary>Observes ten properties chosen by name before they change; each change delivers all ten values.</summary>
    public static void ObserveTenColumnsBeforeChange()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangingUnsafe(
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
                OriginalTitle,
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

    /// <summary>Observes eleven properties chosen by name before they change; each change delivers all eleven values.</summary>
    public static void ObserveElevenColumnsBeforeChange()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangingUnsafe(
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
                OriginalTitle,
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

    /// <summary>Observes twelve properties chosen by name before they change; each change delivers all twelve values.</summary>
    public static void ObserveTwelveColumnsBeforeChange()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangingUnsafe(
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
                OriginalTitle,
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

    /// <summary>Observes thirteen properties chosen by name before they change; each change delivers all thirteen values.</summary>
    public static void ObserveThirteenColumnsBeforeChange()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangingUnsafe(
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
                OriginalTitle,
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

    /// <summary>Observes fourteen properties chosen by name before they change; each change delivers all fourteen values.</summary>
    public static void ObserveFourteenColumnsBeforeChange()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangingUnsafe(
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
                OriginalTitle,
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

    /// <summary>Observes fifteen properties chosen by name before they change; each change delivers all fifteen values.</summary>
    public static void ObserveFifteenColumnsBeforeChange()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangingUnsafe(
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
                OriginalTitle,
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

    /// <summary>Observes sixteen properties chosen by name before they change; each change delivers all sixteen values.</summary>
    public static void ObserveSixteenColumnsBeforeChange()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var changes = item.WhenChangingUnsafe(
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
                OriginalTitle,
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
    private static EditableTodoItem CreateItem() => new() { Title = OriginalTitle, Notes = SeededNotes };

    /// <summary>Builds the path of the title, as if a user had picked that column.</summary>
    /// <returns>The path.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Expression<Func<EditableTodoItem, string>> TitleColumn() => RuntimePath<EditableTodoItem, string>.Of(nameof(EditableTodoItem.Title));

    /// <summary>Builds the path of the notes, as if a user had picked that column.</summary>
    /// <returns>The path.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Expression<Func<EditableTodoItem, string>> NotesColumn() => RuntimePath<EditableTodoItem, string>.Of(nameof(EditableTodoItem.Notes));
}
