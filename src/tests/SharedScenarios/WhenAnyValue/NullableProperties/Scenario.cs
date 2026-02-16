// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyValue.NullableProperties
{
    /// <summary>
    /// Exercises WhenAnyValue with nullable property types.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates WhenAnyValue observables for nullable properties.
        /// </summary>
        /// <param name="vm">The view model to observe.</param>
        /// <returns>A tuple of observables for nullable name and age.</returns>
        public static (IObservable<string?> NameObs, IObservable<int?> AgeObs) Execute(MyViewModel vm)
            => (vm.WhenAnyValue(x => x.NullableName!), vm.WhenAnyValue(x => x.NullableAge!));
    }
}
