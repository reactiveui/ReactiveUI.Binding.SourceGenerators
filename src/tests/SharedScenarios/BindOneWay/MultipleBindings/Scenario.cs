// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.BindOneWay.MultipleBindings
{
    /// <summary>
    /// Exercises BindOneWay with multiple bindings on the same source/target pair.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates two one-way bindings for Name and Age properties.
        /// </summary>
        /// <param name="vm">The source view model.</param>
        /// <param name="view">The target view.</param>
        /// <returns>A tuple of disposables representing the bindings.</returns>
        public static (IDisposable NameBinding, IDisposable AgeBinding) Execute(MyViewModel vm, MyView view)
            => (vm.BindOneWay(view, x => x.Name, x => x.NameText), vm.BindOneWay(view, x => x.Age, x => x.AgeDisplay));
    }
}
