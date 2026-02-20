// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenAny.MultiPropertyDeepChain
{
    /// <summary>
    /// Exercises WhenAny with multiple properties where one is a deep chain.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a WhenAny observable combining a deep chain (Child.Name) and a shallow property (Title).
        /// </summary>
        /// <param name="vm">The parent view model to observe.</param>
        /// <returns>An observable of combined name and title strings.</returns>
        public static IObservable<string> Execute(ParentViewModel vm)
            => vm.WhenAny(x => x.Child.Name, x => x.Title, (c1, c2) => $"{c1.Value} - {c2.Value}");
    }
}
