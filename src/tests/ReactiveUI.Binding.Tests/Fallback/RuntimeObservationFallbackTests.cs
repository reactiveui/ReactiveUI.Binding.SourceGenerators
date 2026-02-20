// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.Mixins;
using ReactiveUI.Binding.Tests.TestModels;

namespace ReactiveUI.Binding.Tests.Fallback;

/// <summary>
/// Tests for the <see cref="RuntimeObservationFallback"/> class.
/// </summary>
public class RuntimeObservationFallbackTests
{
    /// <summary>
    /// Verifies that WhenChanged emits the initial value and subsequent changes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_SingleProperty_EmitsInitialAndChanges()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = "Initial" };
        var values = new List<string>();

        using var sub = RuntimeObservationFallback.WhenChanged(
            vm,
            x => x.Name)
            .Subscribe(values.Add);

        vm.Name = "Changed";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values[0]).IsEqualTo("Initial");
        await Assert.That(values[1]).IsEqualTo("Changed");
    }

    /// <summary>
    /// Verifies that WhenChanging emits before-change notifications.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanging_SingleProperty_EmitsBeforeChange()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = "Initial" };
        var values = new List<string>();

        using var sub = RuntimeObservationFallback.WhenChanging(
            vm,
            x => x.Name)
            .Subscribe(values.Add);

        vm.Name = "Changed";

        // Before-change should emit the old value at the time of notification
        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Verifies that WhenAnyValue emits the initial and subsequent values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenAnyValue_SingleProperty_EmitsValues()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = "Start" };
        var values = new List<string>();

        using var sub = RuntimeObservationFallback.WhenAnyValue(
            vm,
            x => x.Name)
            .Subscribe(values.Add);

        vm.Name = "End";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values[0]).IsEqualTo("Start");
    }

    /// <summary>
    /// Verifies that WhenChanged with two properties emits tuples.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_TwoProperties_EmitsTuples()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = "Alice", Age = 30 };
        var values = new List<(string Value1, int Value2)>();

        using var sub = RuntimeObservationFallback.WhenChanged(
            vm,
            x => x.Name,
            x => x.Age)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Value1).IsEqualTo("Alice");
        await Assert.That(values[0].Value2).IsEqualTo(30);
    }

    /// <summary>
    /// Verifies that WhenChanged with three properties emits tuples.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ThreeProperties_EmitsTuples()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = "Alice", Age = 30, Address = new TestAddress { City = "Seattle" } };
        var values = new List<(string Value1, int Value2, string? Value3)>();

        using var sub = RuntimeObservationFallback.WhenChanged(
            vm,
            x => x.Name,
            x => x.Age,
            x => x.Address!.City)
            .Subscribe(values.Add);

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(values[0].Value1).IsEqualTo("Alice");
        await Assert.That(values[0].Value2).IsEqualTo(30);
        await Assert.That(values[0].Value3).IsEqualTo("Seattle");
    }

    /// <summary>
    /// Verifies that WhenChanged emits after property changes with three properties.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WhenChanged_ThreeProperties_UpdatesOnChange()
    {
        EnsureInitialized();

        var vm = new TestViewModel { Name = "Alice", Age = 30, Address = new TestAddress { City = "Seattle" } };
        var values = new List<(string Value1, int Value2, string? Value3)>();

        using var sub = RuntimeObservationFallback.WhenChanged(
            vm,
            x => x.Name,
            x => x.Age,
            x => x.Address!.City)
            .Subscribe(values.Add);

        vm.Name = "Bob";

        await Assert.That(values.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(values[^1].Value1).IsEqualTo("Bob");
    }

    internal static void EnsureInitialized()
    {
        RxBindingBuilder.ResetForTesting();
        RxBindingBuilder.CreateReactiveUIBindingBuilder()
            .WithCoreServices()
            .BuildApp();
    }
}
