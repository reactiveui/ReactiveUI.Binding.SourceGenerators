// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Controls;

/// <summary>A bar that fills from empty to <see cref="Maximum"/>. A real UI framework supplies this control.</summary>
[System.Diagnostics.DebuggerDisplay("Value = {Value} of {Maximum}")]
public sealed class ProgressBarControl : ControlBase
{
    /// <summary>Gets or sets how far the bar is filled.</summary>
    public double Value
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the value at which the bar is full.</summary>
    public double Maximum
    {
        get;
        set => SetProperty(ref field, value);
    } = 100;
}
