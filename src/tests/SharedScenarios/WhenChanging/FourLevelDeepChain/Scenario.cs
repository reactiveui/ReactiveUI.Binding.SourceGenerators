// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanging.FourLevelDeepChain
{
    /// <summary>
    /// Exercises WhenChanging with a 4-level deep property chain.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a WhenChanging observable for Model.Model.Model.Value.
        /// </summary>
        /// <param name="chain">The top-level object in the chain.</param>
        /// <returns>An observable of value strings from the leaf (before change).</returns>
        public static IObservable<string> Execute(Level1 chain)
            => chain.WhenChanging(x => x.Model.Model.Model.Value);
    }
}
