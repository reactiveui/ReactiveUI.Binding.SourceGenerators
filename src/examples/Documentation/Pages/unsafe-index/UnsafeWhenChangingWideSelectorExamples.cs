// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Documentation.UnsafeIndex;

/// <summary>Demonstrates <c>WhenChangingUnsafe</c> over three to sixteen properties with a selector; the paths are built while the app runs.</summary>
public static class UnsafeWhenChangingWideSelectorExamples
{
    /// <summary>The title of the item an example observes.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives the item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The notes of the item an example observes.</summary>
    private const string SeededNotes = "Bring the insurance certificate.";

    /// <summary>Observes three properties chosen by name before they change and combines them with a selector.</summary>
    public static void ObserveThreeColumnsBeforeChangeWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangingUnsafe(title, notes, notes, static (c1, c2, c3) => ColumnText.Join(c1, c2, c3));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(ColumnText.Join(OriginalTitle, SeededNotes, SeededNotes), recording.Latest);
    }

    /// <summary>Observes four properties chosen by name before they change and combines them with a selector.</summary>
    public static void ObserveFourColumnsBeforeChangeWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangingUnsafe(title, notes, notes, notes, static (c1, c2, c3, c4) => ColumnText.Join(c1, c2, c3, c4));

        using var recording = lines.Record();

        item.Title = RenamedTitle;

        SampleCheck.Equal(ColumnText.Join(OriginalTitle, SeededNotes, SeededNotes, SeededNotes), recording.Latest);
    }

    /// <summary>Observes five properties chosen by name before they change and combines them with a selector.</summary>
    public static void ObserveFiveColumnsBeforeChangeWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangingUnsafe(
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
                OriginalTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes six properties chosen by name before they change and combines them with a selector.</summary>
    public static void ObserveSixColumnsBeforeChangeWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangingUnsafe(
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
                OriginalTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes seven properties chosen by name before they change and combines them with a selector.</summary>
    public static void ObserveSevenColumnsBeforeChangeWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangingUnsafe(
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
                OriginalTitle,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes,
                SeededNotes),
            recording.Latest);
    }

    /// <summary>Observes eight properties chosen by name before they change and combines them with a selector.</summary>
    public static void ObserveEightColumnsBeforeChangeWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangingUnsafe(
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

    /// <summary>Observes nine properties chosen by name before they change and combines them with a selector.</summary>
    public static void ObserveNineColumnsBeforeChangeWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangingUnsafe(
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

    /// <summary>Observes ten properties chosen by name before they change and combines them with a selector.</summary>
    public static void ObserveTenColumnsBeforeChangeWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangingUnsafe(
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

    /// <summary>Observes eleven properties chosen by name before they change and combines them with a selector.</summary>
    public static void ObserveElevenColumnsBeforeChangeWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangingUnsafe(
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

    /// <summary>Observes twelve properties chosen by name before they change and combines them with a selector.</summary>
    public static void ObserveTwelveColumnsBeforeChangeWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangingUnsafe(
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

    /// <summary>Observes thirteen properties chosen by name before they change and combines them with a selector.</summary>
    public static void ObserveThirteenColumnsBeforeChangeWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangingUnsafe(
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

    /// <summary>Observes fourteen properties chosen by name before they change and combines them with a selector.</summary>
    public static void ObserveFourteenColumnsBeforeChangeWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangingUnsafe(
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

    /// <summary>Observes fifteen properties chosen by name before they change and combines them with a selector.</summary>
    public static void ObserveFifteenColumnsBeforeChangeWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangingUnsafe(
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

    /// <summary>Observes sixteen properties chosen by name before they change and combines them with a selector.</summary>
    public static void ObserveSixteenColumnsBeforeChangeWithSelector()
    {
        var item = CreateItem();
        var title = TitleColumn();
        var notes = NotesColumn();
        var lines = item.WhenChangingUnsafe(
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
