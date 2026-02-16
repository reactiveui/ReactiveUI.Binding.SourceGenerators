// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using ReactiveUI.Binding;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;

/// <summary>
/// Extended scenario methods for WhenChanging that exercise the 5-16 property overloads.
/// Each method exercises a specific WhenChanging overload for before-change observation at compile time.
/// </summary>
public static class WhenChangingExtendedScenarios
{
    /// <summary>
    /// Five-property before-change observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    public static IObservable<(string property1, int property2, double property3, bool property4, string property5)> FiveProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5);

    /// <summary>
    /// Six-property before-change observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    public static IObservable<(string property1, int property2, double property3, bool property4, string property5, int property6)> SixProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6);

    /// <summary>
    /// Seven-property before-change observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    public static IObservable<(string property1, int property2, double property3, bool property4, string property5, int property6, double property7)> SevenProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7);

    /// <summary>
    /// Eight-property before-change observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    public static IObservable<(string property1, int property2, double property3, bool property4, string property5, int property6, double property7, bool property8)> EightProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8);

    /// <summary>
    /// Nine-property before-change observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    public static IObservable<(string property1, int property2, double property3, bool property4, string property5, int property6, double property7, bool property8, string property9)> NineProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9);

    /// <summary>
    /// Ten-property before-change observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    public static IObservable<(string property1, int property2, double property3, bool property4, string property5, int property6, double property7, bool property8, string property9, int property10)> TenProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, x => x.Prop10);

    /// <summary>
    /// Eleven-property before-change observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    public static IObservable<(string property1, int property2, double property3, bool property4, string property5, int property6, double property7, bool property8, string property9, int property10, double property11)> ElevenProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, x => x.Prop10, x => x.Prop11);

    /// <summary>
    /// Twelve-property before-change observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    public static IObservable<(string property1, int property2, double property3, bool property4, string property5, int property6, double property7, bool property8, string property9, int property10, double property11, bool property12)> TwelveProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, x => x.Prop10, x => x.Prop11, x => x.Prop12);

    /// <summary>
    /// Thirteen-property before-change observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    public static IObservable<(string property1, int property2, double property3, bool property4, string property5, int property6, double property7, bool property8, string property9, int property10, double property11, bool property12, string property13)> ThirteenProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, x => x.Prop10, x => x.Prop11, x => x.Prop12, x => x.Prop13);

    /// <summary>
    /// Fourteen-property before-change observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    public static IObservable<(string property1, int property2, double property3, bool property4, string property5, int property6, double property7, bool property8, string property9, int property10, double property11, bool property12, string property13, int property14)> FourteenProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, x => x.Prop10, x => x.Prop11, x => x.Prop12, x => x.Prop13, x => x.Prop14);

    /// <summary>
    /// Fifteen-property before-change observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    public static IObservable<(string property1, int property2, double property3, bool property4, string property5, int property6, double property7, bool property8, string property9, int property10, double property11, bool property12, string property13, int property14, double property15)> FifteenProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, x => x.Prop10, x => x.Prop11, x => x.Prop12, x => x.Prop13, x => x.Prop14, x => x.Prop15);

    /// <summary>
    /// Sixteen-property before-change observation returning a tuple.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the property value tuple before changes.</returns>
    public static IObservable<(string property1, int property2, double property3, bool property4, string property5, int property6, double property7, bool property8, string property9, int property10, double property11, bool property12, string property13, int property14, double property15, bool property16)> SixteenProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, x => x.Prop10, x => x.Prop11, x => x.Prop12, x => x.Prop13, x => x.Prop14, x => x.Prop15, x => x.Prop16);

    /// <summary>
    /// Five-property before-change observation with a selector function.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined string value before changes.</returns>
    public static IObservable<string> WithSelector_FiveProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, (p1, p2, p3, p4, p5) => $"{p1}_{p2}_{p3}_{p4}_{p5}");

    /// <summary>
    /// Six-property before-change observation with a selector function.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined string value before changes.</returns>
    public static IObservable<string> WithSelector_SixProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, (p1, p2, p3, p4, p5, p6) => $"{p1}_{p2}_{p3}_{p4}_{p5}_{p6}");

    /// <summary>
    /// Seven-property before-change observation with a selector function.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined string value before changes.</returns>
    public static IObservable<string> WithSelector_SevenProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, (p1, p2, p3, p4, p5, p6, p7) => $"{p1}_{p2}_{p3}_{p4}_{p5}_{p6}_{p7}");

    /// <summary>
    /// Eight-property before-change observation with a selector function.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined string value before changes.</returns>
    public static IObservable<string> WithSelector_EightProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, (p1, p2, p3, p4, p5, p6, p7, p8) => $"{p1}_{p2}_{p3}_{p4}_{p5}_{p6}_{p7}_{p8}");

    /// <summary>
    /// Nine-property before-change observation with a selector function.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined string value before changes.</returns>
    public static IObservable<string> WithSelector_NineProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, (p1, p2, p3, p4, p5, p6, p7, p8, p9) => $"{p1}_{p2}_{p3}_{p4}_{p5}_{p6}_{p7}_{p8}_{p9}");

    /// <summary>
    /// Ten-property before-change observation with a selector function.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined string value before changes.</returns>
    public static IObservable<string> WithSelector_TenProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, x => x.Prop10, (p1, p2, p3, p4, p5, p6, p7, p8, p9, p10) => $"{p1}_{p2}_{p3}_{p4}_{p5}_{p6}_{p7}_{p8}_{p9}_{p10}");

    /// <summary>
    /// Eleven-property before-change observation with a selector function.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined string value before changes.</returns>
    public static IObservable<string> WithSelector_ElevenProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, x => x.Prop10, x => x.Prop11, (p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11) => $"{p1}_{p2}_{p3}_{p4}_{p5}_{p6}_{p7}_{p8}_{p9}_{p10}_{p11}");

    /// <summary>
    /// Twelve-property before-change observation with a selector function.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined string value before changes.</returns>
    public static IObservable<string> WithSelector_TwelveProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, x => x.Prop10, x => x.Prop11, x => x.Prop12, (p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12) => $"{p1}_{p2}_{p3}_{p4}_{p5}_{p6}_{p7}_{p8}_{p9}_{p10}_{p11}_{p12}");

    /// <summary>
    /// Thirteen-property before-change observation with a selector function.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined string value before changes.</returns>
    public static IObservable<string> WithSelector_ThirteenProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, x => x.Prop10, x => x.Prop11, x => x.Prop12, x => x.Prop13, (p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13) => $"{p1}_{p2}_{p3}_{p4}_{p5}_{p6}_{p7}_{p8}_{p9}_{p10}_{p11}_{p12}_{p13}");

    /// <summary>
    /// Fourteen-property before-change observation with a selector function.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined string value before changes.</returns>
    public static IObservable<string> WithSelector_FourteenProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, x => x.Prop10, x => x.Prop11, x => x.Prop12, x => x.Prop13, x => x.Prop14, (p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14) => $"{p1}_{p2}_{p3}_{p4}_{p5}_{p6}_{p7}_{p8}_{p9}_{p10}_{p11}_{p12}_{p13}_{p14}");

    /// <summary>
    /// Fifteen-property before-change observation with a selector function.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined string value before changes.</returns>
    public static IObservable<string> WithSelector_FifteenProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, x => x.Prop10, x => x.Prop11, x => x.Prop12, x => x.Prop13, x => x.Prop14, x => x.Prop15, (p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15) => $"{p1}_{p2}_{p3}_{p4}_{p5}_{p6}_{p7}_{p8}_{p9}_{p10}_{p11}_{p12}_{p13}_{p14}_{p15}");

    /// <summary>
    /// Sixteen-property before-change observation with a selector function.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the combined string value before changes.</returns>
    public static IObservable<string> WithSelector_SixteenProperties(BigViewModel vm)
        => vm.WhenChanging(x => x.Prop1, x => x.Prop2, x => x.Prop3, x => x.Prop4, x => x.Prop5, x => x.Prop6, x => x.Prop7, x => x.Prop8, x => x.Prop9, x => x.Prop10, x => x.Prop11, x => x.Prop12, x => x.Prop13, x => x.Prop14, x => x.Prop15, x => x.Prop16, (p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16) => $"{p1}_{p2}_{p3}_{p4}_{p5}_{p6}_{p7}_{p8}_{p9}_{p10}_{p11}_{p12}_{p13}_{p14}_{p15}_{p16}");

    /// <summary>
    /// Deep property chain before-change observation on BigViewModel.Address.City.
    /// </summary>
    /// <param name="vm">The view model to observe.</param>
    /// <returns>An observable of the nested City property value before changes.</returns>
    public static IObservable<string> DeepChain_AddressCity(BigViewModel vm)
        => vm.WhenChanging(x => x.Address.City);
}
