// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.BindingsCommands;

/// <summary>
/// The upload dialog of the storage browser. The user picks a file, and the upload button hands the picked file to the
/// browser's upload command as its parameter.
/// </summary>
/// <param name="browser">The storage browser the dialog uploads through.</param>
[System.Diagnostics.DebuggerDisplay("PendingUpload = {PendingUpload}")]
public sealed class UploadPanelViewModel(StorageBrowserViewModel browser) : ObservableObject
{
    /// <summary>Gets the command that uploads the picked file; it is the upload command of the browser.</summary>
    public AsyncDelegateCommand UploadCommand { get; } = browser.UploadCommand;

    /// <summary>Gets or sets the file the user picked, or <see langword="null"/> before a file is picked.</summary>
    public UploadRequest? PendingUpload
    {
        get;
        set => SetProperty(ref field, value);
    }
}
