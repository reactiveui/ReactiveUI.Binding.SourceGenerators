// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Controls;

/// <summary>A single-line text input. A real UI framework supplies this control.</summary>
[System.Diagnostics.DebuggerDisplay("Text = {Text}")]
public sealed class TextBoxControl : ControlBase
{
    /// <summary>Gets or sets the text in the box.</summary>
    public string Text
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets the hint shown while the box is empty.</summary>
    public string Placeholder
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;
}
