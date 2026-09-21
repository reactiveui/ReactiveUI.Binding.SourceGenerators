// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.BindingsListsAndSelection;

/// <summary>
/// The master and detail to-do screen: a load button with a progress bar, two filter boxes, the list of items and
/// a detail area for the selected item. A real UI framework builds these controls from markup; here the view
/// creates them in code.
/// </summary>
[System.Diagnostics.DebuggerDisplay("ViewModel = {ViewModel}")]
public sealed class TodoBrowserView : ObservableObject, IViewFor<TodoBrowserViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public TodoBrowserViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the bar that shows while a load is in flight.</summary>
    public ProgressBarControl LoadingBar { get; } = new();

    /// <summary>Gets the button that loads the items.</summary>
    public ButtonControl LoadButton { get; } = new() { Content = "Load" };

    /// <summary>Gets the box where the user narrows the list by title.</summary>
    public TextBoxControl TitleFilterTextBox { get; } = new() { Placeholder = "Title contains" };

    /// <summary>Gets the box where the user narrows the list by notes.</summary>
    public TextBoxControl NotesFilterTextBox { get; } = new() { Placeholder = "Notes contain" };

    /// <summary>Gets the list of items.</summary>
    public ItemsListControl<TodoItem> ItemsList { get; } = new();

    /// <summary>Gets the label that describes the selected item.</summary>
    public LabelControl DetailLabel { get; } = new();

    /// <summary>Gets the button that finishes the selected item.</summary>
    public ButtonControl CompleteButton { get; } = new() { Content = "Complete" };

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TodoBrowserViewModel?)value;
    }
}
