// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ReactiveUI.Binding.Documentation.Controls;

/// <summary>
/// A push button. It raises <see cref="Click"/> and runs its <see cref="Command"/> when pressed. It is disabled
/// while the command cannot run. A real UI framework supplies this control.
/// </summary>
[System.Diagnostics.DebuggerDisplay("Content = {Content}, IsEnabled = {IsEnabled}")]
public sealed class ButtonControl : ControlBase
{
    /// <summary>The command the button is currently attached to.</summary>
    private ICommand? _attached;

    /// <summary>Occurs when the user presses the button.</summary>
    public event EventHandler? Click;

    /// <summary>Gets or sets the caption on the button.</summary>
    public string Content
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets the command the button runs.</summary>
    public ICommand? Command
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            Attach(value);
        }
    }

    /// <summary>Gets or sets the value passed to the command.</summary>
    public object? CommandParameter
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                RefreshEnabled();
            }
        }
    }

    /// <summary>Presses the button as the user would: raises <see cref="Click"/> and runs the command when it can run.</summary>
    public void Press()
    {
        if (!IsEnabled)
        {
            return;
        }

        Click?.Invoke(this, EventArgs.Empty);
        Command?.Execute(CommandParameter);
    }

    /// <summary>Follows a new command, and stops following the previous one.</summary>
    /// <param name="command">The new command, if any.</param>
    private void Attach(ICommand? command)
    {
        if (_attached is not null)
        {
            _attached.CanExecuteChanged -= OnCanExecuteChanged;
        }

        _attached = command;

        if (command is not null)
        {
            command.CanExecuteChanged += OnCanExecuteChanged;
        }

        RefreshEnabled();
    }

    /// <summary>Re-evaluates the command when it reports a change.</summary>
    /// <param name="sender">The command.</param>
    /// <param name="e">The event data.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OnCanExecuteChanged(object? sender, EventArgs e) => RefreshEnabled();

    /// <summary>Enables the button only while the command can run.</summary>
    private void RefreshEnabled()
    {
        if (Command is not null)
        {
            IsEnabled = Command.CanExecute(CommandParameter);
        }
    }
}
