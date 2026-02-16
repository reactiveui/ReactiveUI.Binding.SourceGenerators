// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanging.SinglePropertyINPC
{
    /// <summary>
    /// Exercises WhenChanging on a single INPC property.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a WhenChanging observable for the Name property.
        /// </summary>
        /// <param name="vm">The view model to observe.</param>
        /// <returns>An observable of name values (before change).</returns>
        public static IObservable<string> Execute(MyViewModel vm)
            => vm.WhenChanging(x => x.Name);
    }
}
