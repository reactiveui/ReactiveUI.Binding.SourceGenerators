// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.FallbackRuntime;

/// <summary>
/// Demonstrates <see cref="RuntimeObservationFallback"/>, the observation the <c>Unsafe</c> methods run on. It reads a
/// property path from an expression while the app runs.
/// </summary>
public static class FallbackObservationExamples
{
    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives an item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The notes of the first seeded item.</summary>
    private const string OriginalNotes = "Bring the insurance certificate.";

    /// <summary>The notes an example gives an item.</summary>
    private const string ReplacementNotes = "Bring the insurance certificate and a photo.";

    /// <summary>Observes one property after it changes.</summary>
    public static void ObserveOnePropertyAfterChange()
    {
        var item = new TodoItem { Title = OriginalTitle };
        List<string> titles = [];

        using (RuntimeObservationFallback.WhenChanged(item, x => x.Title).Subscribe(titles.Add))
        {
            item.Title = RenamedTitle;
        }

        SampleCheck.SequenceEqual([OriginalTitle, RenamedTitle], titles);
    }

    /// <summary>Observes two properties after they change; each change delivers both values.</summary>
    public static void ObserveTwoPropertiesAfterChange()
    {
        var item = new TodoItem { Title = OriginalTitle };
        List<PropertyValues<string, bool>> values = [];

        using (RuntimeObservationFallback.WhenChanged(item, x => x.Title, x => x.IsDone).Subscribe(values.Add))
        {
            item.IsDone = true;
        }

        SampleCheck.SequenceEqual([new(OriginalTitle, false), new(OriginalTitle, true)], values);
    }

    /// <summary>Observes three properties after they change; each change delivers all three values.</summary>
    public static void ObserveThreePropertiesAfterChange()
    {
        var item = new TodoItem { Title = OriginalTitle, Notes = OriginalNotes };
        List<PropertyValues<string, string, bool>> values = [];

        using (RuntimeObservationFallback.WhenChanged(item, x => x.Title, x => x.Notes, x => x.IsDone).Subscribe(values.Add))
        {
            item.Notes = ReplacementNotes;
        }

        SampleCheck.SequenceEqual([new(OriginalTitle, OriginalNotes, false), new(OriginalTitle, ReplacementNotes, false)], values);
    }

    /// <summary>Observes the value a property held before it changed.</summary>
    public static void ObserveOnePropertyBeforeChange()
    {
        var item = new EditableTodoItem { Title = OriginalTitle };
        List<string> titles = [];

        using (RuntimeObservationFallback.WhenChanging(item, x => x.Title).Subscribe(titles.Add))
        {
            item.Title = RenamedTitle;
        }

        SampleCheck.SequenceEqual([OriginalTitle], titles);
    }

    /// <summary>Observes two properties before they change.</summary>
    public static void ObserveTwoPropertiesBeforeChange()
    {
        var item = new EditableTodoItem { Title = OriginalTitle, Notes = OriginalNotes };
        List<PropertyValues<string, string>> values = [];

        using (RuntimeObservationFallback.WhenChanging(item, x => x.Title, x => x.Notes).Subscribe(values.Add))
        {
            item.Notes = ReplacementNotes;
        }

        SampleCheck.SequenceEqual([new(OriginalTitle, OriginalNotes)], values);
    }

    /// <summary>Observes three properties before they change.</summary>
    public static void ObserveThreePropertiesBeforeChange()
    {
        var item = new EditableTodoItem { Title = OriginalTitle, Notes = OriginalNotes };
        List<PropertyValues<string, string, bool>> values = [];

        using (RuntimeObservationFallback.WhenChanging(item, x => x.Title, x => x.Notes, x => x.IsDone).Subscribe(values.Add))
        {
            item.IsDone = true;
        }

        SampleCheck.SequenceEqual([new(OriginalTitle, OriginalNotes, false)], values);
    }

    /// <summary>Observes one property with <c>WhenAnyValue</c>.</summary>
    public static void ObserveOneAnyValue()
    {
        var item = new TodoItem { Title = OriginalTitle };
        List<string> titles = [];

        using (RuntimeObservationFallback.WhenAnyValue(item, x => x.Title).Subscribe(titles.Add))
        {
            item.Title = RenamedTitle;
        }

        SampleCheck.SequenceEqual([OriginalTitle, RenamedTitle], titles);
    }

    /// <summary>Observes two properties with <c>WhenAnyValue</c>.</summary>
    public static void ObserveTwoAnyValues()
    {
        var item = new TodoItem { Title = OriginalTitle };
        List<PropertyValues<string, TodoPriority>> values = [];

        using (RuntimeObservationFallback.WhenAnyValue(item, x => x.Title, x => x.Priority).Subscribe(values.Add))
        {
            item.Priority = TodoPriority.High;
        }

        SampleCheck.SequenceEqual([new(OriginalTitle, TodoPriority.Normal), new(OriginalTitle, TodoPriority.High)], values);
    }

    /// <summary>Observes three properties with <c>WhenAnyValue</c>.</summary>
    public static void ObserveThreeAnyValues()
    {
        var item = new TodoItem { Title = OriginalTitle, Notes = OriginalNotes };
        List<PropertyValues<string, string, TodoPriority>> values = [];

        using (RuntimeObservationFallback.WhenAnyValue(item, x => x.Title, x => x.Notes, x => x.Priority).Subscribe(values.Add))
        {
            item.Priority = TodoPriority.High;
        }

        SampleCheck.SequenceEqual([new(OriginalTitle, OriginalNotes, TodoPriority.Normal), new(OriginalTitle, OriginalNotes, TodoPriority.High)], values);
    }
}
