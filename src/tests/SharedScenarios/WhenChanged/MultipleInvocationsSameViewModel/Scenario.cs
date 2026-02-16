// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.MultipleInvocationsSameViewModel
{
    /// <summary>
    /// Exercises multiple WhenChanged invocations on the same ViewModel.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates three separate WhenChanged observables for different properties.
        /// </summary>
        /// <param name="vm">The view model to observe.</param>
        /// <returns>A tuple of observables for name, age, and score.</returns>
        public static (IObservable<string> NameObs, IObservable<int> AgeObs, IObservable<double> ScoreObs) Execute(MyViewModel vm)
            => (vm.WhenChanged(x => x.Name), vm.WhenChanged(x => x.Age), vm.WhenChanged(x => x.Score));
    }
}
