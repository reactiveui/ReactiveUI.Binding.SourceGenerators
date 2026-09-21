// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Controls;

/// <summary>A list of items with one selected. A real UI framework supplies this control.</summary>
/// <typeparam name="T">The type of item in the list.</typeparam>
[System.Diagnostics.DebuggerDisplay("Items = {Items.Count}, SelectedItem = {SelectedItem}")]
public sealed class ItemsListControl<T> : ControlBase
    where T : class
{
    /// <summary>Gets or sets the items the list shows. Assign a new list to change what the control shows.</summary>
    public IReadOnlyList<T> Items
    {
        get;
        set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the selected item, or <see langword="null"/> when nothing is selected.</summary>
    public T? SelectedItem
    {
        get;
        set => SetProperty(ref field, value);
    }
}
