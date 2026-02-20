// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyObservable.MultipleInvocationsSameType
{
    /// <summary>
    /// Exercises multiple WhenAnyObservable invocations with the same type signature to test else-if dispatch.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates two WhenAnyObservable calls on the same VM with the same type signature.
        /// </summary>
        /// <param name="vm">The view model to observe.</param>
        /// <returns>A tuple of observables for each command.</returns>
        public static (IObservable<string> Cmd1Obs, IObservable<string> Cmd2Obs) Execute(MyViewModel vm)
            => (vm.WhenAnyObservable(x => x.Command1), vm.WhenAnyObservable(x => x.Command2));
    }
}
