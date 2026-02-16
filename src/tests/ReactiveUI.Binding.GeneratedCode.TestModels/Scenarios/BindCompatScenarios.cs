// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using ReactiveUI.Binding;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>
/// Scenario methods for Bind (view-first two-way compat alias) that the source generator processes at compile time.
/// </summary>
public static class BindCompatScenarios
{
    /// <summary>
    /// View-first two-way binding for a string property using the Bind compat alias.
    /// </summary>
    /// <param name="view">The target view.</param>
    /// <param name="vm">The source view model.</param>
    /// <returns>A reactive binding representing the binding.</returns>
    public static IReactiveBinding<TestView, (object? view, bool isViewModel)> StringProperty(TestView view, TestViewModel vm)
        => view.Bind(vm, x => x.Name, x => x.DisplayName);
}
