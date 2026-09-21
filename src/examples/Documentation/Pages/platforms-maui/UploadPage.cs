// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.CloudStorage;

namespace ReactiveUI.Binding.Documentation.PlatformsMaui;

/// <summary>The storage upload screen as a MAUI page: the progress bar of the running upload.</summary>
[System.Diagnostics.DebuggerDisplay("ViewModel = {ViewModel}")]
public sealed class UploadPage : ContentPage, IViewFor<StorageBrowserViewModel>
{
    /// <summary>Gets or sets the view model the page shows.</summary>
    public StorageBrowserViewModel? ViewModel { get; set; }

    /// <summary>Gets the bar that shows how much of the running upload the service has received, from 0 to 1.</summary>
    public ProgressBar UploadProgressBar { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (StorageBrowserViewModel?)value;
    }
}
