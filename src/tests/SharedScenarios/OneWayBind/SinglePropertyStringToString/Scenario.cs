// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.OneWayBind.SinglePropertyStringToString
{
    /// <summary>
    /// Exercises OneWayBind (view-first) with string-to-string property binding.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a one-way binding from ViewModel.Name to View.NameText using view-first syntax.
        /// </summary>
        /// <param name="view">The target view.</param>
        /// <param name="vm">The source view model.</param>
        /// <returns>A reactive binding representing the binding.</returns>
        public static IReactiveBinding<MyView, string> Execute(MyView view, MyViewModel vm)
            => view.OneWayBind(vm, x => x.Name, x => x.NameText);
    }
}
