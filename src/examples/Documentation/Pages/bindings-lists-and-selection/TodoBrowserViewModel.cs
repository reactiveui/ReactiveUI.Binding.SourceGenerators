// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.BindingsListsAndSelection;

/// <summary>
/// A master and detail view model over the to-do database. It loads every item, narrows them with a title filter
/// and a notes filter, and tracks the selected item. <see cref="IsLoading"/> is true while a load is in flight.
/// </summary>
[System.Diagnostics.DebuggerDisplay("Items = {Items.Count} of {AllItems.Count}, IsLoading = {IsLoading}")]
public sealed class TodoBrowserViewModel : ObservableObject
{
    /// <summary>The database the view model reads and writes.</summary>
    private readonly ITodoStore _store;

    /// <summary>Initializes a new instance of the <see cref="TodoBrowserViewModel"/> class.</summary>
    /// <param name="store">The database to read and write.</param>
    public TodoBrowserViewModel(ITodoStore store)
    {
        _store = store;
        LoadCommand = new(_ => LoadAsync());
        CompleteCommand = new(_ => CompleteAsync(), _ => SelectedItem is { IsDone: false });
    }

    /// <summary>Gets the command that loads every item from the database.</summary>
    public AsyncDelegateCommand LoadCommand { get; }

    /// <summary>Gets the command that finishes <see cref="SelectedItem"/>; it runs only while an unfinished item is selected.</summary>
    public AsyncDelegateCommand CompleteCommand { get; }

    /// <summary>Gets a value indicating whether a load is in flight.</summary>
    public bool IsLoading
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets every loaded item, before the filters are applied.</summary>
    public IReadOnlyList<TodoItem> AllItems
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
            {
                ApplyFilter();
            }
        }
    } = [];

    /// <summary>Gets the items whose title contains <see cref="TitleFilter"/> and whose notes contain <see cref="NotesFilter"/>.</summary>
    public IReadOnlyList<TodoItem> Items
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the text the item title must contain; blank matches every title.</summary>
    public string TitleFilter
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                ApplyFilter();
            }
        }
    } = string.Empty;

    /// <summary>Gets or sets the text the item notes must contain; blank matches every note.</summary>
    public string NotesFilter
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                ApplyFilter();
            }
        }
    } = string.Empty;

    /// <summary>Gets or sets the selected item, or <see langword="null"/> when nothing is selected.</summary>
    public TodoItem? SelectedItem
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                CompleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Picks the items that match both filters.</summary>
    /// <param name="items">The items to narrow.</param>
    /// <param name="titleFilter">The text the title must contain; blank matches every title.</param>
    /// <param name="notesFilter">The text the notes must contain; blank matches every note.</param>
    /// <returns>The matching items, in their original order.</returns>
    public static IReadOnlyList<TodoItem> Filter(IReadOnlyList<TodoItem> items, string titleFilter, string notesFilter)
    {
        List<TodoItem> matches = [];
        foreach (var item in items)
        {
            if (item.Title.Contains(titleFilter, StringComparison.OrdinalIgnoreCase)
                && item.Notes.Contains(notesFilter, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(item);
            }
        }

        return matches;
    }

    /// <summary>Checks whether a list holds an item.</summary>
    /// <param name="items">The list to search.</param>
    /// <param name="item">The item to look for.</param>
    /// <returns><see langword="true"/> when the list holds the item.</returns>
    private static bool Contains(IReadOnlyList<TodoItem> items, TodoItem item)
    {
        foreach (var candidate in items)
        {
            if (ReferenceEquals(candidate, item))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Reads every item and replaces the list, keeping <see cref="IsLoading"/> true until the read finishes.</summary>
    /// <returns>A task that completes when the list is replaced.</returns>
    private async Task LoadAsync()
    {
        IsLoading = true;

        try
        {
            AllItems = await _store.QueryAsync().ConfigureAwait(false);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Stores the selected item as finished.</summary>
    /// <returns>A task that completes when the item is stored.</returns>
    private async Task CompleteAsync()
    {
        if (SelectedItem is not { } item)
        {
            return;
        }

        var update = item.Clone();
        update.IsDone = true;
        var saved = await _store.UpdateAsync(update).ConfigureAwait(false);
        item.IsDone = saved.IsDone;
        CompleteCommand.RaiseCanExecuteChanged();
    }

    /// <summary>Rebuilds <see cref="Items"/> from the loaded items and both filters, clearing a selection the filters hide.</summary>
    private void ApplyFilter()
    {
        Items = Filter(AllItems, TitleFilter, NotesFilter);

        if (SelectedItem is { } selected && !Contains(Items, selected))
        {
            SelectedItem = null;
        }
    }
}
