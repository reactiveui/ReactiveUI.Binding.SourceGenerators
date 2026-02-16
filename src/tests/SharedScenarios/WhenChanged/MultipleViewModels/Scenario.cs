// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.MultipleViewModels
{
    /// <summary>
    /// Exercises WhenChanged with multiple ViewModels in the same compilation.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates WhenChanged observables for two different ViewModels.
        /// </summary>
        /// <param name="vm1">The first view model.</param>
        /// <param name="vm2">The second view model.</param>
        /// <returns>A tuple of observables for name and count.</returns>
        public static (IObservable<string> NameObs, IObservable<int> CountObs) Execute(ViewModel1 vm1, ViewModel2 vm2)
            => (vm1.WhenChanged(x => x.Name), vm2.WhenChanged(x => x.Count));
    }
}
