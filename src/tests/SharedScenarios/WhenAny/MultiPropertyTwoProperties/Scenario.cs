// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenAny.MultiPropertyTwoProperties
{
    /// <summary>
    /// Exercises WhenAny with two properties and a selector combining IObservedChange values.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a WhenAny observable combining FirstName and LastName via IObservedChange.
        /// </summary>
        /// <param name="vm">The view model to observe.</param>
        /// <returns>An observable of combined name strings.</returns>
        public static IObservable<string> Execute(MyViewModel vm)
            => vm.WhenAny(x => x.FirstName, x => x.LastName, (c1, c2) => $"{c1.Value} {c2.Value}");
    }
}
