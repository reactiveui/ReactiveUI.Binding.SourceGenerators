// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Aot;

/// <summary>
/// Demonstrates bindings in an app published with Native AOT. The generator writes each binding at build time, so the
/// published program needs no reflection, expression trees or run-time code generation.
/// </summary>
public static class AotExamples
{
    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives an item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The text of a filter.</summary>
    private const string FilterQuery = "car";

    /// <summary>The text that replaces <see cref="FilterQuery"/>.</summary>
    private const string SecondFilterQuery = "tax";

    /// <summary>The count of unfinished items in the seeded list, as text.</summary>
    private const string SeededRemainingText = "3";

    /// <summary>The count of unfinished items once the first seeded item is finished, as text.</summary>
    private const string RemainingAfterCompleteText = "2";

    /// <summary>Observes the title of an item through <c>WhenChanged</c>.</summary>
    public static void ObserveTitleChange()
    {
        var viewModel = CreateLoadedViewModel();
        var registration = viewModel.Items[0];
        List<string> titles = [];

        using (registration.WhenChanged(static x => x.Title).Subscribe(titles.Add))
        {
            registration.Title = RenamedTitle;
        }

        SampleCheck.SequenceEqual([OriginalTitle, RenamedTitle], titles);
    }

    /// <summary>Binds the count of unfinished items to a label as text.</summary>
    public static void BindRemainingCountOneWay()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();

        using (viewModel.BindOneWay(view, static x => x.RemainingCount, static v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture)))
        {
            SampleCheck.Equal(SeededRemainingText, view.RemainingLabel.Text);

            viewModel.SelectedItem = viewModel.Items[0];
            viewModel.CompleteCommand.Execute(null);

            SampleCheck.Equal(RemainingAfterCompleteText, view.RemainingLabel.Text);
        }
    }

    /// <summary>Carries the filter text both ways between the view model and the filter box.</summary>
    public static void BindFilterTwoWay()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new() { ViewModel = viewModel };

        using (view.Bind(viewModel, x => x.FilterText, v => v.FilterTextBox.Text))
        {
            view.FilterTextBox.Text = FilterQuery;

            SampleCheck.Equal(FilterQuery, viewModel.FilterText);

            viewModel.FilterText = SecondFilterQuery;

            SampleCheck.Equal(SecondFilterQuery, view.FilterTextBox.Text);
        }
    }

    /// <summary>Creates a view model that has loaded the seeded items.</summary>
    /// <returns>The loaded view model.</returns>
    private static TodoListViewModel CreateLoadedViewModel()
    {
        var viewModel = new TodoListViewModel(InMemoryTodoStore.CreateSeeded());

        viewModel.LoadCommand.Execute(null);
        return viewModel;
    }
}
