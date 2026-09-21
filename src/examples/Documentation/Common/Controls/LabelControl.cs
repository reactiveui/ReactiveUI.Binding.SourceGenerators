// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Controls;

/// <summary>Read-only text. A real UI framework supplies this control.</summary>
[System.Diagnostics.DebuggerDisplay("Text = {Text}")]
public sealed class LabelControl : ControlBase
{
    /// <summary>Gets or sets the text the label shows.</summary>
    public string Text
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;
}
