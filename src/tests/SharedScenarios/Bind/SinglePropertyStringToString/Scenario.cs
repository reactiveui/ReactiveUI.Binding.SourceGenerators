// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.Bind.SinglePropertyStringToString
{
    /// <summary>
    /// Exercises Bind (view-first two-way) with string-to-string property binding.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a two-way binding between ViewModel.Name and View.NameText using view-first syntax.
        /// </summary>
        /// <param name="view">The target view.</param>
        /// <param name="vm">The source view model.</param>
        /// <returns>A reactive binding representing the binding.</returns>
        public static IReactiveBinding<MyView, (object? view, bool isViewModel)> Execute(MyView view, MyViewModel vm)
            => view.Bind(vm, x => x.Name, x => x.NameText);
    }
}
