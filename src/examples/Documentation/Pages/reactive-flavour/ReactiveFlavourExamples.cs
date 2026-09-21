// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Reactive.Concurrency;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.ReactiveFlavour;

/// <summary>
/// Demonstrates the same to-do bindings as the first-binding page against <c>ReactiveUI.Binding.Reactive</c>. That package
/// takes System.Reactive's <see cref="IScheduler"/> where the lean package takes a sequencer, and its types live under
/// the <c>ReactiveUI.Binding.Reactive</c> namespace.
/// </summary>
public static class ReactiveFlavourExamples
{
    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives an item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The number of unfinished items in the seeded list.</summary>
    private const string SeededRemainingText = "3";

    /// <summary>The number of unfinished items once the first seeded item is finished.</summary>
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
    public static void BindRemainingCountToLabel()
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

    /// <summary>Delivers each write on a System.Reactive scheduler you supply, here one that runs its work when the example asks.</summary>
    public static void BindRemainingCountOnReactiveScheduler()
    {
        var viewModel = CreateLoadedViewModel();
        TodoView view = new();
        HistoricalScheduler scheduler = new();

        using (viewModel.BindOneWay(view, static x => x.RemainingCount, static v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture), scheduler))
        {
            SampleCheck.Equal(string.Empty, view.RemainingLabel.Text);

            scheduler.Start();

            SampleCheck.Equal(SeededRemainingText, view.RemainingLabel.Text);
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
