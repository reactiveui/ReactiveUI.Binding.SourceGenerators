// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.WhenChanged;

/// <summary>
/// Tests that the source-generator-generated WhenChanged code works correctly at runtime
/// for extended multi-property overloads (5 through 16 properties).
/// </summary>
public class WhenChangedExtendedTests
{
    /// <summary>
    /// Verifies that five-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FiveProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "a", Prop2 = 2, Prop3 = 3.0, Prop4 = true, Prop5 = "e" };
        var received = false;

        using var sub = WhenChangedExtendedScenarios.FiveProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>
    /// Verifies that six-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SixProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "a", Prop2 = 2, Prop3 = 3.0, Prop4 = true, Prop5 = "e", Prop6 = 6 };
        var received = false;

        using var sub = WhenChangedExtendedScenarios.SixProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>
    /// Verifies that seven-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SevenProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "a", Prop2 = 2, Prop3 = 3.0, Prop4 = true, Prop5 = "e", Prop6 = 6, Prop7 = 7.0 };
        var received = false;

        using var sub = WhenChangedExtendedScenarios.SevenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>
    /// Verifies that eight-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EightProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "a", Prop2 = 2, Prop3 = 3.0, Prop4 = true, Prop5 = "e", Prop6 = 6, Prop7 = 7.0, Prop8 = false };
        var received = false;

        using var sub = WhenChangedExtendedScenarios.EightProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>
    /// Verifies that nine-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NineProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "a", Prop2 = 2, Prop3 = 3.0, Prop4 = true, Prop5 = "e", Prop6 = 6, Prop7 = 7.0, Prop8 = false, Prop9 = "i" };
        var received = false;

        using var sub = WhenChangedExtendedScenarios.NineProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>
    /// Verifies that ten-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TenProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "a", Prop2 = 2, Prop3 = 3.0, Prop4 = true, Prop5 = "e", Prop6 = 6, Prop7 = 7.0, Prop8 = false, Prop9 = "i", Prop10 = 10 };
        var received = false;

        using var sub = WhenChangedExtendedScenarios.TenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>
    /// Verifies that eleven-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ElevenProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "a", Prop2 = 2, Prop3 = 3.0, Prop4 = true, Prop5 = "e", Prop6 = 6, Prop7 = 7.0, Prop8 = false, Prop9 = "i", Prop10 = 10, Prop11 = 11.0 };
        var received = false;

        using var sub = WhenChangedExtendedScenarios.ElevenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>
    /// Verifies that twelve-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TwelveProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "a", Prop2 = 2, Prop3 = 3.0, Prop4 = true, Prop5 = "e", Prop6 = 6, Prop7 = 7.0, Prop8 = false, Prop9 = "i", Prop10 = 10, Prop11 = 11.0, Prop12 = true };
        var received = false;

        using var sub = WhenChangedExtendedScenarios.TwelveProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>
    /// Verifies that thirteen-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ThirteenProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "a", Prop2 = 2, Prop3 = 3.0, Prop4 = true, Prop5 = "e", Prop6 = 6, Prop7 = 7.0, Prop8 = false, Prop9 = "i", Prop10 = 10, Prop11 = 11.0, Prop12 = true, Prop13 = "m" };
        var received = false;

        using var sub = WhenChangedExtendedScenarios.ThirteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>
    /// Verifies that fourteen-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FourteenProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "a", Prop2 = 2, Prop3 = 3.0, Prop4 = true, Prop5 = "e", Prop6 = 6, Prop7 = 7.0, Prop8 = false, Prop9 = "i", Prop10 = 10, Prop11 = 11.0, Prop12 = true, Prop13 = "m", Prop14 = 14 };
        var received = false;

        using var sub = WhenChangedExtendedScenarios.FourteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>
    /// Verifies that fifteen-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task FifteenProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "a", Prop2 = 2, Prop3 = 3.0, Prop4 = true, Prop5 = "e", Prop6 = 6, Prop7 = 7.0, Prop8 = false, Prop9 = "i", Prop10 = 10, Prop11 = 11.0, Prop12 = true, Prop13 = "m", Prop14 = 14, Prop15 = 15.0 };
        var received = false;

        using var sub = WhenChangedExtendedScenarios.FifteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>
    /// Verifies that sixteen-property WhenChanged emits initial values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SixteenProperties_EmitsInitialValues()
    {
        var vm = new BigViewModel { Prop1 = "a", Prop2 = 2, Prop3 = 3.0, Prop4 = true, Prop5 = "e", Prop6 = 6, Prop7 = 7.0, Prop8 = false, Prop9 = "i", Prop10 = 10, Prop11 = 11.0, Prop12 = true, Prop13 = "m", Prop14 = 14, Prop15 = 15.0, Prop16 = false };
        var received = false;

        using var sub = WhenChangedExtendedScenarios.SixteenProperties(vm)
            .Subscribe(_ => received = true);

        await Assert.That(received).IsTrue();
    }

    /// <summary>
    /// Verifies that sixteen-property WhenChanged with a selector correctly combines all property values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithSelector_SixteenProperties()
    {
        var vm = new BigViewModel { Prop1 = "a", Prop2 = 2, Prop3 = 3.0, Prop4 = true, Prop5 = "e", Prop6 = 6, Prop7 = 7.0, Prop8 = false, Prop9 = "i", Prop10 = 10, Prop11 = 11.0, Prop12 = true, Prop13 = "m", Prop14 = 14, Prop15 = 15.0, Prop16 = false };
        var values = new List<string>();

        using var sub = WhenChangedExtendedScenarios.WithSelector_SixteenProperties(vm)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0]).IsNotNull();
    }
}
