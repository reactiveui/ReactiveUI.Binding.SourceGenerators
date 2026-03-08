// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.BindCommand.ExpressionParam
{
    /// <summary>
    /// Exercises BindCommand with an expression-based parameter.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Binds the Save command to the SaveButton's Click event with an expression parameter.
        /// </summary>
        /// <param name="vm">The source view model.</param>
        /// <param name="view">The target view.</param>
        /// <returns>A disposable representing the binding.</returns>
        public static IDisposable Execute(MyViewModel vm, MyView view)
            => view.BindCommand(vm, x => x.Save, x => x.SaveButton, x => x.CurrentItem);
    }
}
