// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using ReactiveUI.Binding;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>
/// Scenario methods for WhenChanged that the source generator processes at compile time.
/// Each method exercises a specific WhenChanged overload.
/// </summary>
public static class WhenChangedScenarios
{
    /// <summary>
    /// Single property observation on TestViewModel.Name.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the Name property value.</returns>
    public static IObservable<string> SingleProperty_Name(TestViewModel vm)
        => vm.WhenChanged(x => x.Name);

    /// <summary>
    /// Single property observation on TestViewModel.Age.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the Age property value.</returns>
    public static IObservable<int> SingleProperty_Age(TestViewModel vm)
        => vm.WhenChanged(x => x.Age);

    /// <summary>
    /// Two-property observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple.</returns>
    public static IObservable<(string property1, int property2)> TwoProperties(BigViewModel vm)
        => vm.WhenChanged(x => x.Prop1, x => x.Prop2);

    /// <summary>
    /// Three-property observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple.</returns>
    public static IObservable<(string property1, int property2, double property3)> ThreeProperties(BigViewModel vm)
        => vm.WhenChanged(x => x.Prop1, x => x.Prop2, x => x.Prop3);

    /// <summary>
    /// Four-property observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple.</returns>
    public static IObservable<(string property1, int property2, double property3, bool property4)> FourProperties(BigViewModel vm)
        => vm.WhenChanged(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4);

    /// <summary>
    /// Two-property observation with a selector function.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined string value.</returns>
    public static IObservable<string> WithSelector_TwoProperties(BigViewModel vm)
        => vm.WhenChanged(x => x.Prop1, x => x.Prop2, (p1, p2) => $"{p1}_{p2}");

    /// <summary>
    /// Deep property chain observation on BigViewModel.Address.City.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the nested City property value.</returns>
    public static IObservable<string> DeepChain_AddressCity(BigViewModel vm)
        => vm.WhenChanged(x => x.Address.City);

    /// <summary>
    /// Deep property chain observation on BigViewModel.Address.Street.
    /// Used for testing intermediate object replacement scenarios.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the nested Street property value.</returns>
    public static IObservable<string> DeepChain_AddressStreet(BigViewModel vm)
        => vm.WhenChanged(x => x.Address.Street);

    /// <summary>
    /// Deep property chain observation on HostTestFixture.Child!.Name.
    /// Uses the null-forgiving operator to test generator support for nullable intermediates.
    /// </summary>
    /// <param name="host">The host fixture to observe.</param>
    /// <returns>An observable of the nested Name property value.</returns>
    public static IObservable<string> DeepChain_ChildName(HostTestFixture host)
        => host.WhenChanged(x => x.Child!.Name);

    /// <summary>
    /// Multi-property observation combining a deep chain and a simple property.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined property values.</returns>
    public static IObservable<(string city, string prop1)> MultiProperty_WithDeepChain(BigViewModel vm)
        => vm.WhenChanged(x => x.Address.City, x => x.Prop1);
}
