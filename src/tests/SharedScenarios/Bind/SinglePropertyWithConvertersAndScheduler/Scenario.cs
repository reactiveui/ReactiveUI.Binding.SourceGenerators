// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Reactive.Concurrency;

using ReactiveUI.Binding;

namespace SharedScenarios.Bind.SinglePropertyWithConvertersAndScheduler
{
    /// <summary>
    /// Exercises Bind (view-first two-way) with conversion functions and a scheduler.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a two-way binding between ViewModel.Count and View.CountText with converters and scheduler.
        /// </summary>
        /// <param name="view">The target view.</param>
        /// <param name="vm">The source view model.</param>
        /// <param name="scheduler">The scheduler to observe on.</param>
        /// <returns>A reactive binding representing the binding.</returns>
        public static IReactiveBinding<MyView, (object? view, bool isViewModel)> Execute(MyView view, MyViewModel vm, IScheduler scheduler)
            => view.Bind(vm, x => x.Count, x => x.CountText, count => count.ToString(), text => int.Parse(text), scheduler);
    }
}
