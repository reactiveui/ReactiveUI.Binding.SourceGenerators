// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Controls;

namespace ReactiveUI.Binding.Documentation.ThreadingInvokers;

/// <summary>Read-only text that belongs to the UI thread of a <see cref="UiDispatcher"/>. Writing the text from another thread fails, as it does in a real UI framework.</summary>
/// <param name="dispatcher">The dispatcher of the thread that owns the label.</param>
[System.Diagnostics.DebuggerDisplay("Text = {Text}")]
public sealed class DispatcherLabelControl(UiDispatcher dispatcher) : ControlBase
{
    /// <summary>Gets the dispatcher of the thread that owns the label.</summary>
    public UiDispatcher Dispatcher { get; } = dispatcher;

    /// <summary>Gets the number of times the text changed.</summary>
    public int WriteCount { get; private set; }

    /// <summary>Gets or sets the text the label shows.</summary>
    /// <exception cref="InvalidOperationException">The text is set from a thread that does not own the label.</exception>
    public string Text
    {
        get;
        set
        {
            Dispatcher.VerifyAccess();

            if (SetProperty(ref field, value))
            {
                WriteCount++;
            }
        }
    } = string.Empty;
}
