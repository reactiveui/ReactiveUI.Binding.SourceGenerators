// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Controls;

/// <summary>A box the user ticks or clears. A real UI framework supplies this control.</summary>
[System.Diagnostics.DebuggerDisplay("IsChecked = {IsChecked}")]
public sealed class CheckBoxControl : ControlBase
{
    /// <summary>Gets or sets a value indicating whether the box is ticked.</summary>
    public bool IsChecked
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the caption next to the box.</summary>
    public string Content
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;
}
