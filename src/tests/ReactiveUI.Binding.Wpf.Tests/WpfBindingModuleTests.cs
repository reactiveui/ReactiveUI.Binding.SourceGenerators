// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat;

namespace ReactiveUI.Binding.Wpf.Tests;

/// <summary>
/// Tests for the WPF platform module. Compiled twice: once against ReactiveUI.Binding.Wpf and once, under
/// REACTIVE_SHIM, against ReactiveUI.Binding.Reactive.Wpf, so both leaves are exercised by the same
/// assertions.
/// </summary>
public class WpfBindingModuleTests
{
    /// <summary>Verifies that Configure registers the dependency-property observation factory.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_RegistersDependencyPropertyObservation()
    {
        var resolver = new ModernDependencyResolver();

        new WpfBindingModule().Configure(resolver);

        var factories = resolver.GetServices<ICreatesObservableForProperty>().ToList();

        await Assert.That(factories.Exists(static f => f is DependencyObjectObservableForProperty)).IsTrue();
    }

    /// <summary>Verifies that Configure registers the resolver naming the thread a view belongs to.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_RegistersTheViewThreadResolver()
    {
        var resolver = new ModernDependencyResolver();

        new WpfBindingModule().Configure(resolver);

        var resolvers = resolver.GetServices<IViewThreadResolver>().ToList();

        await Assert.That(resolvers.Exists(static r => r is DispatcherViewThreadResolver)).IsTrue();
    }

    /// <summary>Verifies that Configure rejects a null resolver rather than failing later.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Configure_NullResolver_ThrowsArgumentNullException()
    {
        var module = new WpfBindingModule();

        await Assert.That(() => module.Configure(null!)).ThrowsExactly<ArgumentNullException>();
    }
}
