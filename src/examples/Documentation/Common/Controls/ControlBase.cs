// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Controls;

/// <summary>
/// The base of the example controls. A real UI framework supplies controls such as these. The examples use these
/// small stand-ins so a binding runs anywhere without WPF, WinForms or MAUI. Each control raises
/// <c>PropertyChanged</c> when a property changes, as a bindable control in a real framework does.
/// </summary>
[System.Diagnostics.DebuggerDisplay("IsEnabled = {IsEnabled}, IsVisible = {IsVisible}")]
public class ControlBase : ObservableObject
{
    /// <summary>Gets or sets a value indicating whether the user can interact with the control.</summary>
    public bool IsEnabled
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <summary>Gets or sets a value indicating whether the control is shown.</summary>
    public bool IsVisible
    {
        get;
        set => SetProperty(ref field, value);
    } = true;
}
