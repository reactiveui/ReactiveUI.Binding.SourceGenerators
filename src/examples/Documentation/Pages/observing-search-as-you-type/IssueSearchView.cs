// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Controls;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.ObservingSearchAsYouType;

/// <summary>The issue search screen: a search box, a search button, a busy bar and the list of matches.</summary>
[System.Diagnostics.DebuggerDisplay("ViewModel = {ViewModel}")]
public sealed class IssueSearchView : ObservableObject, IViewFor<IssueSearchViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public IssueSearchViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the box where the user types the search term.</summary>
    public TextBoxControl SearchBox { get; } = new() { Placeholder = "Search issues" };

    /// <summary>Gets the button that runs the search. It is disabled while a search is waiting.</summary>
    public ButtonControl SearchButton { get; } = new() { Content = "Search" };

    /// <summary>Gets the bar that shows a search is waiting.</summary>
    public ProgressBarControl BusyBar { get; } = new();

    /// <summary>Gets the list of matching issues.</summary>
    public ItemsListControl<Issue> ResultList { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IssueSearchViewModel?)value;
    }
}
