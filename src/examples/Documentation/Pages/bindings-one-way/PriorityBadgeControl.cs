// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Controls;

namespace ReactiveUI.Binding.Documentation.BindingsOneWay;

/// <summary>A small coloured badge next to a to-do item. A real UI framework supplies this control.</summary>
[System.Diagnostics.DebuggerDisplay("Colour = {Colour}")]
public sealed class PriorityBadgeControl : ControlBase
{
    /// <summary>Gets or sets the colour of the badge, as a hex code.</summary>
    public string Colour
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;
}
