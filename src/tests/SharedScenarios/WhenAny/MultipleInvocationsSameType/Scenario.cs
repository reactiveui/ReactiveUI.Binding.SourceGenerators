// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenAny.MultipleInvocationsSameType
{
    /// <summary>
    /// Exercises multiple WhenAny invocations with the same type signature to test else-if dispatch.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates two WhenAny observables with the same type signature (MyViewModel, string, string).
        /// </summary>
        /// <param name="vm">The view model to observe.</param>
        /// <returns>A tuple of observables for first name and last name.</returns>
        public static (IObservable<string> FirstObs, IObservable<string> LastObs) Execute(MyViewModel vm)
            => (vm.WhenAny(x => x.FirstName, c => c.Value), vm.WhenAny(x => x.LastName, c => c.Value));
    }
}
