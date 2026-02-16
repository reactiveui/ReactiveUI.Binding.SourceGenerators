// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI;
using ReactiveUI.Binding;

namespace SharedScenarios.BindOneWay.ReactiveObjectSource
{
    /// <summary>
    /// Exercises BindOneWay with a ReactiveObject source.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a one-way binding from ReactiveObject ViewModel.Name to View.NameText.
        /// </summary>
        /// <param name="vm">The source view model.</param>
        /// <param name="view">The target view.</param>
        /// <returns>A disposable representing the binding.</returns>
        public static IDisposable Execute(MyViewModel vm, MyView view)
            => vm.BindOneWay(view, x => x.Name, x => x.NameText);
    }
}
