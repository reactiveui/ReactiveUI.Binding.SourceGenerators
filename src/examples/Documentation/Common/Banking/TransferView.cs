// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Banking;

/// <summary>
/// The transfer screen: the account and payee pickers, the amount and reference boxes, and the send button. A real
/// UI framework builds these controls from markup; here the view creates them in code.
/// </summary>
[System.Diagnostics.DebuggerDisplay("ViewModel = {ViewModel}")]
public sealed class TransferView : ObservableObject, IViewFor<TransferViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public TransferViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the list of accounts the money can leave.</summary>
    public ItemsListControl<Account> SourceList { get; } = new();

    /// <summary>Gets the list of payees.</summary>
    public ItemsListControl<Payee> PayeeList { get; } = new();

    /// <summary>Gets the box where the customer types the amount.</summary>
    public TextBoxControl AmountTextBox { get; } = new() { Placeholder = "0.00" };

    /// <summary>Gets the box where the customer types the reference.</summary>
    public TextBoxControl ReferenceTextBox { get; } = new() { Placeholder = "Reference" };

    /// <summary>Gets the button that sends the transfer.</summary>
    public ButtonControl TransferButton { get; } = new() { Content = "Send" };

    /// <summary>Gets the label that shows what is wrong with the draft.</summary>
    public LabelControl ValidationLabel { get; } = new();

    /// <summary>Gets the label that shows the receipt of the last transfer.</summary>
    public LabelControl ReceiptLabel { get; } = new();

    /// <summary>Gets the label that shows the last error.</summary>
    public LabelControl ErrorLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TransferViewModel?)value;
    }
}
