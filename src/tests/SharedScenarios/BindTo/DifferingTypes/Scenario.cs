// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ReactiveUI.Binding;

namespace SharedScenarios.BindTo.DifferingTypes
{
    /// <summary>
    /// Exercises BindTo applying an int observable to a string property, coerced via the converter registry.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Binds the int observable stream to the view's string caption.
        /// </summary>
        /// <param name="source">The source observable stream.</param>
        /// <param name="view">The target view.</param>
        /// <returns>A disposable representing the binding.</returns>
        public static IDisposable Execute(IObservable<int> source, MyView view)
            => source.BindTo(view, x => x.Caption);
    }
}
