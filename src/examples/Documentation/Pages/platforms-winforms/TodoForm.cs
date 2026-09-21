// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.PlatformsWinForms;

/// <summary>The to-do screen as a Windows Forms form: the filter box, the checkbox for the selected item and the count of items left.</summary>
[System.Diagnostics.DebuggerDisplay("ViewModel = {ViewModel}")]
public sealed class TodoForm : Form, IViewFor<TodoListViewModel>
{
    /// <summary>Gets or sets the view model the form shows.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TodoListViewModel? ViewModel { get; set; }

    /// <summary>Gets the box where the user narrows the list.</summary>
    public TextBox FilterTextBox { get; } = new();

    /// <summary>Gets the checkbox that ticks the selected item as finished.</summary>
    public CheckBox DoneCheckBox { get; } = new() { Text = "Done" };

    /// <summary>Gets the label that shows how many items are left to do.</summary>
    public Label RemainingLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TodoListViewModel?)value;
    }
}
