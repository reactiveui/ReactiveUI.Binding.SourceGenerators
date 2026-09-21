// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ReactiveUI.Binding.Documentation.Infrastructure;

/// <summary>An <see cref="ICommand"/> that runs a method and lets a view model say when it may run.</summary>
/// <param name="execute">The method the command runs.</param>
/// <param name="canExecute">The check that decides whether the command may run; <see langword="null"/> means it always may.</param>
public sealed class DelegateCommand(Action<object?> execute, Func<object?, bool>? canExecute = null) : ICommand
{
    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc/>
    public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;

    /// <inheritdoc/>
    public void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            execute(parameter);
        }
    }

    /// <summary>Tells the controls bound to this command to ask <see cref="CanExecute"/> again.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
