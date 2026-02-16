// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

using ReactiveUI.Binding;

namespace SharedScenarios.WhenAnyObservable.SingleObservable
{
    /// <summary>
    /// Exercises WhenAnyObservable on a single observable property.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a WhenAnyObservable that switches to the latest value of MyCommand.
        /// </summary>
        /// <param name="vm">The view model to observe.</param>
        /// <returns>An observable that switches to the latest MyCommand observable.</returns>
        public static IObservable<string> Execute(MyViewModel vm)
            => vm.WhenAnyObservable(x => x.MyCommand);
    }
}
