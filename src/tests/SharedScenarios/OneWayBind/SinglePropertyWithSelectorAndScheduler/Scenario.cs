// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Reactive.Concurrency;

using ReactiveUI.Binding;

namespace SharedScenarios.OneWayBind.SinglePropertyWithSelectorAndScheduler
{
    /// <summary>
    /// Exercises OneWayBind (view-first) with a conversion function and scheduler.
    /// </summary>
    public static class Scenario
    {
        /// <summary>
        /// Creates a one-way binding from ViewModel.Count to View.CountText with int-to-string conversion and scheduler.
        /// </summary>
        /// <param name="view">The target view.</param>
        /// <param name="vm">The source view model.</param>
        /// <param name="scheduler">The scheduler to observe on.</param>
        /// <returns>A reactive binding representing the binding.</returns>
        public static IReactiveBinding<MyView, string> Execute(MyView view, MyViewModel vm, IScheduler scheduler)
            => view.OneWayBind(vm, x => x.Count, x => x.CountText, count => count.ToString(), scheduler);
    }
}
