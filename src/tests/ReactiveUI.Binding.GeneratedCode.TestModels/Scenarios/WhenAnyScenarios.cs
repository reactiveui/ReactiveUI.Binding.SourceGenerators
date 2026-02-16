// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using ReactiveUI.Binding;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>
/// Scenario methods for WhenAny (IObservedChange-wrapping) that the source generator processes at compile time.
/// Each method exercises a specific WhenAny overload.
/// </summary>
public static class WhenAnyScenarios
{
    /// <summary>
    /// Single property WhenAny observation with a selector extracting the value.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the Name property value.</returns>
    public static IObservable<string> SingleProperty_Name(TestViewModel vm)
        => vm.WhenAny(x => x.Name, c => c.Value);

    /// <summary>
    /// Single property WhenAny observation returning the IObservedChange directly.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of IObservedChange for the Name property.</returns>
    public static IObservable<IObservedChange<TestViewModel, string>> SingleProperty_Name_ObservedChange(TestViewModel vm)
        => vm.WhenAny(x => x.Name, c => c);

    /// <summary>
    /// Two-property WhenAny observation with a selector combining values.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined name and age string.</returns>
    public static IObservable<string> TwoProperties_NameAge(TestViewModel vm)
        => vm.WhenAny(x => x.Name, x => x.Age, (name, age) => $"{name.Value}_{age.Value}");
}
