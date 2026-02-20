// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.OneWayBind.TwoSameTypeBindings
{
    /// <summary>
    /// Exercises OneWayBind (view-first) with two bindings of the same type signature (string-to-string).
    /// This ensures the generator emits "else if" for the second invocation in the dispatch method.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates two string-to-string one-way bindings using view-first syntax.
        /// Both bindings have the same type signature, causing them to be grouped together.
        /// </summary>
        /// <param name="view">The target view.</param>
        /// <param name="vm">The source view model.</param>
        /// <returns>A tuple of bindings.</returns>
        public static (IReactiveBinding<MyView, string> first, IReactiveBinding<MyView, string> last) Execute(MyView view, MyViewModel vm)
            => (view.OneWayBind(vm, x => x.FirstName, x => x.FirstNameText),
                view.OneWayBind(vm, x => x.LastName, x => x.LastNameText));
    }
}
