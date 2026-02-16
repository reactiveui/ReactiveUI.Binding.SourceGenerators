// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenChanged.IntProperty
{
    /// <summary>
    /// Exercises WhenChanged with an integer property type.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a WhenChanged observable for the Count property.
        /// </summary>
        /// <param name="vm">The view model to observe.</param>
        /// <returns>An observable of count values.</returns>
        public static IObservable<int> Execute(MyViewModel vm)
            => vm.WhenChanged(x => x.Count);
    }
}
