// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.BindingsListsAndSelection;

/// <summary>
/// Demonstrates a master and detail screen: a busy indicator, filter boxes, a filtered list, a selection that
/// drives a detail label, and a button that follows the selection.
/// </summary>
public static class BindingsListsAndSelectionExamples
{
    /// <summary>The first seeded title.</summary>
    private const string RenewTitle = "Renew car registration";

    /// <summary>The second seeded title.</summary>
    private const string DentistTitle = "Book dentist appointment";

    /// <summary>The third seeded title.</summary>
    private const string BirthdayTitle = "Buy birthday present for Sam";

    /// <summary>The fourth seeded title.</summary>
    private const string TaxTitle = "File quarterly tax return";

    /// <summary>The detail text shown while nothing is selected.</summary>
    private const string NoSelectionText = "Nothing selected";

    /// <summary>A title filter that matches the two titles that contain a capital or lower-case b.</summary>
    private const string TitleContainsB = "B";

    /// <summary>A notes filter that matches the notes that contain the word "the".</summary>
    private const string NotesContainThe = "the";

    /// <summary>Shows a progress bar and disables the load button while a load is in flight.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindBusyIndicator()
    {
        var store = InMemoryTodoStore.CreateSeeded();

        var viewModel = new TodoBrowserViewModel(store);

        var view = new TodoBrowserView { ViewModel = viewModel };

        using (viewModel.BindOneWay(view, static x => x.IsLoading, static v => v.LoadingBar.IsVisible))
        using (viewModel.BindOneWay(view, static x => x.IsLoading, static v => v.LoadButton.IsEnabled, static loading => !loading))
        {
            SampleCheck.Equal(false, view.LoadingBar.IsVisible);
            SampleCheck.Equal(true, view.LoadButton.IsEnabled);

            store.Gate.Hold();
            viewModel.LoadCommand.Execute(null);

            SampleCheck.Equal(true, view.LoadingBar.IsVisible);
            SampleCheck.Equal(false, view.LoadButton.IsEnabled);

            store.Gate.ReleaseAll();
            await viewModel.LoadCommand.Completion;

            SampleCheck.Equal(false, view.LoadingBar.IsVisible);
            SampleCheck.Equal(true, view.LoadButton.IsEnabled);
        }
    }

    /// <summary>Binds the title and notes filter boxes to the view model in both directions.</summary>
    public static void BindFilterTextBoxesTwoWay()
    {
        var store = InMemoryTodoStore.CreateSeeded();

        var viewModel = new TodoBrowserViewModel(store) { TitleFilter = TitleContainsB };

        var view = new TodoBrowserView { ViewModel = viewModel };

        using (view.Bind(viewModel, x => x.TitleFilter, v => v.TitleFilterTextBox.Text))
        using (view.Bind(viewModel, x => x.NotesFilter, v => v.NotesFilterTextBox.Text))
        {
            // The view model's starting filter reaches the box.
            SampleCheck.Equal(TitleContainsB, view.TitleFilterTextBox.Text);

            // Typing in a box reaches the view model.
            view.NotesFilterTextBox.Text = NotesContainThe;

            SampleCheck.Equal(NotesContainThe, viewModel.NotesFilter);

            // Clearing the filter in code reaches the box.
            viewModel.TitleFilter = string.Empty;

            SampleCheck.Equal(string.Empty, view.TitleFilterTextBox.Text);
        }
    }

    /// <summary>Binds the filtered items into the list control, so the list narrows as the filters change.</summary>
    public static void BindFilteredItemsToList()
    {
        var store = InMemoryTodoStore.CreateSeeded();

        var viewModel = new TodoBrowserViewModel(store);

        viewModel.LoadCommand.Execute(null);

        var view = new TodoBrowserView { ViewModel = viewModel };

        using (view.OneWayBind(viewModel, x => x.Items, v => v.ItemsList.Items))
        {
            SampleCheck.SequenceEqual([RenewTitle, DentistTitle, BirthdayTitle, TaxTitle], TitlesOf(view.ItemsList.Items));

            viewModel.TitleFilter = TitleContainsB;

            SampleCheck.SequenceEqual([DentistTitle, BirthdayTitle], TitlesOf(view.ItemsList.Items));

            viewModel.NotesFilter = NotesContainThe;

            SampleCheck.SequenceEqual([DentistTitle], TitlesOf(view.ItemsList.Items));
        }
    }

    /// <summary>Binds the list's selected item to the view model in both directions, and shows the selection in a detail label.</summary>
    public static void BindSelectedItemTwoWay()
    {
        const string RenamedTitle = "Book dentist for a check-up";

        var store = InMemoryTodoStore.CreateSeeded();

        var viewModel = new TodoBrowserViewModel(store);

        viewModel.LoadCommand.Execute(null);

        var view = new TodoBrowserView { ViewModel = viewModel };

        using (viewModel.BindOneWay(view.ItemsList, static x => x.Items, static v => v.Items))
        using (viewModel.BindTwoWay(view.ItemsList, static x => x.SelectedItem, static v => v.SelectedItem))
        using (viewModel.BindOneWay(view, static x => x.SelectedItem!.Title, static v => v.DetailLabel.Text, static title => title ?? NoSelectionText))
        {
            SampleCheck.Equal(NoSelectionText, view.DetailLabel.Text);

            // The user clicks the second row.
            var dentist = view.ItemsList.Items[1];
            view.ItemsList.SelectedItem = dentist;

            SampleCheck.Equal(dentist, viewModel.SelectedItem);
            SampleCheck.Equal(DentistTitle, view.DetailLabel.Text);

            // Code selects the first item, and the list follows.
            var renew = viewModel.Items[0];
            viewModel.SelectedItem = renew;

            SampleCheck.Equal(renew, view.ItemsList.SelectedItem);
            SampleCheck.Equal(RenewTitle, view.DetailLabel.Text);

            // The label follows a change to the selected item itself.
            renew.Title = RenamedTitle;

            SampleCheck.Equal(RenamedTitle, view.DetailLabel.Text);

            // Clearing the selection in the list clears the detail.
            view.ItemsList.SelectedItem = null;

            SampleCheck.Equal(NoSelectionText, view.DetailLabel.Text);
        }
    }

    /// <summary>Combines both filter texts and the loaded items into one filtered result with WhenAny.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task FilterItemsWithWhenAny()
    {
        const string LibraryTitle = "Return library books";

        var store = InMemoryTodoStore.CreateSeeded();

        var viewModel = new TodoBrowserViewModel(store);

        viewModel.LoadCommand.Execute(null);

        List<IReadOnlyList<TodoItem>> results = [];

        using (viewModel.WhenAny(
            static x => x.TitleFilter,
            static x => x.NotesFilter,
            static x => x.AllItems,
            static (title, notes, all) => TodoBrowserViewModel.Filter(all.Value!, title.Value!, notes.Value!)).Subscribe(results.Add))
        {
            SampleCheck.SequenceEqual([RenewTitle, DentistTitle, BirthdayTitle, TaxTitle], TitlesOf(results[^1]));

            viewModel.TitleFilter = TitleContainsB;

            SampleCheck.SequenceEqual([DentistTitle, BirthdayTitle], TitlesOf(results[^1]));
            SampleCheck.SequenceEqual(TitlesOf(viewModel.Items), TitlesOf(results[^1]));

            viewModel.NotesFilter = NotesContainThe;

            SampleCheck.SequenceEqual([DentistTitle], TitlesOf(results[^1]));
            SampleCheck.SequenceEqual(TitlesOf(viewModel.Items), TitlesOf(results[^1]));

            viewModel.TitleFilter = string.Empty;

            SampleCheck.SequenceEqual([RenewTitle, DentistTitle, TaxTitle], TitlesOf(results[^1]));
            SampleCheck.SequenceEqual(TitlesOf(viewModel.Items), TitlesOf(results[^1]));

            // A reload replaces AllItems, which is the third input.
            _ = await store.AddAsync(new TodoItem { Title = LibraryTitle, Notes = "Drop them at the front desk." });
            viewModel.LoadCommand.Execute(null);

            SampleCheck.SequenceEqual([RenewTitle, DentistTitle, TaxTitle, LibraryTitle], TitlesOf(results[^1]));
            SampleCheck.SequenceEqual(TitlesOf(viewModel.Items), TitlesOf(results[^1]));
        }
    }

    /// <summary>Enables the complete button only while an unfinished item is selected, using BindCommand.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindCommandFollowsSelection()
    {
        var store = InMemoryTodoStore.CreateSeeded();

        var viewModel = new TodoBrowserViewModel(store);

        viewModel.LoadCommand.Execute(null);

        var view = new TodoBrowserView { ViewModel = viewModel };

        using (view.BindCommand(viewModel, x => x.CompleteCommand, v => v.CompleteButton))
        {
            SampleCheck.Equal(false, view.CompleteButton.IsEnabled);

            var renew = viewModel.Items[0];
            viewModel.SelectedItem = renew;

            SampleCheck.Equal(true, view.CompleteButton.IsEnabled);

            // Pressing the button runs the command, which finishes the item and disables the button again.
            view.CompleteButton.Press();
            await viewModel.CompleteCommand.Completion;

            SampleCheck.Equal(true, renew.IsDone);
            SampleCheck.Equal(false, view.CompleteButton.IsEnabled);

            // Selecting a finished item leaves the button disabled, because the command cannot run on it.
            viewModel.SelectedItem = viewModel.Items[^1];

            SampleCheck.Equal(false, view.CompleteButton.IsEnabled);

            viewModel.SelectedItem = null;

            SampleCheck.Equal(false, view.CompleteButton.IsEnabled);
        }
    }

    /// <summary>Lists the titles of the items, in order.</summary>
    /// <param name="items">The items to read.</param>
    /// <returns>The title of each item.</returns>
    private static List<string> TitlesOf(IReadOnlyList<TodoItem> items)
    {
        List<string> titles = [];
        foreach (var item in items)
        {
            titles.Add(item.Title);
        }

        return titles;
    }
}
