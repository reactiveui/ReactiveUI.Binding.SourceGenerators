// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;

using ReactiveUI.Binding;
using ReactiveUI.Binding.AotValidation;

var passed = 0;
var failed = 0;

// 1. WhenChanged — single property
{
    var vm = new AotViewModel { Name = "Alice" };
    string? last = null;
    using var sub = vm.WhenChanged(x => x.Name).Subscribe(v => last = v);
    AssertEqual("WhenChanged initial", "Alice", last);
    vm.Name = "Bob";
    AssertEqual("WhenChanged after set", "Bob", last);
}

// 2. WhenChanged — deep chain
{
    var vm = new AotViewModel();
    vm.Child.Value = "Deep";
    string? last = null;
    using var sub = vm.WhenChanged(x => x.Child.Value).Subscribe(v => last = v);
    AssertEqual("WhenChanged deep initial", "Deep", last);
    vm.Child.Value = "Deeper";
    AssertEqual("WhenChanged deep after set", "Deeper", last);
}

// 3. WhenChanged — two properties
{
    var vm = new AotViewModel { Name = "Alice", Age = 30 };
    (string name, int age) last = default;
    using var sub = vm.WhenChanged(x => x.Name, x => x.Age).Subscribe(v => last = v);
    AssertEqual("WhenChanged two-prop name", "Alice", last.name);
    AssertEqual("WhenChanged two-prop age", 30, last.age);
    vm.Age = 31;
    AssertEqual("WhenChanged two-prop age update", 31, last.age);
}

// 4. WhenChanged on view type (required for BindTwoWay — the generator needs a
//    WhenChanged call site on AotView to produce a dispatch entry for it)
{
    var view = new AotView { DisplayName = "ViewVal" };
    string? last = null;
    using var sub = view.WhenChanged(x => x.DisplayName).Subscribe(v => last = v);
    AssertEqual("WhenChanged on view initial", "ViewVal", last);
    view.DisplayName = "Updated";
    AssertEqual("WhenChanged on view after set", "Updated", last);
}

// 5. BindOneWay
{
    var source = new AotViewModel { Name = "Source" };
    var target = new AotView();
    using var binding = source.BindOneWay(target, x => x.Name, x => x.DisplayName);
    AssertEqual("BindOneWay initial", "Source", target.DisplayName);
    source.Name = "Updated";
    AssertEqual("BindOneWay after set", "Updated", target.DisplayName);
}

// 6. BindTwoWay
{
    var source = new AotViewModel { Name = "Source" };
    var target = new AotView();
    using var binding = source.BindTwoWay(target, x => x.Name, x => x.DisplayName);
    AssertEqual("BindTwoWay initial", "Source", target.DisplayName);
    source.Name = "FromSource";
    AssertEqual("BindTwoWay source→target", "FromSource", target.DisplayName);
    target.DisplayName = "FromTarget";
    AssertEqual("BindTwoWay target→source", "FromTarget", source.Name);
}

// 7. BindOneWay disposal
{
    var source = new AotViewModel { Name = "Before" };
    var target = new AotView();
    var binding = source.BindOneWay(target, x => x.Name, x => x.DisplayName);
    AssertEqual("BindOneWay pre-dispose", "Before", target.DisplayName);
    binding.Dispose();
    source.Name = "After";
    AssertEqual("BindOneWay post-dispose unchanged", "Before", target.DisplayName);
}

Console.WriteLine();
Console.WriteLine($"AOT Validation: {passed} passed, {failed} failed");
return failed > 0 ? 1 : 0;

void AssertEqual<T>(string label, T expected, T? actual)
{
    if (Equals(expected, actual))
    {
        Console.WriteLine($"  PASS: {label}");
        passed++;
    }
    else
    {
        Console.WriteLine($"  FAIL: {label} — expected '{expected}', got '{actual}'");
        failed++;
    }
}
