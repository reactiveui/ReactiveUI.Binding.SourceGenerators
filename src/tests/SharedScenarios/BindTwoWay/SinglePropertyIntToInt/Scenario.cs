// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.BindTwoWay.SinglePropertyIntToInt
{
    /// <summary>
    /// Exercises BindTwoWay with int-to-int property binding.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a two-way binding between ViewModel.Count and View.DisplayCount.
        /// </summary>
        /// <param name="vm">The source view model.</param>
        /// <param name="view">The target view.</param>
        /// <returns>A disposable representing the binding.</returns>
        public static IDisposable Execute(MyViewModel vm, MyView view)
            => vm.BindTwoWay(view, x => x.Count, x => x.DisplayCount);
    }
}
