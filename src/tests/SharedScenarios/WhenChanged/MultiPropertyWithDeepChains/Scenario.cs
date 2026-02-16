// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.MultiPropertyWithDeepChains
{
    /// <summary>
    /// Exercises WhenChanged with a mix of deep chain (Address.City) and shallow (Name) properties.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a WhenChanged observable combining Address.City and Name with a selector.
        /// </summary>
        /// <param name="vm">The view model to observe.</param>
        /// <returns>An observable of combined city and name strings.</returns>
        public static IObservable<string> Execute(MyViewModel vm)
            => vm.WhenChanged(x => x.Address.City, x => x.Name, (city, name) => $"{city}: {name}");
    }
}
