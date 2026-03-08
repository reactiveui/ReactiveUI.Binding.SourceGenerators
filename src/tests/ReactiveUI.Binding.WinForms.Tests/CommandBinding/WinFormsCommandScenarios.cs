// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.WinForms.Tests.CommandBinding;

/// <summary>
/// Scenario methods for BindCommand with real WinForms controls.
/// The source generator processes these call sites at compile time.
/// </summary>
public static class WinFormsCommandScenarios
{
    /// <summary>
    /// Binds the Save command to a real WinForms Button control.
    /// </summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    public static IDisposable ButtonBasic(WinFormsCommandViewModel vm, WinFormsCommandView view)
        => view.BindCommand(vm, x => x.Save, x => x.SaveButton);

    /// <summary>
    /// Binds the Save command to a real WinForms Button with an expression parameter.
    /// </summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    public static IDisposable ButtonWithExpressionParam(WinFormsCommandViewModel vm, WinFormsCommandView view)
        => view.BindCommand(vm, x => x.Save, x => x.SaveButton, x => x.CurrentItem);

    /// <summary>
    /// Binds the Save command to a real WinForms Button with an observable parameter.
    /// </summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <param name="parameter">An observable producing command parameters.</param>
    /// <returns>A disposable representing the binding.</returns>
    public static IDisposable ButtonWithObservableParam(WinFormsCommandViewModel vm, WinFormsCommandView view, IObservable<string> parameter)
        => view.BindCommand(vm, x => x.Save, x => x.SaveButton, parameter);

    /// <summary>
    /// Binds the Save command to a WinForms Button using a custom MouseUp event.
    /// </summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    public static IDisposable ButtonCustomEvent(WinFormsCommandViewModel vm, WinFormsCommandView view)
        => view.BindCommand(vm, x => x.Save, x => x.SaveButton, "MouseUp");

    /// <summary>
    /// Binds the Save command to a real WinForms ToolStripButton (Component, not Control).
    /// </summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    public static IDisposable ToolStripButton(WinFormsCommandViewModel vm, WinFormsCommandView view)
        => view.BindCommand(vm, x => x.Save, x => x.ToolStripSaveButton);
}
