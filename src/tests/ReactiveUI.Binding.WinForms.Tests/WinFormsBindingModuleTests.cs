// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat;

namespace ReactiveUI.Binding.WinForms.Tests;

/// <summary>
/// Tests for the WinForms platform module. Compiled twice: once against ReactiveUI.Binding.WinForms and once,
/// under REACTIVE_SHIM, against ReactiveUI.Binding.Reactive.WinForms, so both leaves are exercised by the same
/// assertions.
/// </summary>
public class WinFormsBindingModuleTests
{
    /// <summary>Verifies that Configure registers the event-based observation factory.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_RegistersEventBasedObservation()
    {
        var resolver = new ModernDependencyResolver();

        new WinFormsBindingModule().Configure(resolver);

        var factories = resolver.GetServices<ICreatesObservableForProperty>().ToList();

        await Assert.That(factories.Exists(static f => f is WinFormsCreatesObservableForProperty)).IsTrue();
    }

    /// <summary>Verifies that Configure registers the resolver naming the thread a control belongs to.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_RegistersTheViewThreadResolver()
    {
        var resolver = new ModernDependencyResolver();

        new WinFormsBindingModule().Configure(resolver);

        var resolvers = resolver.GetServices<IViewThreadResolver>().ToList();

        await Assert.That(resolvers.Exists(static r => r is ControlViewThreadResolver)).IsTrue();
    }

    /// <summary>Verifies that Configure rejects a null resolver rather than failing later.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_NullResolver_ThrowsArgumentNullException()
    {
        var module = new WinFormsBindingModule();

        await Assert.That(() => module.Configure(null!)).ThrowsExactly<ArgumentNullException>();
    }
}
