// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.Documentation.CloudStorage;

namespace ReactiveUI.Binding.Documentation.PlatformsWinForms;

/// <summary>The storage upload screen as a Windows Forms form: the progress bar of the running upload.</summary>
[System.Diagnostics.DebuggerDisplay("ViewModel = {ViewModel}")]
public sealed class UploadForm : Form, IViewFor<StorageBrowserViewModel>
{
    /// <summary>The value of a full progress bar.</summary>
    private const int FullPercent = 100;

    /// <summary>Gets or sets the view model the form shows.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public StorageBrowserViewModel? ViewModel { get; set; }

    /// <summary>Gets the bar that shows how much of the running upload the service has received.</summary>
    public ProgressBar UploadProgressBar { get; } = new() { Minimum = 0, Maximum = FullPercent };

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (StorageBrowserViewModel?)value;
    }
}
