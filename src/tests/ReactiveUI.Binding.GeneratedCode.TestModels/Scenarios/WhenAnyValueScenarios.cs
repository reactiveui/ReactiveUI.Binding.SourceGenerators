// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using ReactiveUI.Binding;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>
/// Scenario methods for WhenAnyValue that the source generator processes at compile time.
/// Each method exercises a specific WhenAnyValue overload.
/// </summary>
public static class WhenAnyValueScenarios
{
    /// <summary>
    /// Single property observation using WhenAnyValue.
    /// </summary>
    /// <param name="fixture">The fixture to observe.</param>
    /// <returns>An observable of the Value1 property value.</returns>
    public static IObservable<string> SingleProperty(WhenAnyTestFixture fixture)
        => fixture.WhenAnyValue(x => x.Value1);

    /// <summary>
    /// Two-property observation using WhenAnyValue returning a tuple.
    /// </summary>
    /// <param name="fixture">The fixture to observe.</param>
    /// <returns>An observable of the property value tuple.</returns>
    public static IObservable<(string property1, string property2)> TwoProperties(WhenAnyTestFixture fixture)
        => fixture.WhenAnyValue(x => x.Value1, x => x.Value2);

    /// <summary>
    /// Three-property observation using WhenAnyValue returning a tuple.
    /// </summary>
    /// <param name="fixture">The fixture to observe.</param>
    /// <returns>An observable of the property value tuple.</returns>
    public static IObservable<(string property1, string property2, string property3)> ThreeProperties(WhenAnyTestFixture fixture)
        => fixture.WhenAnyValue(x => x.Value1, x => x.Value2, x => x.Value3);

    /// <summary>
    /// Two-property observation with a selector using WhenAnyValue.
    /// </summary>
    /// <param name="fixture">The fixture to observe.</param>
    /// <returns>An observable of the combined string value.</returns>
    public static IObservable<string> WithSelector_TwoProperties(WhenAnyTestFixture fixture)
        => fixture.WhenAnyValue(x => x.Value1, x => x.Value2, (v1, v2) => $"{v1}_{v2}");

    /// <summary>
    /// Deep property chain observation using WhenAnyValue on BigViewModel.Address.City.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the nested City property value.</returns>
    public static IObservable<string> DeepChain_AddressCity(BigViewModel vm)
        => vm.WhenAnyValue(x => x.Address.City);
}
