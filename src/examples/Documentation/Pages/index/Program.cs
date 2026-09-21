// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

const string OriginalTitle = "Renew car registration";

const string RenamedTitle = "Renew car registration online";

var store = InMemoryTodoStore.CreateSeeded();

TodoListViewModel viewModel = new(store);

// The store answers at once until an example holds its gate, so the load has finished when Execute returns.
viewModel.LoadCommand.Execute(null);

var registration = viewModel.Items[0];

List<string> titles = [];

using (registration.WhenChanged(static x => x.Title).Subscribe(titles.Add))
{
    registration.Title = RenamedTitle;
}

SampleCheck.SequenceEqual([OriginalTitle, RenamedTitle], titles);

TodoView view = new();

using (viewModel.BindOneWay(view, static x => x.RemainingCount, static v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture)))
{
    SampleCheck.Equal("3", view.RemainingLabel.Text);

    viewModel.SelectedItem = registration;
    viewModel.CompleteCommand.Execute(null);
    await viewModel.CompleteCommand.Completion;

    SampleCheck.Equal("2", view.RemainingLabel.Text);
}
