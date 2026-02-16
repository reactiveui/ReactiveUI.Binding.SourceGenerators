// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.GeneratedCode.TestModels.Scenarios;
using ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

namespace ReactiveUI.Binding.GeneratedCode.Tests.Binding;

/// <summary>
/// Tests that the Bind compat alias (view-first two-way syntax) works correctly at runtime.
/// </summary>
public class BindCompatTests
{
    /// <summary>
    /// Verifies that Bind syncs the initial value from view model to view.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_SyncsInitialValue()
    {
        var vm = new TestViewModel { Name = "Hello" };
        var view = new TestView();

        using var binding = BindCompatScenarios.StringProperty(view, vm);

        await Assert.That(view.DisplayName).IsEqualTo("Hello");
    }

    /// <summary>
    /// Verifies that Bind syncs changes from view model to view.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_SyncsSourceToView()
    {
        var vm = new TestViewModel { Name = "Hello" };
        var view = new TestView();

        using var binding = BindCompatScenarios.StringProperty(view, vm);

        vm.Name = "World";

        await Assert.That(view.DisplayName).IsEqualTo("World");
    }

    /// <summary>
    /// Verifies that Bind syncs changes from view back to view model.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_SyncsViewToSource()
    {
        var vm = new TestViewModel { Name = "Hello" };
        var view = new TestView();

        using var binding = BindCompatScenarios.StringProperty(view, vm);

        view.DisplayName = "FromView";

        await Assert.That(vm.Name).IsEqualTo("FromView");
    }

    /// <summary>
    /// Verifies that disposing the Bind binding stops syncing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Bind_Disposal_StopsSyncing()
    {
        var vm = new TestViewModel { Name = "Hello" };
        var view = new TestView();

        var binding = BindCompatScenarios.StringProperty(view, vm);
        binding.Dispose();

        vm.Name = "AfterDisposal";

        await Assert.That(view.DisplayName).IsEqualTo("Hello");
    }
}
