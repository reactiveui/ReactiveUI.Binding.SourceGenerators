// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ReactiveUI.Binding.Documentation.Infrastructure;

/// <summary>
/// An <see cref="ICommand"/> that runs an asynchronous method. It refuses to run again while a run is in progress,
/// and it keeps the task of the latest run so an example can wait for it.
/// </summary>
/// <param name="execute">The method the command runs.</param>
/// <param name="canExecute">The check that decides whether the command may run; <see langword="null"/> means it always may.</param>
[System.Diagnostics.DebuggerDisplay("IsRunning = {IsRunning}")]
public sealed class AsyncDelegateCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null) : ICommand
{
    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged;

    /// <summary>Gets a value indicating whether a run is in progress.</summary>
    public bool IsRunning { get; private set; }

    /// <summary>Gets the task of the latest run, which is complete when no run has started.</summary>
    public Task Completion { get; private set; } = Task.CompletedTask;

    /// <inheritdoc/>
    public bool CanExecute(object? parameter) => !IsRunning && (canExecute?.Invoke(parameter) ?? true);

    /// <inheritdoc/>
    public void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            Completion = RunAsync(parameter);
        }
    }

    /// <summary>Tells the controls bound to this command to ask <see cref="CanExecute"/> again.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>Runs the method and keeps <see cref="IsRunning"/> true until it finishes.</summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns>A task that completes when the method finishes.</returns>
    private async Task RunAsync(object? parameter)
    {
        IsRunning = true;
        RaiseCanExecuteChanged();

        try
        {
            await execute(parameter).ConfigureAwait(false);
        }
        finally
        {
            IsRunning = false;
            RaiseCanExecuteChanged();
        }
    }
}
