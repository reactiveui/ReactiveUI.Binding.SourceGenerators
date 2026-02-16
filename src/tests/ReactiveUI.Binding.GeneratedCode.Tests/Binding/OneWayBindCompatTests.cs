// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>
/// Tests that the OneWayBind compat alias (view-first syntax) works correctly at runtime.
/// </summary>
public class OneWayBindCompatTests
{
    /// <summary>
    /// Verifies that OneWayBind syncs the initial value from view model to view.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_SyncsInitialValue()
    {
        var vm = new TestViewModel { Name = "Hello" };
        var view = new TestView();

        using var binding = OneWayBindCompatScenarios.StringProperty(view, vm);

        await Assert.That(view.DisplayName).IsEqualTo("Hello");
    }

    /// <summary>
    /// Verifies that OneWayBind syncs changes from view model to view.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_SyncsOnSourceChange()
    {
        var vm = new TestViewModel { Name = "Hello" };
        var view = new TestView();

        using var binding = OneWayBindCompatScenarios.StringProperty(view, vm);

        vm.Name = "World";

        await Assert.That(view.DisplayName).IsEqualTo("World");
    }

    /// <summary>
    /// Verifies that disposing the OneWayBind binding stops syncing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OneWayBind_Disposal_StopsSyncing()
    {
        var vm = new TestViewModel { Name = "Hello" };
        var view = new TestView();

        var binding = OneWayBindCompatScenarios.StringProperty(view, vm);
        binding.Dispose();

        vm.Name = "AfterDisposal";

        await Assert.That(view.DisplayName).IsEqualTo("Hello");
    }
}
