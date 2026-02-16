// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyValue.MultiPropertyFiveProperties
{
    /// <summary>
    /// Exercises WhenAnyValue with five properties.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a WhenAnyValue observable for five properties.
        /// </summary>
        /// <param name="vm">The view model to observe.</param>
        /// <returns>An observable of five-property tuples.</returns>
        public static IObservable<(string Prop1, int Prop2, double Prop3, bool Prop4, string Prop5)> Execute(MyViewModel vm)
            => vm.WhenAnyValue(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5);
    }
}
