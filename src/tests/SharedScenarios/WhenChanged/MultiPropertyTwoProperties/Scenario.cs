// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.MultiPropertyTwoProperties
{
    /// <summary>
    /// Exercises WhenChanged with two properties returning a tuple.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a WhenChanged observable for Name and Age.
        /// </summary>
        /// <param name="vm">The view model to observe.</param>
        /// <returns>An observable of (name, age) tuples.</returns>
        public static IObservable<(string Name, int Age)> Execute(MyViewModel vm)
            => vm.WhenChanged(x => x.Name, x => x.Age);
    }
}
