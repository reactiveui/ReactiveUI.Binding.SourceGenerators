// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.BindInteraction.ObservableHandler
{
    /// <summary>
    /// Exercises BindInteraction with an observable-based handler.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Binds an observable handler to the ViewModel's Confirm interaction.
        /// </summary>
        /// <param name="vm">The source view model.</param>
        /// <param name="view">The target view.</param>
        /// <returns>A disposable representing the binding.</returns>
        public static IDisposable Execute(MyViewModel vm, MyView view)
            => view.BindInteraction(vm, x => x.Confirm, ctx =>
            {
                ctx.SetOutput(true);
                return new ReactiveUI.Binding.Observables.ReturnObservable<int>(0);
            });
    }
}
