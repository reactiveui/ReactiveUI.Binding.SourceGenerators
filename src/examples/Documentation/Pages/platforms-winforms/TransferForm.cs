// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.Documentation.Banking;

namespace ReactiveUI.Binding.Documentation.PlatformsWinForms;

/// <summary>The transfer screen as a Windows Forms form: the amount box, the send button and the labels for validation and the receipt.</summary>
[System.Diagnostics.DebuggerDisplay("ViewModel = {ViewModel}")]
public sealed class TransferForm : Form, IViewFor<TransferViewModel>
{
    /// <summary>Gets or sets the view model the form shows.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TransferViewModel? ViewModel { get; set; }

    /// <summary>Gets the box where the customer types the amount.</summary>
    public TextBox AmountTextBox { get; } = new();

    /// <summary>Gets the button that sends the transfer.</summary>
    public Button TransferButton { get; } = new() { Text = "Send" };

    /// <summary>Gets the label that shows what is wrong with the draft.</summary>
    public Label ValidationLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TransferViewModel?)value;
    }
}
