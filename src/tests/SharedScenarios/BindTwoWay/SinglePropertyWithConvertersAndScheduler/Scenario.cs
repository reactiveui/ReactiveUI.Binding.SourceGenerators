// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Reactive.Concurrency;

using ReactiveUI.Binding;

namespace SharedScenarios.BindTwoWay.SinglePropertyWithConvertersAndScheduler
{
    /// <summary>
    /// Exercises BindTwoWay with conversion functions and a scheduler.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a two-way binding between ViewModel.Count and View.CountText with converters and scheduler.
        /// </summary>
        /// <param name="vm">The source view model.</param>
        /// <param name="view">The target view.</param>
        /// <param name="scheduler">The scheduler to observe on.</param>
        /// <returns>A disposable representing the binding.</returns>
        public static IDisposable Execute(MyViewModel vm, MyView view, IScheduler scheduler)
            => vm.BindTwoWay(
                view,
                x => x.Count,
                x => x.CountText,
                count => count.ToString(),
                text => int.Parse(text),
                scheduler);
    }
}
