// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveUI.Binding.AotValidation;

/// <summary>
/// Entry point that exercises the source-generated bindings to confirm they work under Native AOT.
/// Each scenario is a dedicated method so the validation surface is easy to read and extend.
/// </summary>
internal static class Program
{
    /// <summary>A sample name value used across the WhenChanged scenarios.</summary>
    private const string Alice = "Alice";

    /// <summary>The updated string value used to assert change propagation.</summary>
    private const string UpdatedName = "Updated";

    /// <summary>The initial source name value used in the binding scenarios.</summary>
    private const string SourceName = "Source";

    /// <summary>The initial age value used in the two-property scenario.</summary>
    private const int InitialAge = 30;

    /// <summary>The updated age value used to assert change propagation.</summary>
    private const int UpdatedAge = 31;

    /// <summary>The number of scenarios that have passed.</summary>
    private static int _passed;

    /// <summary>The number of scenarios that have failed.</summary>
    private static int _failed;

    /// <summary>
    /// Runs every AOT binding validation scenario and reports the aggregate result.
    /// </summary>
    /// <returns>Zero if every scenario passed; otherwise, one.</returns>
    private static int Main()
    {
        ValidateWhenChangedSingleProperty();
        ValidateWhenChangedDeepChain();
        ValidateWhenChangedTwoProperties();
        ValidateWhenChangedOnViewType();
        ValidateBindOneWay();
        ValidateBindTwoWay();
        ValidateBindOneWayDisposal();

        Console.WriteLine();
        Console.WriteLine($"AOT Validation: {_passed} passed, {_failed} failed");
        return _failed > 0 ? 1 : 0;
    }

    /// <summary>
    /// WhenChanged on a single property emits the initial value and subsequent changes.
    /// </summary>
    private static void ValidateWhenChangedSingleProperty()
    {
        var vm = new AotViewModel { Name = Alice };
        string? last = null;
        using var sub = vm.WhenChanged(x => x.Name).Subscribe(v => last = v);
        AssertEqual("WhenChanged initial", Alice, last);
        vm.Name = "Bob";
        AssertEqual("WhenChanged after set", "Bob", last);
    }

    /// <summary>
    /// WhenChanged across a deep property chain tracks changes to the leaf value.
    /// </summary>
    private static void ValidateWhenChangedDeepChain()
    {
        var vm = new AotViewModel();
        vm.Child.Value = "Deep";
        string? last = null;
        using var sub = vm.WhenChanged(x => x.Child.Value).Subscribe(v => last = v);
        AssertEqual("WhenChanged deep initial", "Deep", last);
        vm.Child.Value = "Deeper";
        AssertEqual("WhenChanged deep after set", "Deeper", last);
    }

    /// <summary>
    /// WhenChanged on two properties emits a tuple and updates when either changes.
    /// </summary>
    private static void ValidateWhenChangedTwoProperties()
    {
        var vm = new AotViewModel { Name = Alice, Age = InitialAge };
        (string name, int age) last = default;
        using var sub = vm.WhenChanged(x => x.Name, x => x.Age).Subscribe(v => last = v);
        AssertEqual("WhenChanged two-prop name", Alice, last.name);
        AssertEqual("WhenChanged two-prop age", InitialAge, last.age);
        vm.Age = UpdatedAge;
        AssertEqual("WhenChanged two-prop age update", UpdatedAge, last.age);
    }

    /// <summary>
    /// WhenChanged on the view type produces a dispatch entry required by BindTwoWay.
    /// </summary>
    private static void ValidateWhenChangedOnViewType()
    {
        var view = new AotView { DisplayName = "ViewVal" };
        string? last = null;
        using var sub = view.WhenChanged(x => x.DisplayName).Subscribe(v => last = v);
        AssertEqual("WhenChanged on view initial", "ViewVal", last);
        view.DisplayName = UpdatedName;
        AssertEqual("WhenChanged on view after set", UpdatedName, last);
    }

    /// <summary>
    /// BindOneWay propagates source changes to the target property.
    /// </summary>
    private static void ValidateBindOneWay()
    {
        var source = new AotViewModel { Name = SourceName };
        var target = new AotView();
        using var binding = source.BindOneWay(target, x => x.Name, x => x.DisplayName);
        AssertEqual("BindOneWay initial", SourceName, target.DisplayName);
        source.Name = UpdatedName;
        AssertEqual("BindOneWay after set", UpdatedName, target.DisplayName);
    }

    /// <summary>
    /// BindTwoWay propagates changes in both directions between source and target.
    /// </summary>
    private static void ValidateBindTwoWay()
    {
        var source = new AotViewModel { Name = SourceName };
        var target = new AotView();
        using var binding = source.BindTwoWay(target, x => x.Name, x => x.DisplayName);
        AssertEqual("BindTwoWay initial", SourceName, target.DisplayName);
        source.Name = "FromSource";
        AssertEqual("BindTwoWay source→target", "FromSource", target.DisplayName);
        target.DisplayName = "FromTarget";
        AssertEqual("BindTwoWay target→source", "FromTarget", source.Name);
    }

    /// <summary>
    /// BindOneWay stops propagating once the binding is disposed.
    /// </summary>
    private static void ValidateBindOneWayDisposal()
    {
        var source = new AotViewModel { Name = "Before" };
        var target = new AotView();
        var binding = source.BindOneWay(target, x => x.Name, x => x.DisplayName);
        AssertEqual("BindOneWay pre-dispose", "Before", target.DisplayName);
        binding.Dispose();
        source.Name = "After";
        AssertEqual("BindOneWay post-dispose unchanged", "Before", target.DisplayName);
    }

    /// <summary>
    /// Compares an expected and actual value, recording a pass or failure to the console.
    /// </summary>
    /// <typeparam name="T">The value type being compared.</typeparam>
    /// <param name="label">A human-readable label for the scenario.</param>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value produced by the binding.</param>
    private static void AssertEqual<T>(string label, T expected, T? actual)
    {
        if (Equals(expected, actual))
        {
            Console.WriteLine($"  PASS: {label}");
            _passed++;
        }
        else
        {
            Console.WriteLine($"  FAIL: {label} — expected '{expected}', got '{actual}'");
            _failed++;
        }
    }
}
