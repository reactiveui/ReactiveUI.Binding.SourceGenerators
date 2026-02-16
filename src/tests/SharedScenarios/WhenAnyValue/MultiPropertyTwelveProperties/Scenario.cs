// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyValue.MultiPropertyTwelveProperties
{
    /// <summary>
    /// Exercises WhenAnyValue with twelve properties (maximum standard overload).
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a WhenAnyValue observable for twelve properties.
        /// </summary>
        /// <param name="fixture">The fixture to observe.</param>
        /// <returns>An observable of twelve-property tuples.</returns>
        public static IObservable<(string V1, string V2, string V3, string V4, string V5, string V6, string V7, string V8, string V9, string V10, string V11, string V12)> Execute(WhenAnyFixture fixture)
            => fixture.WhenAnyValue(
                x => x.Value1,
                x => x.Value2,
                x => x.Value3,
                x => x.Value4,
                x => x.Value5,
                x => x.Value6,
                x => x.Value7,
                x => x.Value8,
                x => x.Value9,
                x => x.Value10,
                x => x.Value11,
                x => x.Value12);
    }
}
