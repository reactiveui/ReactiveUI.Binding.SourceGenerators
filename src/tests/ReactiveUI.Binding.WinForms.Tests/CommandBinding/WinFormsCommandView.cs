// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace ReactiveUI.Binding.WinForms.Tests.CommandBinding;

/// <summary>
/// View containing real WinForms controls for command binding tests.
/// </summary>
#pragma warning disable CS0067 // Event is never used
public class WinFormsCommandView : IViewFor, INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public object? ViewModel { get; set; }

    /// <summary>
    /// Gets the save button (real WinForms Button with Click event and Enabled property).
    /// </summary>
    public Button SaveButton { get; } = new Button();

    /// <summary>
    /// Gets the tool strip button (Component with Click event and Enabled property).
    /// </summary>
    public ToolStripButton ToolStripSaveButton { get; } = new ToolStripButton();
}
#pragma warning restore CS0067
