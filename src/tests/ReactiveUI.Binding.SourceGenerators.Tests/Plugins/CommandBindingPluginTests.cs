// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.SourceGenerators.Models;
using ReactiveUI.Binding.SourceGenerators.Plugins;
using ReactiveUI.Binding.SourceGenerators.Plugins.CommandBinding;
using ReactiveUI.Binding.SourceGenerators.Tests.Helpers;

namespace ReactiveUI.Binding.SourceGenerators.Tests.Plugins;

/// <summary>
/// Unit tests for the <see cref="CommandBindingPluginRegistry"/> and command binding plugins.
/// </summary>
public class CommandBindingPluginTests
{
    /// <summary>
    /// Verifies that GetBestPlugin returns CommandPropertyBindingPlugin when HasCommandProperty is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBestPlugin_HasCommandProperty_ReturnsCommandPropertyPlugin()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasCommandProperty: true,
            resolvedEventName: "Click",
            hasEnabledProperty: true);

        var plugin = CommandBindingPluginRegistry.GetBestPlugin(inv);

        await Assert.That(plugin).IsNotNull();
        await Assert.That(plugin).IsTypeOf<CommandPropertyBindingPlugin>();
        await Assert.That(plugin!.Affinity).IsEqualTo(5);
    }

    /// <summary>
    /// Verifies that GetBestPlugin returns EventEnabledBindingPlugin when event and Enabled exist but no Command.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBestPlugin_EventAndEnabled_ReturnsEventEnabledPlugin()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasCommandProperty: false,
            resolvedEventName: "Click",
            hasEnabledProperty: true);

        var plugin = CommandBindingPluginRegistry.GetBestPlugin(inv);

        await Assert.That(plugin).IsNotNull();
        await Assert.That(plugin).IsTypeOf<EventEnabledBindingPlugin>();
        await Assert.That(plugin!.Affinity).IsEqualTo(4);
    }

    /// <summary>
    /// Verifies that GetBestPlugin returns DefaultEventBindingPlugin when event exists but no Command or Enabled.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBestPlugin_EventOnly_ReturnsDefaultEventPlugin()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasCommandProperty: false,
            resolvedEventName: "Click",
            hasEnabledProperty: false);

        var plugin = CommandBindingPluginRegistry.GetBestPlugin(inv);

        await Assert.That(plugin).IsNotNull();
        await Assert.That(plugin).IsTypeOf<DefaultEventBindingPlugin>();
        await Assert.That(plugin!.Affinity).IsEqualTo(3);
    }

    /// <summary>
    /// Verifies that GetBestPlugin returns null when no plugin can handle the invocation.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBestPlugin_NoEventNoCommand_ReturnsNull()
    {
        var inv = ModelFactory.CreateBindCommandInvocationInfo(
            hasCommandProperty: false,
            resolvedEventName: null,
            hasEnabledProperty: false);

        var plugin = CommandBindingPluginRegistry.GetBestPlugin(inv);

        await Assert.That(plugin).IsNull();
    }

    /// <summary>
    /// Verifies CommandPropertyBindingPlugin.RequiresCustomBinderFallback is false.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CommandPropertyPlugin_RequiresCustomBinderFallback_IsFalse()
    {
        var plugin = new CommandPropertyBindingPlugin();
        await Assert.That(plugin.RequiresCustomBinderFallback).IsFalse();
    }

    /// <summary>
    /// Verifies EventEnabledBindingPlugin.RequiresCustomBinderFallback is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task EventEnabledPlugin_RequiresCustomBinderFallback_IsTrue()
    {
        var plugin = new EventEnabledBindingPlugin();
        await Assert.That(plugin.RequiresCustomBinderFallback).IsTrue();
    }

    /// <summary>
    /// Verifies DefaultEventBindingPlugin.RequiresCustomBinderFallback is true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultEventPlugin_RequiresCustomBinderFallback_IsTrue()
    {
        var plugin = new DefaultEventBindingPlugin();
        await Assert.That(plugin.RequiresCustomBinderFallback).IsTrue();
    }
}
