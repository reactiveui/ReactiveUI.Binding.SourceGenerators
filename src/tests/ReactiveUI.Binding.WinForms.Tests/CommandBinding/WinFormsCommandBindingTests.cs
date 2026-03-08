// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using System.Windows.Input;

namespace ReactiveUI.Binding.WinForms.Tests.CommandBinding;

/// <summary>
/// Tests that source-generator-generated BindCommand code works correctly with real WinForms controls.
/// These tests run only on Windows where WinForms types are available.
/// </summary>
[NotInParallel]
public class WinFormsCommandBindingTests
{
    /// <summary>
    /// Verifies that clicking a real WinForms Button executes the bound command.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Button_ClickExecutesCommand()
    {
        var vm = new WinFormsCommandViewModel();
        var view = new WinFormsCommandView();
        var command = new TrackingCommand();
        vm.Save = command;

        using var binding = WinFormsCommandScenarios.ButtonBasic(vm, view);

        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that setting the command after binding still wires correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Button_CommandSetAfterBinding_ExecutesOnClick()
    {
        var vm = new WinFormsCommandViewModel();
        var view = new WinFormsCommandView();
        var command = new TrackingCommand();

        using var binding = WinFormsCommandScenarios.ButtonBasic(vm, view);

        vm.Save = command;
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that when CanExecute is false, the command is not executed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Button_CanExecuteFalse_DoesNotExecute()
    {
        var vm = new WinFormsCommandViewModel();
        var view = new WinFormsCommandView();
        var command = new TrackingCommand { CanExecuteResult = false };
        vm.Save = command;

        using var binding = WinFormsCommandScenarios.ButtonBasic(vm, view);
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(0);
    }

    /// <summary>
    /// Verifies that disposing the binding prevents further command execution.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Button_Dispose_StopsExecution()
    {
        var vm = new WinFormsCommandViewModel();
        var view = new WinFormsCommandView();
        var command = new TrackingCommand();
        vm.Save = command;

        var binding = WinFormsCommandScenarios.ButtonBasic(vm, view);
        view.SaveButton.PerformClick();
        await Assert.That(command.ExecuteCount).IsEqualTo(1);

        binding.Dispose();
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that when the command changes, the old command is unwired and the new command is wired.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Button_CommandChanges_NewCommandExecutes()
    {
        var vm = new WinFormsCommandViewModel();
        var view = new WinFormsCommandView();
        var first = new TrackingCommand();
        var second = new TrackingCommand();
        vm.Save = first;

        using var binding = WinFormsCommandScenarios.ButtonBasic(vm, view);

        view.SaveButton.PerformClick();
        await Assert.That(first.ExecuteCount).IsEqualTo(1);

        vm.Save = second;
        view.SaveButton.PerformClick();

        await Assert.That(second.ExecuteCount).IsEqualTo(1);
        await Assert.That(first.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that an expression parameter is passed to the command on click.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Button_ExpressionParam_PassesParameter()
    {
        var vm = new WinFormsCommandViewModel { CurrentItem = "TestItem" };
        var view = new WinFormsCommandView();
        var command = new TrackingCommand();
        vm.Save = command;

        using var binding = WinFormsCommandScenarios.ButtonWithExpressionParam(vm, view);
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
        await Assert.That(command.LastParameter).IsEqualTo("TestItem");
    }

    /// <summary>
    /// Verifies that an observable parameter is passed to the command on click.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Button_ObservableParam_PassesParameter()
    {
        var vm = new WinFormsCommandViewModel();
        var view = new WinFormsCommandView();
        var command = new TrackingCommand();
        var paramSubject = new BehaviorSubject<string>("obs-param");
        vm.Save = command;

        using var binding = WinFormsCommandScenarios.ButtonWithObservableParam(vm, view, paramSubject);
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
        await Assert.That(command.LastParameter).IsEqualTo("obs-param");
    }

    /// <summary>
    /// Verifies that the observable parameter updates reactively.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Button_ObservableParam_ParameterUpdates()
    {
        var vm = new WinFormsCommandViewModel();
        var view = new WinFormsCommandView();
        var command = new TrackingCommand();
        var paramSubject = new BehaviorSubject<string>("initial");
        vm.Save = command;

        using var binding = WinFormsCommandScenarios.ButtonWithObservableParam(vm, view, paramSubject);

        paramSubject.OnNext("updated");
        view.SaveButton.PerformClick();

        await Assert.That(command.LastParameter).IsEqualTo("updated");
    }

    /// <summary>
    /// Verifies that a custom MouseUp event fires the command.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Button_CustomMouseUpEvent_ExecutesCommand()
    {
        var vm = new WinFormsCommandViewModel();
        var view = new WinFormsCommandView();
        var command = new TrackingCommand();
        vm.Save = command;

        using var binding = WinFormsCommandScenarios.ButtonCustomEvent(vm, view);

        // Raise MouseUp event on the button
        var mouseArgs = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
        view.SaveButton.GetType()
            .GetMethod("OnMouseUp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(view.SaveButton, [mouseArgs]);

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that BindCommand works with a real ToolStripButton (Component, not Control).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ToolStripButton_ClickExecutesCommand()
    {
        var vm = new WinFormsCommandViewModel();
        var view = new WinFormsCommandView();
        var command = new TrackingCommand();
        vm.Save = command;

        using var binding = WinFormsCommandScenarios.ToolStripButton(vm, view);

        view.ToolStripSaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(1);
    }

    /// <summary>
    /// Verifies that a null command doesn't throw when clicking a real Button.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Button_NullCommand_ClickDoesNotThrow()
    {
        var vm = new WinFormsCommandViewModel();
        var view = new WinFormsCommandView();

        using var binding = WinFormsCommandScenarios.ButtonBasic(vm, view);

        var action = () => view.SaveButton.PerformClick();
        await Assert.That(action).ThrowsNothing();
    }

    /// <summary>
    /// Verifies that multiple clicks result in multiple executions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Button_MultipleClicks_ExecutesMultipleTimes()
    {
        var vm = new WinFormsCommandViewModel();
        var view = new WinFormsCommandView();
        var command = new TrackingCommand();
        vm.Save = command;

        using var binding = WinFormsCommandScenarios.ButtonBasic(vm, view);

        view.SaveButton.PerformClick();
        view.SaveButton.PerformClick();
        view.SaveButton.PerformClick();

        await Assert.That(command.ExecuteCount).IsEqualTo(3);
    }

    /// <summary>
    /// A simple <see cref="ICommand"/> implementation that tracks invocations for testing.
    /// </summary>
    private sealed class TrackingCommand : ICommand
    {
        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="CanExecute"/> returns <see langword="true"/>.
        /// </summary>
        public bool CanExecuteResult { get; set; } = true;

        /// <summary>
        /// Gets the number of times <see cref="Execute"/> has been called.
        /// </summary>
        public int ExecuteCount { get; private set; }

        /// <summary>
        /// Gets the parameter passed to the most recent <see cref="Execute"/> call.
        /// </summary>
        public object? LastParameter { get; private set; }

        /// <inheritdoc/>
        public bool CanExecute(object? parameter) => CanExecuteResult;

        /// <inheritdoc/>
        public void Execute(object? parameter)
        {
            ExecuteCount++;
            LastParameter = parameter;
        }
    }
}
