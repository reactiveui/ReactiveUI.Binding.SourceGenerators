// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>
/// Scenario methods for BindCommand that the source generator processes at compile time.
/// Each method exercises a specific BindCommand overload with different configurations.
/// </summary>
public static class BindCommandScenarios
{
    /// <summary>
    /// Binds the Save command to the SaveButton's Click event with no parameter.
    /// </summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    public static IDisposable BasicNoParam(
        SharedScenarios.BindCommand.BasicNoParam.MyViewModel vm,
        SharedScenarios.BindCommand.BasicNoParam.MyView view)
        => SharedScenarios.BindCommand.BasicNoParam.Scenario.Execute(vm, view);

    /// <summary>
    /// Binds the Save command with an expression-based parameter from the VM.
    /// </summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    public static IDisposable ExpressionParam(
        SharedScenarios.BindCommand.ExpressionParam.MyViewModel vm,
        SharedScenarios.BindCommand.ExpressionParam.MyView view)
        => SharedScenarios.BindCommand.ExpressionParam.Scenario.Execute(vm, view);

    /// <summary>
    /// Binds the Save command with an observable parameter.
    /// </summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <param name="parameter">An observable producing command parameters.</param>
    /// <returns>A disposable representing the binding.</returns>
    public static IDisposable ObservableParam(
        SharedScenarios.BindCommand.ObservableParam.MyViewModel vm,
        SharedScenarios.BindCommand.ObservableParam.MyView view,
        IObservable<string> parameter)
        => SharedScenarios.BindCommand.ObservableParam.Scenario.Execute(vm, view, parameter);

    /// <summary>
    /// Binds the Save command to the SaveButton's MouseUp event via explicit toEvent.
    /// </summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    public static IDisposable CustomEvent(
        SharedScenarios.BindCommand.CustomEvent.MyViewModel vm,
        SharedScenarios.BindCommand.CustomEvent.MyView view)
        => SharedScenarios.BindCommand.CustomEvent.Scenario.Execute(vm, view);

    /// <summary>
    /// Binds the Child.SaveCommand to the SaveButton's Click event via a deep command path.
    /// </summary>
    /// <param name="vm">The source view model.</param>
    /// <param name="view">The target view.</param>
    /// <returns>A disposable representing the binding.</returns>
    public static IDisposable DeepCommandPath(
        SharedScenarios.BindCommand.DeepCommandPath.MyViewModel vm,
        SharedScenarios.BindCommand.DeepCommandPath.MyView view)
        => SharedScenarios.BindCommand.DeepCommandPath.Scenario.Execute(vm, view);
}
